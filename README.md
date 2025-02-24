# MiniCompiler
A C# compiler implementation for MiniLang, a C-like language with lexical, syntax, and semantic analysis capabilities.

A robust compiler implementation for MiniLang, a small programming language with C-like syntax. This project demonstrates the complete compilation pipeline including lexical analysis, syntax analysis, and semantic analysis.

Key Features
Includes lexical analysis, syntax parsing, and semantic validation
Uses a formal grammar definition for precise language parsing
Collects and reports errors at each stage of compilation
Tracks variables, functions, and structures
Enforces strong typing rules and validates type compatibility
Identifies and validates control structures
Automatically detects recursive function calls
Ensures correct function usage with proper parameters

MiniLang supports:
Basic types: int, float, double, string, void
Variable declarations with initialization
Global and local scopes
Function definitions with parameters
Structures with fields and methods
Control structures: if-else, while, for
Arithmetic, logical, and comparison operators
Function calls with argument validation

Implementation Details
The compiler is implemented in C# and uses ANTLR4 for grammar parsing. The compilation process generates detailed output files for tokens, global variables, functions, and errors, making it ideal for educational purposes or as a foundation for more complex language implementations.
