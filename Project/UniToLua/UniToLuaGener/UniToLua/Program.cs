using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UniToLuaGener
{
    class Program
    {
        private static List<string> dllList = new List<string>();
        private static string outputPath;

        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.StartsWith("-path"))
                {
                    
                }
            }

            foreach (var dllPath in dllList)
            {
                GenAll(dllPath);
            }
        }

        private static void GenAll(string dllpath)
        {
            var exporter = new ExportToLua()
            {
                dllPath = dllpath,
                outputPath = outputPath
            };
            exporter.GenAll(Assembly.LoadFile(dllpath));
        }
    }
}
