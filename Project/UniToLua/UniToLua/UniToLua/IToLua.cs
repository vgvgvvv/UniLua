using System;

namespace UniLua
{
    public interface IToLua
    {
        void PushObject(Object obj);
    }
}