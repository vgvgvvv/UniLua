using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniLua;
using UniLua.Tools;

namespace TestUniToLua
{
    [TestClass]
    public class TestLuaRegister
    {
       

        [TestMethod]
        public void TestRegisterModule()
        {
            LuaState state = Util.InitTestEnv();
           

            state.L_OpenLibs();
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
    }
}
