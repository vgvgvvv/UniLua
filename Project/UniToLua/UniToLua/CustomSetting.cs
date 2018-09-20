using System;
using System.Collections.Generic;

namespace UniLua
{
    public class CustomSetting
    {
        public static System.Action<object> Log;
        public static System.Action<object> LogError;

        /// <summary>
        /// Lua加载路径
        /// </summary>
        public static string LuaRoot;

        /// <summary>
        /// LuaWrap路径
        /// </summary>
        public static string WrapRoot;

        /// <summary>
        /// 导出类型
        /// </summary>
        public static List<Type> ExportTypeList = new List<Type>()
        {
            
        };
    }
}