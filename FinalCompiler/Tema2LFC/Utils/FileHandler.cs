using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Antlr4.Runtime;
using MiniLangCompiler.Models;

namespace MiniLangCompiler.Utils
{
    public class FileHandler
    {
        private readonly string _baseOutputPath;

        public FileHandler(string baseOutputPath = "")
        {
            _baseOutputPath = baseOutputPath;
            InitializeOutputDirectory();
        }

        private void InitializeOutputDirectory()
        {
            if (!string.IsNullOrEmpty(_baseOutputPath) && !Directory.Exists(_baseOutputPath))
            {
                Directory.CreateDirectory(_baseOutputPath);
            }
        }

        public string ReadSourceFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading source file: {ex.Message}");
            }
        }

        public void SaveTokens(List<Token> tokens)
        {
            var path = Path.Combine(_baseOutputPath, "tokens.txt");
            using (var writer = new StreamWriter(path))
            {
                foreach (var token in tokens)
                {
                    writer.WriteLine($"<{token.Type}, {token.Lexeme}, {token.Line}>");
                }
            }
        }

        public void SaveGlobalVariables(List<Variable> variables)
        {
            var path = Path.Combine(_baseOutputPath, "globals.txt");
            using (var writer = new StreamWriter(path))
            {
                foreach (var variable in variables)
                {
                    var initValue = string.IsNullOrEmpty(variable.InitialValue) ? "" : $" = {variable.InitialValue}";
                    writer.WriteLine($"{variable.Type} {variable.Name}{initValue};");
                }
            }
        }

        public void SaveFunctions(List<Function> functions)
        {
            var path = Path.Combine(_baseOutputPath, "functions.txt");
            using (var writer = new StreamWriter(path))
            {
                foreach (var function in functions)
                {
                    WriteFunctionDetails(writer, function);
                    writer.WriteLine();
                }
            }
        }

        private void WriteFunctionDetails(StreamWriter writer, Function function)
        {
            writer.WriteLine($"Function: {function.ReturnType} {function.Name}");
            writer.WriteLine($"Type: {(function.IsRecursive ? "Recursive" : "Iterative")}");
            writer.WriteLine($"Is Main: {function.IsMain}");

            writer.WriteLine("Parameters:");
            foreach (var param in function.Parameters)
            {
                writer.WriteLine($"  {param.Type} {param.Name}");
            }

            writer.WriteLine("Local Variables:");
            foreach (var local in function.LocalVariables)
            {
                var initValue = string.IsNullOrEmpty(local.InitialValue) ? "" : $" = {local.InitialValue}";
                writer.WriteLine($"  {local.Type} {local.Name}{initValue}");
            }

            writer.WriteLine("Control Structures:");
            foreach (var structure in function.ControlStructures)
            {
                writer.WriteLine($"  {structure}");
            }
        }

        public void SaveStructures(List<Structure> structures)
        {
            var path = Path.Combine(_baseOutputPath, "structures.txt");
            using (var writer = new StreamWriter(path))
            {
                foreach (var structure in structures)
                {
                    WriteStructureDetails(writer, structure);
                    writer.WriteLine();
                }
            }
        }

        private void WriteStructureDetails(StreamWriter writer, Structure structure)
        {
            writer.WriteLine($"Structure: {structure.Name}");

            writer.WriteLine("Fields:");
            foreach (var field in structure.Fields)
            {
                writer.WriteLine($"  {field.Type} {field.Name}");
            }

            writer.WriteLine("Methods:");
            foreach (var method in structure.Methods)
            {
                if (method.IsConstructor)
                {
                    writer.WriteLine($"  Constructor: {method.Name}");
                }
                else if (method.IsDestructor)
                {
                    writer.WriteLine($"  Destructor: ~{method.Name}");
                }
                else
                {
                    writer.WriteLine($"  Method: {method.ReturnType} {method.Name}");
                }
            }
        }

        public void SaveErrors(List<string> errors)
        {
            if (errors.Count == 0) return;

            var path = Path.Combine(_baseOutputPath, "errors.txt");
            using (var writer = new StreamWriter(path))
            {
                foreach (var error in errors)
                {
                    writer.WriteLine(error);
                }
            }
        }

        public string RemoveComments(string sourceCode)
        {
            var output = new StringBuilder();
            var inLineComment = false;
            var inBlockComment = false;

            for (int i = 0; i < sourceCode.Length; i++)
            {
                if (inLineComment)
                {
                    if (sourceCode[i] == '\n')
                    {
                        inLineComment = false;
                        output.Append('\n');
                    }
                    continue;
                }

                if (inBlockComment)
                {
                    if (i < sourceCode.Length - 1 &&
                        sourceCode[i] == '*' &&
                        sourceCode[i + 1] == '/')
                    {
                        inBlockComment = false;
                        i++;
                    }
                    continue;
                }

                if (i < sourceCode.Length - 1)
                {
                    if (sourceCode[i] == '/' && sourceCode[i + 1] == '/')
                    {
                        inLineComment = true;
                        i++;
                        continue;
                    }

                    if (sourceCode[i] == '/' && sourceCode[i + 1] == '*')
                    {
                        inBlockComment = true;
                        i++;
                        continue;
                    }
                }

                if (!inLineComment && !inBlockComment)
                {
                    output.Append(sourceCode[i]);
                }
            }

            return output.ToString();
        }

        public string NormalizeWhitespace(string code)
        {
            // Replace all whitespace sequences with a single space
            var normalized = new StringBuilder();
            bool lastWasWhitespace = false;

            foreach (char c in code)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!lastWasWhitespace)
                    {
                        normalized.Append(' ');
                        lastWasWhitespace = true;
                    }
                }
                else
                {
                    normalized.Append(c);
                    lastWasWhitespace = false;
                }
            }

            return normalized.ToString().Trim();
        }
    }
}