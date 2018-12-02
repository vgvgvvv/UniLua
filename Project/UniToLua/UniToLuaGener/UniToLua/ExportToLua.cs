using System.Reflection;

namespace UniToLuaGener
{
    public class ExportToLua
    {
        public void GenAll(Assembly target)
        {
            GenBinder(target);
            GenWrapper(target);
        }

        public void GenBinder(Assembly target)
        {

        }

        public void GenWrapper(Assembly target)
        {

        }
    }
}