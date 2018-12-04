using UniLua;

namespace TestUniToLua.TestClasses
{
    [ToLua]
    public class MyClass
    {
        public static int staticField;

        public static int staticProperty { get; set; }

        public static int StaticFunction(int a, int b)
        {
            return a + b;
        }

        public int memberField;

        public int memberProperty { get; set; }

        public int MemberFunction(int a, int b)
        {
            return a + b;
        }

    }
}