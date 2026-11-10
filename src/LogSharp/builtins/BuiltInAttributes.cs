using System;
using System.Diagnostics.CodeAnalysis;

namespace logSharp.builtins
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BuiltInsAttribute : Attribute

    {
        
        public BuiltInsAttribute()
        {
        }

    }
}