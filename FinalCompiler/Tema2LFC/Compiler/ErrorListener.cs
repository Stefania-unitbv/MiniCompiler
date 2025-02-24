using Antlr4.Runtime;
using System.IO;

namespace MiniLangCompiler.Compiler
{
    public class ErrorListener : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
    {
        private readonly CompilerContext _context;

        public ErrorListener(CompilerContext context)
        {
            _context = context;
        }

        // For Lexer errors
        public void SyntaxError(IRecognizer recognizer,
            int offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            var error = $"Line {line}:{charPositionInLine} - {msg}";
            _context.Errors.Add(error);
            // Write error to file immediately
            using (StreamWriter writer = File.AppendText("errors.txt"))
            {
                writer.WriteLine(error);
            }
        }

        // For Parser errors
        public void SyntaxError(IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            var error = $"Line {line}:{charPositionInLine} - {msg}";
            _context.Errors.Add(error);
            // Write error to file immediately
            using (StreamWriter writer = File.AppendText("errors.txt"))
            {
                writer.WriteLine(error);
            }
        }

        public void ReportLexicalError(int line, string msg)
        {
            var error = $"Lexical error at line {line}: {msg}";
            _context.Errors.Add(error);
            using (StreamWriter writer = File.AppendText("errors.txt"))
            {
                writer.WriteLine(error);
            }
        }

        public void ReportSyntaxError(int line, string msg)
        {
            var error = $"Syntax error at line {line}: {msg}";
            _context.Errors.Add(error);
            using (StreamWriter writer = File.AppendText("errors.txt"))
            {
                writer.WriteLine(error);
            }
        }

        public void ReportSemanticError(int line, string msg)
        {
            var error = $"Semantic error at line {line}: {msg}";
            _context.Errors.Add(error);
            using (StreamWriter writer = File.AppendText("errors.txt"))
            {
                writer.WriteLine(error);
            }
        }

        public bool HasErrors => _context.Errors.Count > 0;

        public void PrintErrors()
        {
            if (HasErrors)
            {
                System.Console.WriteLine("\nCompilation errors:");
                foreach (var error in _context.Errors)
                {
                    System.Console.WriteLine(error);
                }
            }
        }
    }
}