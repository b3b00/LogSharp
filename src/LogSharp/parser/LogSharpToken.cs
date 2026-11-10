using sly.lexer;

namespace logSharp.parser
{
    [CallBacks(typeof(LogSharpLexerCallbacks))]
    public enum LogSharpToken
    {
        #region scalars

        [Lexeme(GenericToken.Identifier, IdentifierType.AlphaNumericDash)]
        Identifier = 1,
        Variable = 2,
        Atom = 3,

        [String("\"")] String = 4,
        [String("'")] AtomicString = 41,
        

        [Int] Integer = 5,

        [Double] Double = 6,

        #endregion

        #region sugar

        [Sugar(":-")] Imply = 10,

        [Sugar("(")] LeftPar = 11,

        [Sugar(")")] RightPar = 12,

        [Sugar(",")] Comma = 13,

        [Sugar(".")] Dot = 14,

        [Sugar("[")] LeftBrack = 15,

        [Sugar("]")] RightBrack = 16,

        [Sugar("|")] Pipe = 17,

        [Sugar("?")] QuestionMark = 18,


        [SingleLineComment("%")] Comment = 19,
        
        [Sugar("!")] Cut = 191,
        
        [Sugar("-->")] DcgArrow = 192,
        
        [Sugar("{")] LeftCurly = 193,
        
        [Sugar("}")] RightCurly = 194,
        



        #endregion

        [Lexeme(GenericToken.KeyWord,"is")]
        Is = 20,

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
        
        [Lexeme(GenericToken.SugarToken,"\\=")] NotUnifiable = 60,

        [Lexeme(GenericToken.SugarToken,"=")] Equal = 61,
        

        
        
        #endregion 
        
    }
}