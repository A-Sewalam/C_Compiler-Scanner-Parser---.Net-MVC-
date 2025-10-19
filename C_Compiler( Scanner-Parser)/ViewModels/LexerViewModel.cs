using C_Compiler__Scanner_Parser_.Models;

namespace C_Compiler__Scanner_Parser_.ViewModels
{
    public class LexerViewModel
    {
        public string? InputCode { get; set; }
        public List<Token>? Tokens { get; set; }
        public string? ErrorMessage { get; set; }

        public LexerViewModel()
        {
            Tokens = new List<Token>();
        }
    }
}
