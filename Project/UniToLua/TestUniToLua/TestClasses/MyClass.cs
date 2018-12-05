using UniLua;

namespace TestUniToLua.TestClasses
{
    [ToLua]
    public class MyClass
    {
        public static int staticField = 100;

        public static int staticProperty { get; set; } = 200;

        public static int StaticFunction(int a, int b)
        {
            return a + b;
        }

        public int memberField = 300;

        public int memberProperty { get; set; } = 400;

        public int MemberFunction(int a, int b)
        {
            return a + b;
        }

    }
}