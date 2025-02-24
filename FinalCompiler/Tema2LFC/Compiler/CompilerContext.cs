using Antlr4.Runtime;
using System.Collections.Generic;
using System.IO;
using MiniLangCompiler.Models;
using Tema2LFC.Grammar;  // This contains MiniLangLexer and MiniLangParser
using MiniLangCompiler.Utils;

namespace MiniLangCompiler.Compiler
{
    public class CompilerContext
    {
        public List<Variable> GlobalVariables { get; } = new List<Variable>();
        public List<Function> Functions { get; } = new List<Function>();
        public List<Structure> Structures { get; } = new List<Structure>();
        public SymbolTable SymbolTable { get; } = new SymbolTable();
        public List<string> Errors { get; } = new List<string>();

        public void CompileFile(string filePath)
        {
            // Golește lista de erori pentru fiecare rulare
            Errors.Clear();

            if (string.IsNullOrEmpty(filePath))
            {
                Errors.Add("File path cannot be null or empty");
                return;
            }

            try
            {
                var fileContent = File.ReadAllText(filePath);

                // Analiza lexicală folosind LexicalAnalyzer
                var lexicalAnalyzer = new LexicalAnalyzer(this);
                var tokens=lexicalAnalyzer.Analyze(fileContent);

                // Crearea token stream-ului din tokenurile generate
                //var tokens = new CommonTokenStream(new MiniLangLexer(new AntlrInputStream(fileContent)));
                if (tokens == null)
                {
                    Errors.Add("Lexical analysis failed");
                    return;
                }

                // Crearea parser-ului
                var parser = new MiniLangParser(tokens);
                var errorListener = new ErrorListener(this);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(errorListener);

                // Parsează input-ul
                var tree = parser.program();

               // Console.WriteLine(tree.ToStringTree(parser)); // Aceasta va afișa arborele în format text

                //if (parser.NumberOfSyntaxErrors > 0)
                //{
                //    Errors.Add($"Parsing failed with {parser.NumberOfSyntaxErrors} syntax errors");
                //    return;
                //}

                // Rulează analiza sintactică și semantică
                var syntaxAnalyzer = new SyntaxAnalyzer(this);
                syntaxAnalyzer.Visit(tree);

                var semanticAnalyzer = new SemanticAnalyzer(this);
                semanticAnalyzer.Visit(tree);

                // Salvează rezultatele doar dacă nu există erori
                if (Errors.Count == 0)
                {
                    SaveResults(tokens);
                }
            }
            catch (FileNotFoundException)
            {
                Errors.Add($"File not found: {filePath}");
            }
            catch (IOException ex)
            {
                Errors.Add($"IO Error while reading file: {ex.Message}");
            }
            catch (Exception ex)
            {
                Errors.Add($"Unexpected error during compilation: {ex.Message}");
            }
        }



        public void SaveResults(CommonTokenStream tokens)
        {
            try
            {
                // Golire și scriere tokenuri
                File.WriteAllText("tokens.txt", string.Empty); // Golește fișierul
                using (var writer = new StreamWriter("tokens.txt", append: true))
                {
                    var allTokens = tokens.GetTokens();
                    foreach (var token in allTokens)
                    {
                        writer.WriteLine($"Token {token.Type} ({token.Text}) at line {token.Line}:{token.Column}");
                    }
                }

                // Golire și scriere variabile globale
                File.WriteAllText("globals.txt", string.Empty); // Golește fișierul
                using (var writer = new StreamWriter("globals.txt", append: true))
                {
                    foreach (var variable in GlobalVariables)
                    {
                        writer.WriteLine($"{variable.Type} {variable.Name}{(variable.InitialValue != null ? $" = {variable.InitialValue}" : "")};");
                    }
                }

                // Golire și scriere funcții
                File.WriteAllText("functions.txt", string.Empty); // Golește fișierul
                using (var writer = new StreamWriter("functions.txt", append: true))
                {
                    foreach (var function in Functions)
                    {
                        writer.WriteLine($"Function: {function.ReturnType} {function.Name}");
                        writer.WriteLine($"Type: {(function.IsRecursive ? "Recursive" : "Iterative")}");

                        if (function.Parameters.Count > 0)
                        {
                            writer.WriteLine("Parameters:");
                            foreach (var param in function.Parameters)
                            {
                                writer.WriteLine($"  {param.Type} {param.Name}");
                            }
                        }

                        if (function.LocalVariables.Count > 0)
                        {
                            writer.WriteLine("Local Variables:");
                            foreach (var local in function.LocalVariables)
                            {
                                writer.WriteLine($"  {local.Type} {local.Name}{(local.InitialValue != null ? $" = {local.InitialValue}" : "")};");
                            }
                        }

                        if (function.ControlStructures.Count > 0)
                        {
                            writer.WriteLine("Control Structures:");
                            foreach (var structure in function.ControlStructures)
                            {
                                writer.WriteLine($"  {structure}");
                            }
                        }

                        writer.WriteLine();
                    }
                }

                // Golire și scriere erori
                File.WriteAllText("errors.txt", string.Empty); // Golește fișierul
                if (Errors.Count > 0)
                {
                    using (var writer = new StreamWriter("errors.txt", append: true))
                    {
                        foreach (var error in Errors)
                        {
                            writer.WriteLine(error);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Errors.Add($"Error saving results: {ex.Message}");
            }
        }

    }
}