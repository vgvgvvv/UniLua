using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ResetCore.CodeDom;
using UniLua;

namespace UniToLuaGener
{
    public class ExportToLua
    {
        public string outputPath;
        public string dllPath;

        private readonly Dictionary<Type, string> typeNameMap = new Dictionary<Type, string>()
        {
            [typeof(string)] = "String",
            [typeof(short)] = "Integer",
            [typeof(int)] = "Integer",
            [typeof(long)] = "Integer",
            [typeof(ushort)] = "Unsigned",
            [typeof(uint)] = "Unsigned",
            [typeof(float)] = "Number",
            [typeof(double)] = "Double",
            [typeof(bool)] = "Boolean",
            [typeof(ulong)] = "UInt64"
        };

        public void GenAll(Assembly target)
        {
            List<Type> targetTypeList = GetTargetType(target);
            GenWrapper(targetTypeList);
            GenBinder(targetTypeList);
        }

        private List<Type> GetTargetType(Assembly target)
        {
            var types = target.GetTypes().Where((t) =>
            {
                var toluaAttr = t.GetCustomAttribute<ToLuaAttribute>();
                return toluaAttr != null;
            });
            return types.ToList();
        }

        public void GenBinder(List<Type> targetTypeList)
        {

        }

        public void GenWrapper(List<Type> targetTypeList)
        {
            foreach (var type in targetTypeList)
            {
                if (type.IsEnum)
                {
                    GenEnum(type);
                }
                else if (type.IsInterface)
                {
                    Logger.Error("Cannot Gen Interface Wrap");    
                }
                else if (type.IsSealed && type.IsAbstract)
                {
                    GenStaticLib(type);
                }
                else
                {
                    GenClass(type);
                }
            }
        }

        #region Enum

        private void GenEnum(Type enumType)
        {
            if (enumType == null)
                return;
            var className = enumType.FullName?.Replace(".", "_") + "Wrap";
            var enumNames = enumType.GetEnumNames();

            CodeGener gener = new CodeGener("UniToLua", className);

            List<CodeStatement> registerMethodStatement = new List<CodeStatement>();
            registerMethodStatement.Add(new CodeSnippetStatement($"L.BeginEnum(\"{enumType.Name}\");"));
            foreach (var enumName in enumNames)
            {
                registerMethodStatement.Add(new CodeSnippetStatement($"L.RegVar({enumName}, get_{enumName}, null)"));
            }
            registerMethodStatement.Add(new CodeSnippetStatement($"L.EndEnum()"));

            gener.AddMemberMethod(typeof(void), "Register", new Dictionary<string, Type>() { { "L", typeof(ILuaState) } },
                MemberAttributes.Public | MemberAttributes.Static, registerMethodStatement.ToArray());

            foreach (var enumName in enumNames)
            {
                GenRegEnum(gener, enumType, enumName);
            }

            gener.GenCSharp(outputPath);
        }

        private void GenRegEnum(CodeGener gener, Type enumType, string enumName)
        {
            gener.AddMemberMethod(typeof(int), $"get_{enumName}", new Dictionary<string, Type>() { { "L", typeof(ILuaState) } },
                MemberAttributes.Private, new CodeSnippetStatement[]
                {
                    new CodeSnippetStatement($"L.PushLightUserData({enumType.FullName}.{enumName});"),
                    new CodeSnippetStatement("return 1;"),
                });
        }

        #endregion


        #region StaticLib

        private void GenStaticLib(Type libType)
        {
            if (libType == null)
                return;
            var className = libType.FullName?.Replace(".", "_") + "Wrap";
            CodeGener gener = new CodeGener("UniToLua", className);

            var fields = libType.GetFields(BindingFlags.Public | BindingFlags.Static);
            var propertys = libType.GetProperties(BindingFlags.Public | BindingFlags.Static);
            var methods = libType.GetMethods(BindingFlags.Public | BindingFlags.Static);

            List<CodeStatement> registerMethodStatement = new List<CodeStatement>();
            registerMethodStatement.Add(new CodeSnippetStatement($"L.BeginStaticLib(\"{libType.Name}\");"));

            foreach (var fieldInfo in fields)
            {
                registerMethodStatement.Add(new CodeSnippetStatement($"L.RegVar(\"{fieldInfo.Name}\", get_{fieldInfo.Name}, set_{fieldInfo.Name});"));
            }

            foreach (var propertyInfo in propertys)
            {
                StringBuilder builder = new StringBuilder($"L.RegVar(\"{propertyInfo.Name}\", ");
                if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
                {
                    builder.Append($"get_{propertyInfo.Name}");
                }
                else
                {
                    builder.Append("null, ");
                }
                if (propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsPublic)
                {
                    builder.Append($"set_{propertyInfo.Name}");
                }
                else
                {
                    builder.Append("null");
                }
                builder.Append(");");
                registerMethodStatement.Add(new CodeSnippetStatement(builder.ToString()));
            }

            foreach (var methodInfo in methods)
            {
                registerMethodStatement.Add(new CodeSnippetStatement($"L.RegFunction(\"{methodInfo.Name}\", {methodInfo.Name})"));
            }

            registerMethodStatement.Add(new CodeSnippetStatement($"L.EndStaticLib()"));

            gener.AddMemberMethod(typeof(void), "Register", new Dictionary<string, Type>() { { "L", typeof(ILuaState) } },
                MemberAttributes.Public | MemberAttributes.Static, registerMethodStatement.ToArray());

            foreach (var fieldInfo in fields)
            {
                GenRegStaticField(gener, libType, fieldInfo);
            }

            foreach (var propertyInfo in propertys)
            {
                GenRegStaticProperty(gener, libType, propertyInfo);
            }

            foreach (var methodInfo in methods)
            {
                GenRegStaticFunction(gener, libType, methodInfo);
            }

            gener.GenCSharp(outputPath);
        }

        #endregion

        #region Class

        private void GenClass(Type classType)
        {
            if (classType == null)
                return;
            var className = classType.FullName?.Replace(".", "_") + "Wrap";
            CodeGener gener = new CodeGener("UniToLua", className);

            var fields = classType.GetFields(BindingFlags.Public);
            var propertys = classType.GetProperties(BindingFlags.Public);
            var methods = classType.GetMethods(BindingFlags.Public);

            List<CodeStatement> registerMethodStatement = new List<CodeStatement>();

        }

        #endregion



        private void GenConstructor(CodeGener gener, ConstructorInfo constructor)
        {
            //TODO
        }

        private void GenRegStaticField(CodeGener gener, Type type, FieldInfo fieldInfo)
        {
            var temp = new List<CodeStatement>();
            temp.AddRange(new List<CodeStatement>()
            {
                new CodeSnippetStatement($"L.{GetPushString(fieldInfo.FieldType)}({type.FullName}.{fieldInfo.Name});"),
                new CodeSnippetStatement($"return 1;")
            });

            gener.AddMemberMethod(typeof(int), $"get_{fieldInfo.Name}",
                new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private, temp.ToArray());

            temp.Clear();
            temp.AddRange(new List<CodeStatement>()
            {
                new CodeSnippetStatement($"var value = L.{GetCheckString(fieldInfo.FieldType)}(-1);"),
                new CodeSnippetStatement($"{type.FullName}.{fieldInfo.Name} = value;"),
                new CodeSnippetStatement($"return 0;")
            });

            gener.AddMemberMethod(typeof(int), $"set_{fieldInfo.Name}",
                new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private, temp.ToArray());
        }

        private void GenRegStaticProperty(CodeGener gener, Type type, PropertyInfo propertyInfo)
        {
            var temp = new List<CodeStatement>();
            if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
            {
                temp.AddRange(new List<CodeStatement>()
                {
                    new CodeSnippetStatement($"L.{GetPushString(propertyInfo.PropertyType)}({type.FullName}.{propertyInfo.Name});"),
                    new CodeSnippetStatement($"return 1;")
                });

                gener.AddMemberMethod(typeof(int), $"get_{propertyInfo.Name}",
                    new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private,
                    temp.ToArray());
            }

            if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
            {
                temp.Clear();
                temp.AddRange(new List<CodeStatement>()
                {
                    new CodeSnippetStatement($"var value = L.{GetCheckString(propertyInfo.PropertyType)}(-1);"),
                    new CodeSnippetStatement($"{type.FullName}.{propertyInfo.Name} = value;"),
                    new CodeSnippetStatement($"return 0;")
                });

                gener.AddMemberMethod(typeof(int), $"set_{propertyInfo.Name}",
                    new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private,
                    temp.ToArray());
            }
        }

        private void GenRegStaticFunction(CodeGener gener, Type type, MethodInfo methodInfo)
        {
            var temp = new List<CodeStatement>();
            var paramInfos = methodInfo.GetParameters();
            var returnParameterInfo = methodInfo.ReturnParameter;

            for (int i = 1; i <= paramInfos.Length; i++)
            {
                var paramInfo = paramInfos[i - 1];
                temp.Add(new CodeSnippetStatement($"var arg{i} = L.{GetCheckString(paramInfo.ParameterType)}(-{i});"));
            }

            var paramBuilder = new StringBuilder();
            for (int i = 1; i <= paramInfos.Length; i++)
            {
                var paramInfo = paramInfos[i - 1];
                if (i != 1)
                {
                    paramBuilder.Append(", ");
                }
                paramBuilder.Append(paramInfo.Name);
            }

            if (methodInfo.ReturnType == typeof(void))
            {
                temp.Add(new CodeSnippetStatement($"L.{type.FullName}.{methodInfo.Name}({paramBuilder});"));
                temp.Add(new CodeSnippetStatement("return 0;"));
            }
            else
            {
                temp.Add(new CodeSnippetStatement($"var result = L.{type.FullName}.{methodInfo.Name}({paramBuilder});"));
                temp.Add(new CodeSnippetStatement($"L.{GetPushString(methodInfo.ReturnType)}(result);"));
                temp.Add(new CodeSnippetStatement("return 1;"));
            }


            gener.AddMemberMethod(typeof(int), methodInfo.Name,
                new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private,
                temp.ToArray());
        }

        private void GenRegMemberField(CodeGener gener, Type type, FieldInfo fieldInfo)
        {
            
        }

        private void GenRegMemberProperty(CodeGener gener, Type type, PropertyInfo propertyInfo)
        {
            
        }

        private void GenRegMemberFunction(CodeGener gener, Type type, MethodInfo methodInfo)
        {
            
        }

        /// <summary>
        /// 获取Push方法对应的String
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetPushString(Type type)
        {
            if (!typeNameMap.TryGetValue(type, out var typeName))
            {
                return "PushObject";
            }
            return $"Push{typeName}";
        }

        /// <summary>
        /// 获取Check方法对应的String
        /// </summary>
        /// <returns></returns>
        private string GetCheckString(Type type)
        {
            if (!typeNameMap.TryGetValue(type, out var typeName))
            {
                return "CheckObject";
            }
            return $"Check{typeName}";
        }

        /// <summary>
        /// 获取To方法对应的String
        /// </summary>
        /// <returns></returns>
        private string GetToString(Type type)
        {
            if (!typeNameMap.TryGetValue(type, out var typeName))
            {
                return "ToObject";
            }
            return $"To{typeName}";
        }
    }
}