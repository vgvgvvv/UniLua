namespace UniLua
{
    public partial class LuaState
    {

        public void GetRef(int reference)
        {
           API.RawGetI(LuaDef.LUA_REGISTRYINDEX, reference);
        }
    }
}