using NUnit.Framework;
using UniToLuaGener;

namespace TestUniToLua
{
    public class TestLuaWrapGener
    {
        [Test]
        public void TestGenWrap()
        {
            var exporter = new ExportToLua()
            {
                dllPath = @"E:\Projects\CSProjects\UniLua\Project\UniToLua\TestUniToLua\bin\Debug\TestUniToLua.dll",
                outputPath = @"E:\Projects\CSProjects\UniLua\Project\UniToLua\TestUniToLua\WrapClasses"
            };
            exporter.GenAll();
        }

    }
}