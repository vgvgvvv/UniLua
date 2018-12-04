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
                dllPath = @"D:\Documents\Projects\UniLua\Project\UniToLua\TestUniToLua\bin\Debug\TestUniToLua.dll",
                outputPath = @"D:\Documents\Projects\UniLua\Project\UniToLua\TestUniToLua\WrapClasses"
            };
            exporter.GenAll();
        }

    }
}