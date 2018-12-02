using System;

namespace UniLua
{
    public partial interface ILuaState : IToLua
    {
    }

    public interface IToLua
    {
        void OpenToLua();

        void NewTable(string tableName, int index = -3);

        void PushObject(Object obj);

        bool BeginModule(string name);

        void EndModule();

        int BeginClass(Type bindClass, Type baseClass);

        void EndClass();

        int BeginStaticLib(string staticLibName);

        void EndStaticLib();

        int BeginEnum(string enumName);

        void EndEnum();

        void RegFunction(string funcName, CSharpFunctionDelegate func);

        void RegVar(string name, CSharpFunctionDelegate get, CSharpFunctionDelegate set);


    }
}