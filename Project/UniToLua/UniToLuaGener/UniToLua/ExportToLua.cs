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

        private void GenEnum(Type type)
        {
            if (type == null)
                return;
            var className = type.FullName?.Replace(".", "_") + "Wrap";
            var enumNames = type.GetEnumNames();

            CodeGener gener = new CodeGener("UniToLua", className);

            List<CodeStatement> registerMethodStatement = new List<CodeStatement>();
            registerMethodStatement.Add(new CodeSnippetStatement($"L.BeginEnum(\"{type.Name}\");"));
            foreach (var enumName in enumNames)
            {
                registerMethodStatement.Add(new CodeSnippetStatement($"L.RegVar({enumName}, get_{enumName}, null)"));
            }
            registerMethodStatement.Add(new CodeSnippetStatement($"L.EndEnum()"));

            gener.AddMemberMethod(typeof(void), "Register", new Dictionary<string, Type>() { { "L", typeof(ILuaState) } },
                MemberAttributes.Public | MemberAttributes.Static, registerMethodStatement.ToArray());

            foreach (var enumName in enumNames)
            {
                GenRegEnum(gener, type, enumName);
            }

        }

        private void GenRegEnum(CodeGener gener, Type type, string enumName)
        {
            gener.AddMemberMethod(typeof(int), $"get_{enumName}", new Dictionary<string, Type>() { { "L", typeof(ILuaState) } },
                MemberAttributes.Private, new CodeSnippetStatement[]
                {
                    new CodeSnippetStatement($"L.PushLightUserData({type.FullName}.{enumName});"),
                    new CodeSnippetStatement("return 1;"),
                });
        }


        #endregion


        #region StaticLib

        private void GenStaticLib(Type type)
        {
            if (type == null)
                return;
            var className = type.FullName?.Replace(".", "_") + "Wrap";
            CodeGener gener = new CodeGener("UniToLua", className);

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            var propertys = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);

            List<CodeStatement> registerMethodStatement = new List<CodeStatement>();
            registerMethodStatement.Add(new CodeSnippetStatement($"L.BeginStaticLib(\"{type.Name}\");"));

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
                //TODO
                //List<CodeStatement> temp = new List<CodeStatement>();
                
                //gener.AddMemberMethod(typeof(int), $"get_{fieldInfo.Name}",
                //    new Dictionary<string, Type>() {{"L", typeof(ILuaState)}}, MemberAttributes.Private, temp.ToArray());
            }

            foreach (var propertyInfo in propertys)
            {
                //TODO
            }

            foreach (var methodInfo in methods)
            {
                //TODO
            }
        }

        #endregion



        private void GenClass(Type type)
        {
            //TODO
        }

        private void GenConstructor(CodeGener gener, ConstructorInfo constructor)
        {
            //TODO
        }

        private void GenRegProperty(CodeGener gener, FieldInfo info, Func<bool> filter = null)
        {
            //TODO
        }

        private void GenRegField(CodeGener gener, PropertyInfo type, Func<bool> filter = null)
        {
            //TODO
        }

        private void GenRegFunction(CodeGener gener, MethodInfo type, Func<bool> filter = null)
        {
            //TODO
        }

        private string GetPushString(Type type)
        {
            if (type == typeof(string))
            {
                return "PushString";
            }
            else if (type == typeof(int))
            {
                return "PushInteger";
            }
            else if (type == typeof(uint))
            {
                return "PushUnsigned";
            }
            else if (type == typeof(float) || type == typeof(double))
            {
                return "PushNumber";
            }
            else if (type == typeof(bool))
            {
                return "PushBoolean";
            }
            //TODO
        }

        private string GetCheckString()
        {
            //TODO
        }

        private string GetToString()
        {
            //TODO
        }
    }
}