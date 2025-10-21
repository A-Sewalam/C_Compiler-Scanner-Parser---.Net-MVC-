using C_Compiler__Scanner_Parser_.Models;

namespace C_Compiler__Scanner_Parser_.Services
{
    public class LexerService : ILexerService
    {
        private readonly HashSet<string> _keywords = new HashSet<string> { "int", "return", "if", "else", "main", "void", "char", "float", "double", "for", "while", "do" };
        private readonly HashSet<char> _specialChars = new HashSet<char> { '(', ')', '{', '}', ';', ',' };
        private readonly HashSet<string> _operators = new HashSet<string> { "+", "-", "*", "/", "=", "==", "!=", "<", ">", "<=", ">=" };

        public List<Token> GetTokens(string code)
        {
            var tokens = new List<Token>();
            var codeWithoutComments = RemoveComments(code);
            int position = 0;

            while (position < codeWithoutComments.Length)
            {
                if (char.IsWhiteSpace(codeWithoutComments[position]))
                {
                    position++;
                    continue;
                }

                if (TryMatchKeywordOrIdentifier(codeWithoutComments, ref position, out var token))
                {
                    tokens.Add(token);
                }
                else if (TryMatchNumberLiteral(codeWithoutComments, ref position, out token))
                {
                    tokens.Add(token);
                }
                else if (TryMatchOperator(codeWithoutComments, ref position, out token))
                {
                    tokens.Add(token);
                }
                else if (TryMatchSpecialCharacter(codeWithoutComments, ref position, out token))
                {
                    tokens.Add(token);
                }
                else
                {
                   
                    tokens.Add(new Token(TokenType.UNKNOWN, codeWithoutComments[position].ToString()));
                    position++;
                }
            }

            return tokens;
        }

        private string RemoveComments(string code)
        {
            string result = "";
            bool inSingleLineComment = false;
            bool inMultiLineComment = false;

            for (int i = 0; i < code.Length; i++)
            {
                if (i + 1 < code.Length)
                {
                    if (code[i] == '/' && code[i + 1] == '/')
                    {
                        inSingleLineComment = true;
                        i++; 
                        continue;
                    }
                    if (code[i] == '/' && code[i + 1] == '*')
                    {
                        inMultiLineComment = true;
                        i++; 
                        continue;
                    }
                }

                if (inSingleLineComment)
                {
                    if (code[i] == '\n')
                    {
                        inSingleLineComment = false;
                        result += code[i]; 
                    }
                    continue;
                }

                if (inMultiLineComment)
                {
                    if (i + 1 < code.Length && code[i] == '*' && code[i + 1] == '/')
                    {
                        inMultiLineComment = false;
                        i++; 
                    }
                    continue;
                }

                result += code[i];
            }

            return result;
        }

        private bool TryMatchKeywordOrIdentifier(string code, ref int position, out Token token)
        {
            if (char.IsLetter(code[position]) || code[position] == '_')
            {
                var start = position;
                while (position < code.Length && (char.IsLetterOrDigit(code[position]) || code[position] == '_'))
                {
                    position++;
                }
                var value = code.Substring(start, position - start);
                var type = _keywords.Contains(value) ? TokenType.KEYWORD : TokenType.IDENTIFIER;
                token = new Token(type, value);
                return true;
            }
            token = null;
            return false;
        }

        private bool TryMatchNumberLiteral(string code, ref int position, out Token token)
        {
            if (char.IsDigit(code[position]))
            {
                var start = position;
                bool hasDecimal = false;
                while (position < code.Length && (char.IsDigit(code[position]) || code[position] == '.'))
                {
                    if (code[position] == '.')
                    {
                        if (hasDecimal) break;
                        hasDecimal = true;
                    }
                    position++;
                }
                var value = code.Substring(start, position - start);
                var type = hasDecimal ? TokenType.FLOAT_LITERAL : TokenType.INTEGER_LITERAL;
                token = new Token(type, value);
                return true;
            }
            token = null;
            return false;
        }

        private bool TryMatchOperator(string code, ref int position, out Token token)
        {
         
            if (position + 1 < code.Length)
            {
                string twoCharOp = code.Substring(position, 2);
                if (_operators.Contains(twoCharOp))
                {
                    token = new Token(TokenType.OPERATOR, twoCharOp);
                    position += 2;
                    return true;
                }
            }

            
            string oneCharOp = code[position].ToString();
            if (_operators.Contains(oneCharOp))
            {
                token = new Token(TokenType.OPERATOR, oneCharOp);
                position += 1;
                return true;
            }

            token = null;
            return false;
        }

        private bool TryMatchSpecialCharacter(string code, ref int position, out Token token)
        {
            if (_specialChars.Contains(code[position]))
            {
                token = new Token(TokenType.SPECIAL_CHARACTER, code[position].ToString());
                position++;
                return true;
            }
            token = null;
            return false;
        }
    }
}
