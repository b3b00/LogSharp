using System;

namespace logSharp.builtins;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class BuiltInBuilderAttribute : Attribute

{
        
    public BuiltInBuilderAttribute()
    {
    }

}