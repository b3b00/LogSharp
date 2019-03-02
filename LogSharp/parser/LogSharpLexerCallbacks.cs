using sly.lexer;

namespace logSharp.parser
{
    public class LogSharpLexerCallbacks
    {
        [TokenCallback((int) LogSharpToken.Identifier)]
        public static Token<LogSharpToken> TranslateIdentifier(Token<LogSharpToken> token)
        {
            var leadingChar = token.Value[0];

            if (char.IsUpper(leadingChar) || leadingChar == '_')
                token.TokenID = LogSharpToken.Variable;
            else
                token.TokenID = LogSharpToken.Atom;

            return token;
        }
    }
}