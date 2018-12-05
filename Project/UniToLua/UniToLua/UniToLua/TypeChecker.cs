using System;

namespace UniLua
{
    public partial class LuaState
    {
        public class TypeChecker<T>
        {
            public static Func<LuaState, int, bool> Check = DefaultCheck;
            public static Type Type = typeof(T);
            public static bool IsValueType = Type.IsValueType;
            public static bool IsArray = Type.IsArray;

            public static bool IsNumberType = IsNumber();
            public static bool IsBoolType = Type == typeof(bool) || IsNumberType;
            public static bool IsStringType = Type == typeof(string);

            private static int canBeNil = -1;//0 不可为空 1 可为空


            private static bool DefaultCheck(LuaState L, int pos)
            {
                StkId addr;
                if (!L.Index2Addr(pos, out addr))
                {
                    return false;
                }

                var value = addr.V;

                if (value.TtIsNil())
                {
                    return true;
                }

                var luaType = (LuaType)value.Tt;
                switch (luaType)
                {
                    case LuaType.LUA_TNIL:
                        return true;
                    case LuaType.LUA_TNUMBER:
                    case LuaType.LUA_TUINT64:
                        return IsNumberType;
                    case LuaType.LUA_TBOOLEAN:
                        return IsBoolType;
                    case LuaType.LUA_TSTRING:
                        return IsStringType;
                    case LuaType.LUA_TTABLE:
                    case LuaType.LUA_TFUNCTION:
                    case LuaType.LUA_TLIGHTUSERDATA:
                        return value.OValue is T;
                    default:
                        return false;
                }

            }

            private static bool CanBeNil()
            {
                if (canBeNil != -1)
                {
                    return canBeNil == 0;
                }

                if (!IsValueType)
                {
                    canBeNil = 1;
                    return true;
                }

                if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    canBeNil = 1;
                    return true;
                }

                canBeNil = 0;
                return false;
            }

            private static bool IsNumber()
            {
                return Type == typeof(short) || Type == typeof(ushort) ||
                       Type == typeof(int) || Type == typeof(uint) ||
                       Type == typeof(long) || Type == typeof(ulong);
            }

        }
    }

    
}