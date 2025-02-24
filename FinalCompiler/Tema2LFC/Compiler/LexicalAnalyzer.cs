using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MiniLangCompiler.Models;
using Tema2LFC.Grammar;

namespace MiniLangCompiler.Compiler
{
    public class LexicalAnalyzer
    {
        private readonly CompilerContext _context;
        private readonly List<Token> _tokens;
        private readonly HashSet<string> _keywords;

        public LexicalAnalyzer(CompilerContext context)
        {
            _context = context;
            _tokens = new List<Token>();
            _keywords = new HashSet<string>
            {
                "int", "float", "double", "string", "void",
                "if", "else", "for", "while", "return",
                "struct"
            };
        }

        public CommonTokenStream Analyze(string sourceCode)  
        {
            try
            {
                // Create input stream and lexer
                var inputStream = new AntlrInputStream(sourceCode);
                var lexer = new MiniLangLexer(inputStream);

                // Create token stream
                var tokenStream = new CommonTokenStream(lexer);
                tokenStream.Fill();

                // Process all tokens
                foreach (IToken token in tokenStream.GetTokens())
                {
                    if (token.Type == MiniLangLexer.Eof)
                        continue;

                    string tokenType = DetermineTokenType(token);
                    string lexeme = token.Text;
                    int line = token.Line;
                    int column = token.Column;

                    _tokens.Add(new Token(tokenType, lexeme, line, column));
                }

                // Save tokens to file
                SaveTokens();

                // Validate tokens
                ValidateTokens();

                // Return the token stream pentru a fi folosit de parser
                return tokenStream;
            }
            catch (Exception ex)
            {
                _context.Errors.Add($"Lexical analysis error: {ex.Message}");
                return null;
            }
        }

        private string DetermineTokenType(IToken token)
        {
            var text = token.Text;

            // Check for keywords
            if (_keywords.Contains(text))
                return "KEYWORD";

            // Check for identifiers
            if (Regex.IsMatch(text, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                return "IDENTIFIER";

            // Check for numeric literals
            if (Regex.IsMatch(text, @"^\d+$"))
                return "INTEGER_LITERAL";
            if (Regex.IsMatch(text, @"^\d*\.\d+$"))
                return "FLOAT_LITERAL";

            // Check for string literals
            if (text.StartsWith("\"") && text.EndsWith("\""))
                return "STRING_LITERAL";

            // Check for operators
            if (IsOperator(text))
                return "OPERATOR";

            // Check for delimiters
            if (IsDelimiter(text))
                return "DELIMITER";

            return "UNKNOWN";
        }

        private bool IsOperator(string text)
        {
            var operators = new HashSet<string>
            {
                "+", "-", "*", "/", "%",
                "<", ">", "<=", ">=", "==", "!=",
                "&&", "||", "!",
                "=", "+=", "-=", "*=", "/=", "%=",
                "++", "--"
            };

            return operators.Contains(text);
        }

        private bool IsDelimiter(string text)
        {
            var delimiters = new HashSet<string>
            {
                "(", ")", "{", "}", ",", ";"
            };

            return delimiters.Contains(text);
        }

        private void ValidateTokens()
        {
            for (int i = 0; i < _tokens.Count; i++)
            {
                var token = _tokens[i];

                // Validate identifiers
                if (token.Type == "IDENTIFIER")
                {
                    if (_keywords.Contains(token.Lexeme))
                    {
                        AddError($"Invalid identifier: '{token.Lexeme}' is a keyword", token.Line);
                    }
                }

                // Validate string literals
                if (token.Type == "STRING_LITERAL")
                {
                    if (!ValidateStringLiteral(token.Lexeme))
                    {
                        AddError($"Invalid string literal: {token.Lexeme}", token.Line);
                    }
                }

                // Validate numeric literals
                if (token.Type == "INTEGER_LITERAL" || token.Type == "FLOAT_LITERAL")
                {
                    if (!ValidateNumericLiteral(token.Lexeme))
                    {
                        AddError($"Invalid numeric literal: {token.Lexeme}", token.Line);
                    }
                }

                // Check for invalid sequences of operators
                if (token.Type == "OPERATOR" && i > 0 && _tokens[i - 1].Type == "OPERATOR")
                {
                    if (!IsValidOperatorSequence(_tokens[i - 1].Lexeme, token.Lexeme))
                    {
                        AddError($"Invalid operator sequence: {_tokens[i - 1].Lexeme}{token.Lexeme}", token.Line);
                    }
                }
            }
        }

        private bool ValidateStringLiteral(string literal)
        {
            if (!literal.StartsWith("\"") || !literal.EndsWith("\""))
                return false;

            var content = literal.Substring(1, literal.Length - 2);
            try
            {
              var regex = new Regex(@"\\[nrt""\\]");
return !regex.IsMatch(content) && !content.Any(c => c < 32 || c == 127);

            }
            catch
            {
                return false;
            }
        }

        private bool ValidateNumericLiteral(string literal)
        {
            // For integer literals
            if (Regex.IsMatch(literal, @"^\d+$"))
            {
                return int.TryParse(literal, out _);
            }

            // For float literals
            if (Regex.IsMatch(literal, @"^\d*\.\d+$"))
            {
                return float.TryParse(literal, out _);
            }

            return false;
        }

        private bool IsValidOperatorSequence(string op1, string op2)
        {
            // Allow increment/decrement after arithmetic operators
            if ((op1 == "+" || op1 == "-" || op1 == "*" || op1 == "/" || op1 == "%") &&
                (op2 == "++" || op2 == "--"))
                return true;

            // Allow logical not after comparison operators
            if ((op1 == "==" || op1 == "!=" || op1 == "<" || op1 == ">" || op1 == "<=" || op1 == ">=") &&
                op2 == "!")
                return true;

            return false;
        }

        private void SaveTokens()
        {
            using (var writer = new StreamWriter("tokens.txt"))
            {
                foreach (var token in _tokens)
                {
                    writer.WriteLine($"<{token.Type}, {token.Lexeme}, {token.Line}>");
                }
            }
        }

        private void AddError(string message, int line)
        {
            _context.Errors.Add($"Lexical error at line {line}: {message}");
        }

        public IReadOnlyList<Token> GetTokens() => _tokens.AsReadOnly();
    }
}