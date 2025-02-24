using Antlr4.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace MiniLangCompiler.Models
{
    public class CompilationUnit
    {
        public string SourceFileName { get; set; }
        public List<Variable> GlobalVariables { get; set; } = new List<Variable>();
        public List<Function> Functions { get; set; } = new List<Function>();
        public List<Structure> Structures { get; set; } = new List<Structure>();
        public List<Token> Tokens { get; set; } = new List<Token>();
        public List<string> Errors { get; set; } = new List<string>();

        public bool HasMainFunction => Functions.Any(f => f.IsMain);
        public bool HasErrors => Errors.Any();

        public Function GetMainFunction()
        {
            return Functions.FirstOrDefault(f => f.IsMain);
        }

        public IEnumerable<Function> GetNonMainFunctions()
        {
            return Functions.Where(f => !f.IsMain);
        }

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public void AddToken(Token token)
        {
            Tokens.Add(token);
        }

        public List<Variable> GetInitializedGlobals()
        {
            return GlobalVariables.Where(v => v.InitialValue != null).ToList();
        }

        public List<Variable> GetUninitializedGlobals()
        {
            return GlobalVariables.Where(v => v.InitialValue == null).ToList();
        }

        public List<Function> GetRecursiveFunctions()
        {
            return Functions.Where(f => f.IsRecursive).ToList();
        }

        public List<Function> GetIterativeFunctions()
        {
            return Functions.Where(f => !f.IsRecursive).ToList();
        }

        public Structure GetStructureByName(string name)
        {
            return Structures.FirstOrDefault(s => s.Name == name);
        }

        public Function GetFunctionByName(string name)
        {
            return Functions.FirstOrDefault(f => f.Name == name);
        }

        public Variable GetGlobalVariableByName(string name)
        {
            return GlobalVariables.FirstOrDefault(v => v.Name == name);
        }

        public void SaveToFiles()
        {
            // Save tokens
            using (var writer = new System.IO.StreamWriter("tokens.txt"))
            {
                foreach (var token in Tokens)
                {
                    writer.WriteLine(token.ToString());
                }
            }

            // Save global variables
            using (var writer = new System.IO.StreamWriter("globals.txt"))
            {
                foreach (var variable in GlobalVariables)
                {
                    writer.WriteLine(variable.ToString());
                }
            }

            // Save functions
            using (var writer = new System.IO.StreamWriter("functions.txt"))
            {
                foreach (var function in Functions)
                {
                    WriteFunctionDetails(writer, function);
                }
            }

            // Save errors if any
            if (HasErrors)
            {
                using (var writer = new System.IO.StreamWriter("errors.txt"))
                {
                    foreach (var error in Errors)
                    {
                        writer.WriteLine(error);
                    }
                }
            }
        }

        private void WriteFunctionDetails(System.IO.StreamWriter writer, Function function)
        {
            writer.WriteLine($"Function: {function}");
            writer.WriteLine($"Type: {(function.IsRecursive ? "Recursive" : "Iterative")}");
            writer.WriteLine($"Is Main: {function.IsMain}");

            writer.WriteLine("Parameters:");
            foreach (var param in function.Parameters)
            {
                writer.WriteLine($"  {param}");
            }

            writer.WriteLine("Local Variables:");
            foreach (var local in function.LocalVariables)
            {
                writer.WriteLine($"  {local}");
            }

            writer.WriteLine("Control Structures:");
            foreach (var structure in function.ControlStructures)
            {
                writer.WriteLine($"  {structure}");
            }

            writer.WriteLine();
        }
    }
}