using Antlr4.Runtime.Misc;
using MiniLangCompiler.Models;
using System.Collections.Generic;
using Tema2LFC.Grammar;

namespace MiniLangCompiler.Compiler
{
    public class SyntaxAnalyzer : Tema2LFC.Grammar.MiniLangBaseVisitor<object>
    {
        private readonly CompilerContext _context;
        private Function _currentFunction;
        private bool _isInGlobalScope = true;

        public SyntaxAnalyzer(CompilerContext context)
        {
            _context = context;
        }

        public override object VisitProgram(Tema2LFC.Grammar.MiniLangParser.ProgramContext context)
        {
            foreach (var child in context.children)
            {
                Visit(child);
            }
            return null;
        }

        public override object VisitGlobalDeclaration(Tema2LFC.Grammar.MiniLangParser.GlobalDeclarationContext context)
        {
            if (_isInGlobalScope)
            {
                var type = context.typeSpecifier().GetText();
                var name = context.IDENTIFIER().GetText();
                string initialValue = null;

                if (context.expression() != null)
                {
                    initialValue = context.expression().GetText();
                }

                _context.GlobalVariables.Add(new Variable
                {
                    Name = name,
                    Type = type,
                    InitialValue = initialValue,
                    IsGlobal = true
                });
            }

            return null;
        }

        public override object VisitFunctionDeclaration(Tema2LFC.Grammar.MiniLangParser.FunctionDeclarationContext context)
        {
            var returnType = context.typeSpecifier().GetText();
            var functionName = context.IDENTIFIER().GetText();

            _currentFunction = new Function
            {
                Name = functionName,
                ReturnType = returnType,
                Parameters = new List<Variable>(),
                LocalVariables = new List<Variable>(),
                ControlStructures = new List<string>(),
                IsMain = functionName == "main"
            };

            _isInGlobalScope = false;

            if (context.parameterList() != null)
            {
                Visit(context.parameterList());
            }

            Visit(context.block());

            _context.Functions.Add(_currentFunction);
            _isInGlobalScope = true;
            _currentFunction = null;

            return null;
        }

        public override object VisitVariableDeclaration(Tema2LFC.Grammar.MiniLangParser.VariableDeclarationContext context)
        {
            if (!_isInGlobalScope && _currentFunction != null)
            {
                var type = context.typeSpecifier().GetText();
                var name = context.IDENTIFIER().GetText();
                string initialValue = null;

                if (context.expression() != null)
                {
                    initialValue = context.expression().GetText();
                }

                _currentFunction.LocalVariables.Add(new Variable
                {
                    Name = name,
                    Type = type,
                    InitialValue = initialValue,
                    IsGlobal = false
                });
            }

            return null;
        }

        public override object VisitIfStatement(Tema2LFC.Grammar.MiniLangParser.IfStatementContext context)
        {
            if (_currentFunction != null)
            {
                _currentFunction.ControlStructures.Add($"if-else at line {context.Start.Line}");
            }
            return base.VisitIfStatement(context);
        }

        public override object VisitWhileStatement(Tema2LFC.Grammar.MiniLangParser.WhileStatementContext context)
        {
            if (_currentFunction != null)
            {
                _currentFunction.ControlStructures.Add($"while at line {context.Start.Line}");
            }
            return base.VisitWhileStatement(context);
        }

        public override object VisitForStatement(Tema2LFC.Grammar.MiniLangParser.ForStatementContext context)
        {
            if (_currentFunction != null)
            {
                _currentFunction.ControlStructures.Add($"for at line {context.Start.Line}");
            }
            return base.VisitForStatement(context);
        }
    }
}