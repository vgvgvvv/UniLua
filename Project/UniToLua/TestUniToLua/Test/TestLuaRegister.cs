using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniLua;
using UniLua.Tools;

namespace TestUniToLua
{
    public enum HelloEnum
    {
        ENUM_A,
        ENUM_B
    }

    [TestClass]
    public class TestLuaRegister
    {
        [TestMethod]
        public void TestLua()
        {
            LuaState state = Util.InitTestEnv();
            if (state.L_DoFile("TestUniLua.lua") != ThreadStatus.LUA_OK)
            {
                Console.WriteLine(state.L_CheckString(-1));
            }
        }

        [TestMethod]
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


        [TestMethod]
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

        [TestMethod]
        public void TestSetI()
        {
            
        }


    }
}
