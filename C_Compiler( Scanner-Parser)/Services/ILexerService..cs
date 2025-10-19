using C_Compiler__Scanner_Parser_.Models;

namespace C_Compiler__Scanner_Parser_.Services
{
    public interface ILexerService
    {
        List<Token> GetTokens(string code);
    }
}
