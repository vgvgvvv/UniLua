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

        #region Push

        public void PushObject(Object obj)
        {
            var type = obj.GetType();
            var reference = ClassMetaRefDict[type.Name];
            API.PushLightUserData(obj);
            GetRef(reference);
            API.SetMetaTable(-2);
        }

        #endregion

        #region Check

        public string CheckString(int narg)
        {
            return L_CheckString(narg);
        }

        public int CheckInteger(int narg)
        {
            return L_CheckInteger(narg);
        }

        public uint CheckUnsigned(int narg)
        {
            return L_CheckUnsigned(narg);
        }

        public double CheckNumber(int narg)
        {
            return L_CheckNumber(narg);
        }

        public bool CheckBoolean(int narg)
        {
            return L_CheckBool(narg);
        }

        public ulong CheckUInt64(int narg)
        {
            return L_CheckUInt64(narg);
        }

        public object CheckObject(int narg)
        {
            return L_CheckOBject(narg);
        }

        #endregion

        #region To

        public string ToString(int narg)
        {
            return API.ToString(narg);
        }

        public int ToInteger(int narg)
        {
            return API.ToInteger(narg);
        }

        public uint ToUnsigned(int narg)
        {
            return API.ToUnsigned(narg);
        }

        public double ToNumber(int narg)
        {
            return API.ToNumber(narg);
        }

        public bool ToBoolean(int narg)
        {
            return API.ToBoolean(narg);
        }

        public ulong ToUInt64(int narg)
        {
            return API.ToUInt64(narg);
        }

        public object ToObject(int narg)
        {
            return API.ToObject(narg);
        }

        #endregion
    }
}