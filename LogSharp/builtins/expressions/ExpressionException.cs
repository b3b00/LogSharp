using System;

namespace logSharp.builtins.expressions
{

    public enum ExpressionError
    {
        MissingValue,
        BadTypes,
        UnknownOperation,
        Error
    }
    
    public class ExpressionException : Exception
    {
        public ExpressionException(string message) : base(message)
        {
            
        }

        public ExpressionException(string message, Exception inner) : base(message, inner)
        {
            
        }
        
        public ExpressionException(ExpressionError error, string message, Exception inner) 
            : base($"{error} - {(int)error} : {message}", inner)
        {
            
        }

        public ExpressionException(ExpressionError error, string message) 
            : base($"{error} - {(int)error} : {message}")
        {
            
        }
        
    }
}