using System;

namespace logSharp.builtins
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BuiltInsAttribute : Attribute

    {
        
        public BuiltInsAttribute()
        {
        }

    }
}