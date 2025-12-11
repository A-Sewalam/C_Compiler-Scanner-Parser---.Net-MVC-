using C_Compiler__Scanner_Parser_.Models;
using System.Collections.Generic;

namespace C_Compiler__Scanner_Parser_.Services
{
    public interface IParserService
    {
        ParserNode Parse(List<Token> tokens);
    }
}