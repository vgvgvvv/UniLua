//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace UniToLua
{
    
    
    public class TestUniToLua_TestClasses_MyClassWrap
    {
        
        public static void Register(UniLua.ILuaState L)
        {
			L.BeginClass(typeof(TestUniToLua.TestClasses.MyClass), null);
			L.RegFunction("New", _CreateMyClass);
			L.RegVar("memberField", get_memberField, set_memberField);
			L.RegVar("staticField", get_staticField, set_staticField);
			L.RegVar("staticProperty", get_staticProperty, set_staticProperty);
			L.RegVar("memberProperty", get_memberProperty, set_memberProperty);
			L.RegFunction("StaticFunction", StaticFunction);
			L.RegFunction("MemberFunction", MemberFunction);
			L.RegFunction("ToString", ToString);
			L.RegFunction("Equals", Equals);
			L.RegFunction("GetHashCode", GetHashCode);
			L.RegFunction("GetType", GetType);
			L.EndClass();
        }
        
        private static int _CreateMyClass(UniLua.ILuaState L)
        {
			if(L.CheckNum(0))
			{
				L.PushValue<TestUniToLua.TestClasses.MyClass>(new TestUniToLua.TestClasses.MyClass());
				return 1;
			}
			else if(L.CheckNum(1)&& L.CheckType<TestUniToLua.TestClasses.MyClass>(0))
			{
				var arg1 = L.CheckValue<TestUniToLua.TestClasses.MyClass>(1);
				L.PushValue<TestUniToLua.TestClasses.MyClass>(new TestUniToLua.TestClasses.MyClass(arg1));
				return 1;
			}
			L.L_Error("call function args is error");
			return 1;
        }
        
        private static int get_memberField(UniLua.ILuaState L)
        {
			var obj = (TestUniToLua.TestClasses.MyClass) L.ToObject(1);
			L.PushValue<System.Int32>(obj.memberField);
			return 1;
        }
        
        private static int set_memberField(UniLua.ILuaState L)
        {
			var obj = (TestUniToLua.TestClasses.MyClass) L.ToObject(1);
			var value = L.CheckValue<System.Int32>(2);
			obj.memberField = value;
			return 0;
        }
        
        private static int get_staticField(UniLua.ILuaState L)
        {
			L.PushValue<System.Int32>(TestUniToLua.TestClasses.MyClass.staticField);
			return 1;
        }
        
        private static int set_staticField(UniLua.ILuaState L)
        {
			var value = L.CheckValue<System.Int32>(1);
			TestUniToLua.TestClasses.MyClass.staticField = value;
			return 0;
        }
        
        private static int get_staticProperty(UniLua.ILuaState L)
        {
			L.PushValue<System.Int32>(TestUniToLua.TestClasses.MyClass.staticProperty);
			return 1;
        }
        
        private static int set_staticProperty(UniLua.ILuaState L)
        {
			var value = L.CheckValue<System.Int32>(1);
			TestUniToLua.TestClasses.MyClass.staticProperty = value;
			return 0;
        }
        
        private static int get_memberProperty(UniLua.ILuaState L)
        {
			var obj = (TestUniToLua.TestClasses.MyClass) L.ToObject(1);
			L.PushValue<System.Int32>(obj.memberProperty);
			return 1;
        }
        
        private static int set_memberProperty(UniLua.ILuaState L)
        {
			var obj = (TestUniToLua.TestClasses.MyClass) L.ToObject(1);
			var value = L.CheckValue<System.Int32>(2);
			obj.memberProperty = value;
			return 0;
        }
        
        private static int StaticFunction(UniLua.ILuaState L)
        {
			if(L.CheckNum(2) && L.CheckType<System.Int32, System.Int32>(1))
			{
				var arg1 = L.CheckValue<System.Int32>(1);
				var arg2 = L.CheckValue<System.Int32>(2);
				var result = TestUniToLua.TestClasses.MyClass.StaticFunction(arg1, arg2);
				L.PushValue<System.Int32>(result);
				return 1;
			}
			L.L_Error("call function args is error");
			return 1;
        }
        
        private static int MemberFunction(UniLua.ILuaState L)
        {
			if(L.CheckNum(3) && L.CheckType<TestUniToLua.TestClasses.MyClass, System.Int32, System.Int32>(1))
			{
				var obj = (TestUniToLua.TestClasses.MyClass) L.ToObject(1);
				var arg1 = L.CheckValue<System.Int32>(2);
				var arg2 = L.CheckValue<System.Int32>(3);
				var result = obj.MemberFunction(arg1, arg2);
				L.PushValue<System.Int32>(result);
				return 1;
			}
			L.L_Error("call function args is error");
			return 1;
        }
        
        private static int ToString(UniLua.ILuaState L)
        {
			if(L.CheckNum(1))
			{
				var obj = (TestUniToLua.TestClasses.MyClass) L.ToObject(1);
				var result = obj.ToString();
				L.PushValue<System.String>(result);
				return 1;
			}
			L.L_Error("call function args is error");
			return 1;
        }
        
        private static int Equals(UniLua.ILuaState L)
        {
			if(L.CheckNum(2) && L.CheckType<TestUniToLua.TestClasses.MyClass, System.Object>(1))
			{
				var obj = (TestUniToLua.TestClasses.MyClass) L.ToObject(1);
				var arg1 = L.CheckValue<System.Object>(2);
				var result = obj.Equals(arg1);
				L.PushValue<System.Boolean>(result);
				return 1;
			}
			L.L_Error("call function args is error");
			return 1;
        }
        
        private static int GetHashCode(UniLua.ILuaState L)
        {
			if(L.CheckNum(1))
			{
				var obj = (TestUniToLua.TestClasses.MyClass) L.ToObject(1);
				var result = obj.GetHashCode();
				L.PushValue<System.Int32>(result);
				return 1;
			}
			L.L_Error("call function args is error");
			return 1;
        }
        
        private static int GetType(UniLua.ILuaState L)
        {
			if(L.CheckNum(1))
			{
				var obj = (TestUniToLua.TestClasses.MyClass) L.ToObject(1);
				var result = obj.GetType();
				L.PushValue<System.Type>(result);
				return 1;
			}
			L.L_Error("call function args is error");
			return 1;
        }
    }
}
