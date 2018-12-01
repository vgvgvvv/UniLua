namespace UniLua
{
    public partial class LuaState
    {
        public const int LUA_PRELOAD = 21;
        public const int LUA_LOADED = 22;

        public void OpenToLua()
        {
            API.NewTable();             //table
            API.SetGlobal("tolua");     //

            OpenPreload();
        }


        private void OpenPreload()
        {
            API.GetGlobal("tolua");     //table

            API.PushString("preload");  //table preload
            API.NewTable();             //table preload table
            API.PushValue(-1);          //table preload table table
            API.RawSetI(LuaDef.LUA_REGISTRYINDEX, LUA_PRELOAD); //table preload table
            API.RawSet(-3);             //table

            API.PushString("loaded");  //table preload
            API.NewTable();             //table preload table
            API.PushValue(-1);          //table preload table table
            API.RawSetI(LuaDef.LUA_REGISTRYINDEX, LUA_LOADED); //table preload table
            API.RawSet(-3);             //table

            API.Pop(1);
            

        }
    }
}