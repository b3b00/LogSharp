using sly.lexer;

namespace LogSharp.parser
{
    [CallBacks(typeof(LogSharpLexerCallbacks))]
    public enum LogSharpToken
    {
        #region scalars
        
        [Lexeme(GenericToken.Identifier)]
        Identifier = 1,
        Variable = 2,
        Atom = 3,
        
        [Lexeme(GenericToken.String,"\"")]
        String = 4,
        
        [Lexeme(GenericToken.Int)]
        Integer = 5,
        
        [Lexeme(GenericToken.Double)]
        Double = 6,
        
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
        Dot = 14
        
        #endregion
        
        
    }
}