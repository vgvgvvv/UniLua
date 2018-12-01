namespace UniLua
{
    public partial class LuaState
    {
        public void NewTable(string tableName, int index = -3)
        {
            API.PushString(tableName);
            API.NewTable();
            API.RawSet(index);
        }
    }
}