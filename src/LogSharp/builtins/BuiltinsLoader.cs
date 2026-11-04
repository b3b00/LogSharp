using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using logSharp.interpretation;
using sly.parser.generator;

namespace logSharp.builtins
{
    public class BuiltinsLoader
    {
     
        public static void LoadBuiltIns(Interpreter interpreter, Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                foreach (var attribute in type.CustomAttributes)
                {
                    if (attribute.AttributeType == typeof(BuiltInsAttribute))
                    {
                        LoadBuiltIns(interpreter, type);
                        LoadBuiltInsBuilder(interpreter, type);
                    }
                }
            }
        }

        public static void LoadBuiltIns(Interpreter interpreter)
        {
            var thisAssembly = Assembly.GetAssembly(typeof(PrologBuiltins));
            LoadBuiltIns(interpreter,typeof(PrologBuiltins));
            ;
            var calling = Assembly.GetCallingAssembly();
            LoadBuiltIns(interpreter,calling);
            ;
            var executing = Assembly.GetExecutingAssembly();
            LoadBuiltIns(interpreter,executing);
            
            var entry = Assembly.GetEntryAssembly();
            LoadBuiltIns(interpreter,entry);
            


        }

        public static void LoadBuiltInsBuilder(Interpreter interpreter, Type builtInClass)
        {
            var methods = builtInClass.GetMethods().ToList();
            methods.ForEach(method =>
            {
                var attributes = method.GetCustomAttributes().ToList();
                var attr = attributes.Find(a => a.GetType() == typeof(BuiltInBuilderAttribute));
                if (attr != null && attr is BuiltInBuilderAttribute)
                {
                    method.Invoke(null,new object[] {interpreter});
                    //interpreter.AddBuiltIn(builtInAttribute.Signature, builtin);
                }
            });
        }

        public static void LoadBuiltIns(Interpreter interpreter, Type builtInClass)
        {
            var methods = builtInClass.GetMethods().ToList();
            methods.ForEach(method =>
            {
                var attributes = method.GetCustomAttributes().ToList();
                var attr = attributes.Find(a => a.GetType() == typeof(BuiltInAttribute));
                if (attr != null && attr is BuiltInAttribute builtInAttribute)
                {
                    var builtin = (PrologBuiltIn)method.CreateDelegate(typeof(PrologBuiltIn));
                    interpreter.AddBuiltIn(builtInAttribute.Signature, builtin);
                }
            });
        }


    }
}