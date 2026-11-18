using System;

namespace logSharp.model
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

        public ExpressionException(ExpressionError error, string message) 
            : base($"{error} - {(int)error} : {message}")
        {
            
        }
        
    }
}