using MiniLangCompiler.Compiler;
using System;

namespace MiniLangCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var context = new CompilerContext();

                // Compile the input file
                context.CompileFile("input.txt");

                // Display results
                if (context.Errors.Count == 0)
                {
                    Console.WriteLine("Compilation successful!");
                }
                else
                {
                    Console.WriteLine("Compilation failed with errors:");
                    foreach (var error in context.Errors)
                    {
                        Console.WriteLine(error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
