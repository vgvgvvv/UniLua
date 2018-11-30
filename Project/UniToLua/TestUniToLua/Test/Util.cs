using System;
using System.IO;
using UniLua;
using UniLua.Tools;

namespace TestUniToLua
{
    public class Util
    {
        public static readonly string LuaPath = Path.Combine(Environment.CurrentDirectory, "../../Lua");

        public static LuaState InitTestEnv()
        {
            LuaFile.SetPathHook((fileName) => Path.Combine(LuaPath, fileName));
            ULDebug.Log = Console.WriteLine;
            ULDebug.LogError = Console.Error.WriteLine;

            var lusState = new LuaState();
            lusState.L_OpenLibs();

            return lusState;
        }
    }
}