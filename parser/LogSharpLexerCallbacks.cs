using sly.lexer;

namespace logSharp.parser
{
    public class LogSharpLexerCallbacks
    {

        [TokenCallback((int)LogSharpToken.Identifier)]
        public static Token<LogSharpToken> TranslateIdentifier(Token<LogSharpToken> token)
        {
            if (char.IsUpper(token.Value[0]))
            {
                token.TokenID = LogSharpToken.Variable;
            }
            else {
                token.TokenID = LogSharpToken.Atom;
            }
            
            return token;
        } 
        
    }
}