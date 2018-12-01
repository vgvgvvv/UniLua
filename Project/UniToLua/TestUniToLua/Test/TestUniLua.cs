using Microsoft.VisualStudio.TestTools.UnitTesting;
using UniLua;

namespace TestUniToLua
{
    [TestClass]
    public class TestUniLua
    {
        [TestMethod]
        public void TestLightUserData()
        {
            LuaState state = Util.InitTestEnv();
        }
    }
}