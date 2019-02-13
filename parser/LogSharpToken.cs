using sly.lexer;

namespace LogSharp.parser
{
    public enum LogSharpToken
    {
        #region scalars
        
        [Lexeme(GenericToken.Identifier)] 
        Variable = 1,
        Atom = 2,
        
        [Lexeme(GenericToken.String,"\"")]
        String = 3,
        
        [Lexeme(GenericToken.Int)]
        Integer = 4,
        
        [Lexeme(GenericToken.Double)]
        Double = 5,
        
        #endregion
        
        #region sugar
        
        [Lexeme(GenericToken.SugarToken,":-")]
        Imply = 10,
        
        [Lexeme(GenericToken.SugarToken,"(")]
        LeftPar = 11,
        
        [Lexeme(GenericToken.SugarToken,")")]
        RightPar = 12,
        
        [Lexeme(GenericToken.SugarToken,",")]
        Comma = 13,
        
        [Lexeme(GenericToken.SugarToken,".")]
        Dot = 13
        
        #endregion
        
        
    }
}