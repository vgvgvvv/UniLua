using System;
using NUnit.Framework;
using UniLua;

namespace TestUniToLua
{
    public enum HelloEnum
    {
        ENUM_A,
        ENUM_B
    }

    public static class HelloStaticLib
    {
        public static string value = "hoho";

        public static string Concat(string str1, string str2)
        {
            return str1 + str2;
        }
    }

    public class TestLuaRegister
    {
        [Test]
        public void TestLua()
        {
            LuaState state = Util.InitTestEnv();
            if (state.L_DoFile("TestUniLua.lua") != ThreadStatus.LUA_OK)
            {
                Console.WriteLine(state.L_CheckString(-1));
            }
        }

        [Test]
        public void TestRegisterModule()
        {
            LuaState state = Util.InitTestEnv();
           
            state.BeginModule(null);
            state.BeginModule("Test");
            state.BeginModule("HHH");
            state.EndModule();
            state.EndModule();
            state.EndModule();

            
            if (state.L_DoFile("TestLuaRegister.lua") != ThreadStatus.LUA_OK)
            {
                Console.WriteLine(state.L_CheckString(-1));
            }
        }


        [Test]
        public void TestRegisterEnum()
        {
            LuaState state = Util.InitTestEnv();
            state.BeginModule(null);
            state.BeginModule("Test");
            state.BeginEnum("Hello");
            state.RegVar("ENUM_A", GetA, null);
            state.RegVar("ENUM_B", GetB, null);
            state.EndEnum();
            state.EndModule();
            state.EndModule();

            if (state.L_DoFile("TestLuaRegisterEnum.lua") != ThreadStatus.LUA_OK)
            {
                Console.WriteLine(state.L_CheckString(-1));
            }
        }

        private int GetA(ILuaState state)
        {
            state.PushLightUserData(HelloEnum.ENUM_A);
            return 1;
        }

        private int GetB(ILuaState state)
        {
            state.PushLightUserData(HelloEnum.ENUM_B);
            return 1;
        }

        [Test]
        public void TestRegisterStaticLib()
        {
            LuaState state = Util.InitTestEnv();
            state.BeginModule(null);
            state.BeginModule("Test");
            state.BeginStaticLib("HelloStaticLib");
            state.RegVar("value", HelloStatic_get_value, HelloStatic_set_value);
            state.RegFunction("Concat", HelloStatic_Concat);
            state.EndStaticLib();
            state.EndModule();
            state.EndModule();

            if (state.L_DoFile("TestLuaRegisterStaticLib.lua") != ThreadStatus.LUA_OK)
            {
                Console.WriteLine(state.L_CheckString(-1));
            }
        }

        private int HelloStatic_get_value(ILuaState L)
        {
            L.PushString(HelloStaticLib.value);
            return 1;
        }

        private int HelloStatic_set_value(ILuaState L)
        {
            var result = L.L_CheckString(-1);
            HelloStaticLib.value = result;
            return 1;
        }

        private int HelloStatic_Concat(ILuaState L)
        {
            var str1 = L.L_CheckString(-1);
            var str2 = L.L_CheckString(-2);
            L.PushString(HelloStaticLib.Concat(str1, str2));
            return 1;
        }


    }
}
