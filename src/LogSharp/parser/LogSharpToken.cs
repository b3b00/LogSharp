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



        #endregion


    }
}