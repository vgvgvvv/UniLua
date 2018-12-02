using System;

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

        public void PushObject(Object obj)
        {
            var type = obj.GetType();
            var reference = ClassMetaRefDict[type.Name];
            API.PushLightUserData(obj);
            GetRef(reference);
            API.SetMetaTable(-2);

        }
    }
}