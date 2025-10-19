
using C_Compiler__Scanner_Parser_.Services;
using C_Compiler__Scanner_Parser_.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace C_Compiler__Scanner_Parser_.Controllers
{
    public class ScannerController : Controller
    {
        private readonly ILexerService _lexerService;

        public ScannerController(ILexerService lexerService)
        {
            _lexerService = lexerService;
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
                viewModel.Tokens = _lexerService.GetTokens(viewModel.InputCode);
            }
            return View(viewModel);
        }

    }
}
