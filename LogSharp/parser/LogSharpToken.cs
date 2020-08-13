using sly.lexer;

namespace logSharp.parser
{
    [CallBacks(typeof(LogSharpLexerCallbacks))]
    public enum LogSharpToken
    {
        #region scalars
        
        [Lexeme(GenericToken.Identifier,IdentifierType.AlphaNumericDash)]
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
        Dot = 14,
        
        [Lexeme(GenericToken.SugarToken,"[")]
        LeftBrack = 15,
        
        [Lexeme(GenericToken.SugarToken,"]")]
        RightBrack = 16,
        
        [Lexeme(GenericToken.SugarToken,"|")]
        Pipe = 17,
        
        [Lexeme(GenericToken.SugarToken,"?")]
        QuestionMark = 18,
        
        [Lexeme(GenericToken.KeyWord,"is")]
        Is = 19,
        
        
        #endregion
        
        #region expression 50 -> 100
        
        // the + operator
        [Lexeme(GenericToken.SugarToken,"+")] Plus = 50,

        // the - operator
        [Lexeme(GenericToken.SugarToken,"-")]
        Minus = 51,

        // the * operator
        [Lexeme(GenericToken.SugarToken,"*")]
        Times = 52,

        //  the  / operator
        [Lexeme(GenericToken.SugarToken,"/")]
        Divide = 53,

   
        
        [Lexeme(GenericToken.SugarToken,"==")]
        EqualsTo = 54,
        
        [Lexeme(GenericToken.SugarToken,"!=")] Different = 55,
        
        [Lexeme(GenericToken.SugarToken,">")] Gt = 56,
        
        [Lexeme(GenericToken.SugarToken,"<")] Lt = 57,
        
        [Lexeme(GenericToken.SugarToken,">=")] Gte = 58,
        
        [Lexeme(GenericToken.SugarToken,"<=")] Lte = 59,
        

        
        
        #endregion 
        
        #region comments 101 -> 
        
        [SingleLineComment("#")] 
        SINGlE_LINE_COMMENT,
        
        [MultiLineComment("/#","#/")] 
        Multi_LINE_COMMENT,
        
        #endregion
    }
}