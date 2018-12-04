﻿using System;
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

        public void GenAll()
        {
            var target = Assembly.LoadFile(dllPath);
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
            registerMethodStatement.Add(new CodeSnippetStatement($"\t\t\tL.BeginEnum(\"{enumType.Name}\");"));
            foreach (var enumName in enumNames)
            {
                registerMethodStatement.Add(new CodeSnippetStatement($"\t\t\tL.RegVar({enumName}, get_{enumName}, null);"));
            }
            registerMethodStatement.Add(new CodeSnippetStatement($"\t\t\tL.EndEnum();"));

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
                    new CodeSnippetStatement($"\t\t\tL.PushLightUserData({enumType.FullName}.{enumName});"),
                    new CodeSnippetStatement("\t\t\treturn 1;"),
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
            registerMethodStatement.Add(new CodeSnippetStatement($"\t\t\tL.BeginStaticLib(\"{libType.Name}\");"));

            foreach (var fieldInfo in fields)
            {
                registerMethodStatement.Add(new CodeSnippetStatement($"\t\t\tL.RegVar(\"{fieldInfo.Name}\", get_{fieldInfo.Name}, set_{fieldInfo.Name});"));
            }

            foreach (var propertyInfo in propertys)
            {
                StringBuilder builder = new StringBuilder($"\t\t\tL.RegVar(\"{propertyInfo.Name}\", ");
                if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
                {
                    builder.Append($"get_{propertyInfo.Name}, ");
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
                registerMethodStatement.Add(new CodeSnippetStatement($"\t\t\tL.RegFunction(\"{methodInfo.Name}\", {methodInfo.Name});"));
            }

            registerMethodStatement.Add(new CodeSnippetStatement($"\t\t\tL.EndStaticLib();"));

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

            var fields = classType.GetFields();
            var propertys = classType.GetProperties();
            var methods = classType.GetMethods();

            List<CodeStatement> registerMethodStatement = new List<CodeStatement>();

            registerMethodStatement.Add(new CodeSnippetStatement($"\t\t\tL.BeginClass(\"{classType.Name}\");"));

            registerMethodStatement.Add(new CodeSnippetStatement($"\t\t\tL.RegFunction(\"New\", _Create{classType.Name});"));

            foreach (var fieldInfo in fields)
            {
                registerMethodStatement.Add(new CodeSnippetStatement($"\t\t\tL.RegVar(\"{fieldInfo.Name}\", get_{fieldInfo.Name}, set_{fieldInfo.Name});"));
            }

            foreach (var propertyInfo in propertys)
            {
                StringBuilder builder = new StringBuilder($"\t\t\tL.RegVar(\"{propertyInfo.Name}\", ");
                if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
                {
                    builder.Append($"get_{propertyInfo.Name}, ");
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
                registerMethodStatement.Add(new CodeSnippetStatement($"\t\t\tL.RegFunction(\"{methodInfo.Name}\", {methodInfo.Name});"));
            }

            registerMethodStatement.Add(new CodeSnippetStatement("\t\t\tL.EndClass();"));

            gener.AddMemberMethod(typeof(void), "Register", new Dictionary<string, Type>() { { "L", typeof(ILuaState) } },
                MemberAttributes.Public | MemberAttributes.Static, registerMethodStatement.ToArray());

            GenConstructor(gener, classType);

            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.IsStatic)
                {
                    GenRegStaticField(gener, classType, fieldInfo);
                }
                else
                {
                    GenRegMemberField(gener, classType, fieldInfo);
                }
            }

            foreach (var propertyInfo in propertys)
            {
                if ((propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsStatic) || 
                    (propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsStatic))
                {
                    GenRegStaticProperty(gener, classType, propertyInfo);
                }
                else
                {
                    GenRegMemberProperty(gener, classType, propertyInfo);
                }
                
            }

            foreach (var methodInfo in methods)
            {
                if (methodInfo.IsStatic)
                {
                    GenRegStaticFunction(gener, classType, methodInfo);
                }
                else
                {
                    GenRegMemberFunction(gener, classType, methodInfo);
                }
                
            }

            gener.GenCSharp(outputPath);

        }

        #endregion



        private void GenConstructor(CodeGener gener, Type type)
        {
            //TODO
            var constructorInfos = type.GetConstructors();
        }

        private void GenRegStaticField(CodeGener gener, Type type, FieldInfo fieldInfo)
        {
            var temp = new List<CodeStatement>();
            temp.AddRange(new List<CodeStatement>()
            {
                new CodeSnippetStatement($"\t\t\tL.{GetPushString(fieldInfo.FieldType)}({type.FullName}.{fieldInfo.Name});"),
                new CodeSnippetStatement($"\t\t\treturn 1;")
            });

            gener.AddMemberMethod(typeof(int), $"get_{fieldInfo.Name}",
                new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private | MemberAttributes.Static, temp.ToArray());

            temp.Clear();
            temp.AddRange(new List<CodeStatement>()
            {
                new CodeSnippetStatement($"\t\t\tvar value = L.{GetCheckString(fieldInfo.FieldType)}(1);"),
                new CodeSnippetStatement($"\t\t\t{type.FullName}.{fieldInfo.Name} = value;"),
                new CodeSnippetStatement($"\t\t\treturn 0;")
            });

            gener.AddMemberMethod(typeof(int), $"set_{fieldInfo.Name}",
                new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private | MemberAttributes.Static, temp.ToArray());
        }

        private void GenRegStaticProperty(CodeGener gener, Type type, PropertyInfo propertyInfo)
        {
            var temp = new List<CodeStatement>();
            if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
            {
                temp.AddRange(new List<CodeStatement>()
                {
                    new CodeSnippetStatement($"\t\t\tL.{GetPushString(propertyInfo.PropertyType)}({type.FullName}.{propertyInfo.Name});"),
                    new CodeSnippetStatement($"\t\t\treturn 1;")
                });

                gener.AddMemberMethod(typeof(int), $"get_{propertyInfo.Name}",
                    new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private | MemberAttributes.Static,
                    temp.ToArray());
            }

            if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
            {
                temp.Clear();
                temp.AddRange(new List<CodeStatement>()
                {
                    new CodeSnippetStatement($"\t\t\tvar value = L.{GetCheckString(propertyInfo.PropertyType)}(1);"),
                    new CodeSnippetStatement($"\t\t\t{type.FullName}.{propertyInfo.Name} = value;"),
                    new CodeSnippetStatement($"\t\t\treturn 0;")
                });

                gener.AddMemberMethod(typeof(int), $"set_{propertyInfo.Name}",
                    new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private | MemberAttributes.Static,
                    temp.ToArray());
            }
        }

        private void GenRegStaticFunction(CodeGener gener, Type type, MethodInfo methodInfo)
        {
            var temp = new List<CodeStatement>();
            var paramInfos = methodInfo.GetParameters();

            for (int i = 1; i <= paramInfos.Length; i++)
            {
                var paramInfo = paramInfos[i - 1];
                temp.Add(new CodeSnippetStatement($"\t\t\tvar arg{i} = L.{GetCheckString(paramInfo.ParameterType)}({i});"));
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
                temp.Add(new CodeSnippetStatement($"\t\t\t{type.FullName}.{methodInfo.Name}({paramBuilder});"));
                temp.Add(new CodeSnippetStatement("\t\t\treturn 0;"));
            }
            else
            {
                temp.Add(new CodeSnippetStatement($"\t\t\tvar result = {type.FullName}.{methodInfo.Name}({paramBuilder});"));
                temp.Add(new CodeSnippetStatement($"\t\t\tL.{GetPushString(methodInfo.ReturnType)}(result);"));
                temp.Add(new CodeSnippetStatement("\t\t\treturn 1;"));
            }


            gener.AddMemberMethod(typeof(int), methodInfo.Name,
                new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private | MemberAttributes.Static,
                temp.ToArray());
        }

        private void GenRegMemberField(CodeGener gener, Type type, FieldInfo fieldInfo)
        {
            var temp = new List<CodeStatement>();
            temp.AddRange(new List<CodeStatement>()
            {
                new CodeSnippetStatement($"\t\t\tvar obj = ({type.FullName}) L.ToObject(1);"),
                new CodeSnippetStatement($"\t\t\tL.{GetPushString(fieldInfo.FieldType)}(obj.{fieldInfo.Name});"),
                new CodeSnippetStatement($"\t\t\treturn 1;")
            });

            gener.AddMemberMethod(typeof(int), $"get_{fieldInfo.Name}",
                new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private | MemberAttributes.Static, temp.ToArray());

            temp.Clear();
            temp.AddRange(new List<CodeStatement>()
            {
                new CodeSnippetStatement($"\t\t\tvar obj = ({type.FullName}) L.ToObject(1);"),
                new CodeSnippetStatement($"\t\t\tvar value = L.{GetCheckString(fieldInfo.FieldType)}(2);"),
                new CodeSnippetStatement($"\t\t\tobj.{fieldInfo.Name} = value;"),
                new CodeSnippetStatement($"\t\t\treturn 0;")
            });

            gener.AddMemberMethod(typeof(int), $"set_{fieldInfo.Name}",
                new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private | MemberAttributes.Static, temp.ToArray());
        }

        private void GenRegMemberProperty(CodeGener gener, Type type, PropertyInfo propertyInfo)
        {
            var temp = new List<CodeStatement>();
            if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
            {
                temp.AddRange(new List<CodeStatement>()
                {
                    new CodeSnippetStatement($"\t\t\tvar obj = ({type.FullName}) L.ToObject(1);"),
                    new CodeSnippetStatement($"\t\t\tL.{GetPushString(propertyInfo.PropertyType)}(obj.{propertyInfo.Name});"),
                    new CodeSnippetStatement($"\t\t\treturn 1;")
                });

                gener.AddMemberMethod(typeof(int), $"get_{propertyInfo.Name}",
                    new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private | MemberAttributes.Static,
                    temp.ToArray());
            }

            if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic)
            {
                temp.Clear();
                temp.AddRange(new List<CodeStatement>()
                {
                    new CodeSnippetStatement($"\t\t\tvar obj = ({type.FullName}) L.ToObject(1);"),
                    new CodeSnippetStatement($"\t\t\tvar value = L.{GetCheckString(propertyInfo.PropertyType)}(2);"),
                    new CodeSnippetStatement($"\t\t\tobj.{propertyInfo.Name} = value;"),
                    new CodeSnippetStatement($"\t\t\treturn 0;")
                });

                gener.AddMemberMethod(typeof(int), $"set_{propertyInfo.Name}",
                    new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private | MemberAttributes.Static,
                    temp.ToArray());
            }
        }

        private void GenRegMemberFunction(CodeGener gener, Type type, MethodInfo methodInfo)
        {
            var temp = new List<CodeStatement>();
            var paramInfos = methodInfo.GetParameters();

            temp.Add(new CodeSnippetStatement($"\t\t\tvar obj = ({type.FullName}) L.ToObject(1);"));
            for (int i = 1; i <= paramInfos.Length; i++)
            {
                var paramInfo = paramInfos[i - 1];
                temp.Add(new CodeSnippetStatement(
                    $"\t\t\tvar arg{i + 1} = L.{GetCheckString(paramInfo.ParameterType)}({i + 1});"));
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
                temp.Add(new CodeSnippetStatement($"\t\t\tobj.{methodInfo.Name}({paramBuilder});"));
                temp.Add(new CodeSnippetStatement("\t\t\treturn 0;"));
            }
            else
            {
                temp.Add(new CodeSnippetStatement($"\t\t\tvar result = obj.{methodInfo.Name}({paramBuilder});"));
                temp.Add(new CodeSnippetStatement($"\t\t\tL.{GetPushString(methodInfo.ReturnType)}(result);"));
                temp.Add(new CodeSnippetStatement("\t\t\treturn 1;"));
            }


            gener.AddMemberMethod(typeof(int), methodInfo.Name,
                new Dictionary<string, Type>() { { "L", typeof(ILuaState) } }, MemberAttributes.Private | MemberAttributes.Static,
                temp.ToArray());
        }

        /// <summary>
        /// 获取Push方法对应的String
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetPushString(Type type)
        {
            return $"PushValue<{type.FullName}>";
        }

        /// <summary>
        /// 获取Check方法对应的String
        /// </summary>
        /// <returns></returns>
        private string GetCheckString(Type type)
        {
            return $"CheckValue<{type.FullName}>";
        }

        
    }
}