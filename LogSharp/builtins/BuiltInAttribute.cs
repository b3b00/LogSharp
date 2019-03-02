using System;

namespace logSharp.builtins
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BuiltInAttribute : Attribute

    {
        public BuiltInAttribute(string signature)
        {
            Signature = signature;
        }

        public string Signature { get; set; }
    }
}