
using System.Text;

namespace UniLua
{
    public partial class LuaState
    {
        private string _currentModuleName = string.Empty;
        private readonly StringBuilder _sharedStringBuilder = new StringBuilder();

        /// <summary>
        /// 开始模块 -1 +1
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool BeginModule(string name)//stack name
        {
            if (name != null)
            {
                if (API.Type(-1) != LuaType.LUA_TTABLE)
                {
                    TagError(-1, LuaType.LUA_TTABLE);
                    return false;
                }

                API.PushString(name);   //stack name
                API.RawGet(-2);         //stack value

                //没有该table的情况
                if (API.IsNil(-1))
                {
                    API.Pop(1);         //stack
                    API.NewTable();     //stack table

                    API.PushString(GetTagMethodName(TMS.TM_INDEX)); //stack table "__index"
                    API.PushCSharpFunction(ModuleIndexEvent);       //stack table "__index" function
                    API.RawSet(-3);                                 //stack table

                    API.PushString(name);                           //stack table name
                    API.PushString(".name");                        //stack table name ".name"
                    PushModuleName(name);

                    API.RawSet(-4);     //stack table name
                    API.PushValue(-2);  //stack table name table
                    API.RawSet(-4);     //stack table 

                    API.PushValue(-1);      //stack table table
                    API.SetMetaTable(-2);   //stack table
                    return true;

                }
                //Table已经存在的情况
                else if(API.IsTable(-1))
                {
                    if (API.GetMetaTable(-1) == false)
                    {
                        API.PushString(GetTagMethodName(TMS.TM_INDEX)); //stack table "__index"
                        API.PushCSharpFunction(ModuleIndexEvent);
                        API.RawSet(-3);                                 //stack table

                        API.PushString(name);
                        API.PushString(".name");
                        PushModuleName(name);
                        API.RawSet(-4);     //stack table name
                        API.PushValue(-2);  //stack table name table
                        API.RawSet(-4);     //stack table 

                        API.PushValue(-1);      //stack table table
                        API.SetMetaTable(-2);   //stack table
                    }
                    //stack value metatable
                    else
                    {
                        API.Pop(2);
                        PushModuleName(name);
                        API.Pop(1);
                        API.PushString(name);   //stack key
                        API.RawGet(-2);         //stack table
                    }
                    return true;
                }
                return false;
            }
            else
            {
                API.PushGlobalTable();// global
                return true;
            }
        }

        //-0 +1
        /// <summary>
        /// 压入模块名
        /// </summary>
        /// <param name="name"></param>
        internal void PushModuleName(string name)
        {
            _sharedStringBuilder.Clear();
            _sharedStringBuilder.Append(_currentModuleName).Append(".").Append(name);
            _currentModuleName = _sharedStringBuilder.ToString();
            API.PushString(_currentModuleName);
        }

        //-1 +1
        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="L"></param>
        /// <returns></returns>
        internal int ModuleIndexEvent(ILuaState L)//table key
        {
            //先尝试直接拿
            API.PushValue(2);// table key key
            API.RawGet(1);// table key value
            if (!API.IsNil(-1))
            {
                return 1;
            }

            //API.Pop(1);
            //TODO 从preload里拿
            return 1;
        }

        //-1 +0
        /// <summary>
        /// 结束模块
        /// </summary>
        public void EndModule()
        {
            API.Pop(1);
            var index = _currentModuleName.LastIndexOf('.');
            if (index > 0)
            {
                _currentModuleName = _currentModuleName.Substring(0, index);
            }
            else
            {
                _currentModuleName = string.Empty;
            }
        }

    }
}