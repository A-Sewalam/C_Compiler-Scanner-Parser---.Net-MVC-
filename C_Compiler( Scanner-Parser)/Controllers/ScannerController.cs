using C_Compiler__Scanner_Parser_.Services;
using C_Compiler__Scanner_Parser_.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace C_Compiler__Scanner_Parser_.Controllers
{
    public class ScannerController : Controller
    {
        private readonly ILexerService _lexerService;
        private readonly IParserService _parserService; // 1. Add Field

        // 2. Inject IParserService in Constructor
        public ScannerController(ILexerService lexerService, IParserService parserService)
        {
            _lexerService = lexerService;
            _parserService = parserService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new LexerViewModel
            {
                InputCode =
                              @"int main() {
                  int x,y;
                  // This is a single-line comment
                  if (x == 42) {
                      /* This is
                         a block
                         comment */
                      x = x-3;
                  } else {
                      y = 3.1; // Another comment
                  }
                  return 0;
              }"
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Index(LexerViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.InputCode))
            {
                try
                {
                    // 1. Get Tokens
                    viewModel.Tokens = _lexerService.GetTokens(viewModel.InputCode);

                    // 2. Parse Tokens (Add this logic)
                    if (viewModel.Tokens != null && viewModel.Tokens.Any())
                    {
                        var rootNode = _parserService.Parse(viewModel.Tokens);
                        // Convert the tree to a string representation
                        viewModel.ParseTreeOutput = rootNode.Print();
                    }
                }
                catch (Exception ex)
                {
                    // Catch parsing errors (e.g., Syntax Error)
                    viewModel.ErrorMessage = "Parsing Error: " + ex.Message;
                }
            }
            return View(viewModel);
        }
    }
}