using System;

namespace logSharp.builtins
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BuiltInAttribute : Attribute

    {
        public string Signature { get; set; }
        
        public BuiltInAttribute(string signature)
        {
            Signature = signature;
        }

    }
}