
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
            API.PushValue(2);   // table key key
            API.RawGet(1);      // table key value
            if (!API.IsNil(-1))
            {
                return 1;
            }

            //TODO 从preload拿
            //API.Pop(1);             //table key
            //API.PushString(".name");//table key .name
            //API.RawGet(1);          //table key space

            ////看看space是不是空的
            //if (!API.IsNil(-1))
            //{
            //    API.
            //}

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

        #region Class

        public void BeginClass(Type bindClass, Type parentClass)
        {
            //API.PushString(bindClass.Name);     //table name
            //API.NewTable();                     //table name classtable

        }

        public void EndClass()
        {

        }

        #endregion

        #region StaticLib

        public void BeginStaticLib(string staticLibName)
        {
            API.PushString(staticLibName);  //name
            API.NewTable();                 //name table
            AddToLoaded();                  
            API.PushValue(-1);              //name table table

            //TODO tag??

            API.PushString(".name");        //name table table .name
            PushFullName(-4);
            API.RawSet(-3);

            API.PushString(GetTagMethodName(TMS.TM_INDEX));
            API.PushCSharpFunction(StaticIndexEvent);
            API.RawSet(-3);

            API.PushString(GetTagMethodName(TMS.TM_NEWINDEX));
            API.PushCSharpFunction(StackNewIndexEvent);
            API.RawSet(-3);

        }

        public void EndStaticLib()
        {
            API.SetMetaTable(-2);   //name table table
            API.RawSet(-3);         //name table
        }

        /// <summary>
        /// t k 
        /// </summary>
        /// <param name="L"></param>
        /// <returns></returns>
        private int StaticIndexEvent(ILuaState L)
        {
            API.PushValue(2);   //t k k
            API.RawGet(1);      //t k v

            if (!API.IsNil(-1))
            {
                return 1;
            }

            API.Pop(1);
            API.PushString(".get"); //t k .get
            API.RawGet(1);          //t k tget

            if (API.IsTable(-1))
            {
                API.PushValue(2);   //t k tget k
                API.RawGet(-2);     //t k tget func

                if (API.IsFunction(-1))
                {
                    API.Call(0, 1);
                    return 1;
                }
            }

            API.SetTop(2);

            if (GetFromPreload())
            {
                return 1;
            }

            L_Error("field or property %s does not exist", API.ToString(2));
            return 1;

        }

        /// <summary>
        /// t k v
        /// </summary>
        /// <param name="L"></param>
        /// <returns></returns>
        private int StackNewIndexEvent(ILuaState L)
        {
            API.PushString(".set"); //t k v.set
            API.RawGet(1);          // t k v table

            if (API.IsTable(-1))    
            {
                API.PushValue(2);   //t k v table k
                API.RawGet(-2);     //t k v table func

                if (API.IsFunction(-1))
                {
                    API.PushValue(1);
                    API.PushValue(3);
                    API.Call(2, 0);
                    return 0;
                }
            }

            API.SetTop(3);
            L_Error("field or property %s does not exist", API.ToString(2));
            return 1;
        }

        #endregion



        #region Enum

        public void BeginEnum(string enumName)
        {
            API.PushString(enumName);       //enumName
            API.NewTable();                 //enumName table
            AddToLoaded();                  
            API.NewTable();                 //enumName table table

            //TODO tag??

            API.PushString(".name");        //enumName table table .name
            PushFullName(-4);               //enumName table table .name fullname
            API.RawSet(-3);                 //enumName table table

            API.PushString(GetTagMethodName(TMS.TM_INDEX)); //enum table table __index
            API.PushCSharpFunction(EnumIndexEvent);         //enum table table __index func
            API.RawSet(-3);                                 //enum table table

            API.PushString(GetTagMethodName(TMS.TM_NEWINDEX));
            API.PushCSharpFunction(EnumNewIndexEvent);
            API.RawSet(-3);

        }

        /// <summary>
        /// 结束Enum
        /// enumName table table
        /// -3 +0
        /// </summary>
        public void EndEnum()
        {
            API.SetMetaTable(-2);   //enumName table 设置元表
            API.RawSet(-3);         //加入module
        }

        /// <summary>
        ///
        /// table key
        /// </summary>
        /// <param name="L"></param>
        /// <returns></returns>
        private int EnumIndexEvent(ILuaState L)
        {
            API.GetMetaTable(1);    //table key meta

            if (API.IsTable(-1))
            {
                API.PushValue(2);   //table key meta key
                API.RawGet(-2);     //table key meta value

                if (!API.IsNil(-1))
                {
                    return 1;
                }

                API.Pop(1);             //table key meta

                API.PushString(".get"); //table key meta .get
                API.RawGet(-2);         //table key meta gettable

                if (API.IsTable(-1))
                {
                    API.PushValue(2);   //table key meta gettable key
                    API.RawGet(-2);     //table key meta gettable getfunc

                    if (API.IsFunction(-1))
                    {
                        API.Call(0, 1);     //table key meta gettable value
                        API.PushValue(2);   //table key meta gettable value key
                        API.PushValue(-2);  //table key meta gettable value key value
                        API.RawSet(3);      //table key meta gettable value
                        return 1;
                    }

                    API.Pop(1);         //table key meta gettable
                }

            }

            API.SetTop(2);          //table key
            API.PushNil();          //table key nil
            return 1;
        }

        private int EnumNewIndexEvent(ILuaState L)
        {
            L_Error("the left-hand side of an assignment must be a variable, a property or an indexer");
            return 1;
        }
        #endregion

        public void RegFunction(string funcName, CSharpFunctionDelegate func)
        {
            API.PushString(funcName);
            API.PushCSharpFunction(func);
            API.RawSet(-3);
        }

        /// <summary>
        /// table
        /// </summary>
        /// <param name="func"></param>
        /// <param name="get"></param>
        /// <param name="set"></param>
        public void RegVar(string name, CSharpFunctionDelegate get, CSharpFunctionDelegate set)
        {
            if (get != null)
            {
                API.PushString(".get"); //table .get
                API.RawGet(-2);         //table gettable

                if (!API.IsTable(-1))
                {
                    API.Pop(1);             //table
                    API.NewTable();         //table table
                    API.PushString(".get"); //table table .get
                    API.PushValue(-2);      //table table .get table
                    API.RawSet(-4);         //table table
                }

                //设置get函数
                API.PushString(name);
                API.PushCSharpFunction(get);
                API.RawSet(-3);
                API.Pop(1);

            }

            if (set != null)
            {
                API.PushString(".set"); //table .get
                API.RawGet(-2);         //table gettable

                if (!API.IsTable(-1))
                {
                    API.Pop(1);             //table
                    API.NewTable();         //table table
                    API.PushString(".set"); //table table .get
                    API.PushValue(-2);      //table table .get table
                    API.RawSet(-4);         //table table
                }

                //设置set函数
                API.PushString(name);
                API.PushCSharpFunction(set);
                API.RawSet(-3);
                API.Pop(1);
            }

        }


        /// <summary>
        /// 推入（类、枚举、静态类）的完整名称
        /// -1 +1
        /// </summary>
        /// <param name="pos">局部名称在栈中的位置</param>
        private void PushFullName(int pos)//pos
        {
            if (_currentModuleName.Length > 0)
            {
                API.PushString(_currentModuleName);         //modulename
                API.PushString(".");                        //modulename .
                API.PushValue(pos < 0 ? pos - 2 : pos + 2); //modulename . name
                API.Concat(3);                              //fullname
            }
            else
            {
                API.PushValue(pos);                         //fullname
            }
        }

        /// <summary>
        /// 加入已加载
        /// name table
        /// +0 -0
        /// </summary>
        private void AddToLoaded()
        {
            GetRef(TOLUA_LOADED);     //name table preload
            PushFullName(-3);       //name table preload fullname
            API.PushValue(-3);      //name table preload fullname table
            API.RawSet(-3);         //name table preload
            API.Pop(1);             //name table
        }

        /// <summary>
        /// t k
        /// </summary>
        /// <returns></returns>
        private bool GetFromPreload()
        {
            API.SetTop(2);          //t k
            API.SetMetaTable(1);    //t k mt
            API.PushString(".name");//t k mt .name
            API.RawGet(-2);         //t k mt space

            if (!API.IsNil(-1))
            {
                GetRef(TOLUA_PRELOAD);  //t k mt space preload
                API.PushValue(-2);      //t k mt space preload space
                API.PushString(".");    //t k mt space preload space .
                API.PushValue(2);       ////t k mt space preload space . k
                API.Concat(3);          //t k mt space preload fullname
                API.PushValue(-1);      //t k mt space preload fullname fullname
                API.RawGet(-3);         //t k mt space preload fullname value

                if (!API.IsNil(-1))
                {
                    API.Pop(1);             //t k mt space preload fullname
                    GetRef(TOLUA_REQUIRE);  //t k mt space preload fullname require
                    API.PushValue(-2);      //t k mt space preload fullname require fullname
                    API.Call(1, 1);
                    return true;
                }
            }

            API.SetTop(2);
            return false;
        }
        
    }
}