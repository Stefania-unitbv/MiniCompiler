using Antlr4.Runtime.Misc;
using System.Collections.Generic;
using MiniLangCompiler.Models;

namespace Tema2LFC.Grammar
{
    public class MiniLangVisitor : MiniLangBaseVisitor<object>
    {
        private CompilationUnit _compilationUnit;
        private Function _currentFunction;
        private Structure _currentStructure;
        private bool _isInGlobalScope;
        private Stack<Dictionary<string, Variable>> _scopeStack;

        public MiniLangVisitor()
        {
            _compilationUnit = new CompilationUnit();
            _isInGlobalScope = true;
            _scopeStack = new Stack<Dictionary<string, Variable>>();
            _scopeStack.Push(new Dictionary<string, Variable>()); // Global scope
        }

        public override object VisitProgram([NotNull] MiniLangParser.ProgramContext context)
        {
            foreach (var child in context.children)
            {
                Visit(child);
            }
            return _compilationUnit;
        }

        public override object VisitGlobalDeclaration([NotNull] MiniLangParser.GlobalDeclarationContext context)
        {
            if (!_isInGlobalScope)
            {
                _compilationUnit.AddError($"Global declarations must be at file scope - Line {context.Start.Line}");
                return null;
            }

            var type = context.typeSpecifier().GetText();
            var name = context.IDENTIFIER().GetText();
            string initialValue = null;

            if (context.expression() != null)
            {
                initialValue = context.expression().GetText();
            }

            var variable = new Variable
            {
                Name = name,
                Type = type,
                InitialValue = initialValue,
                IsGlobal = true,
                DeclarationLine = context.Start.Line
            };

            _compilationUnit.GlobalVariables.Add(variable);
            _scopeStack.Peek()[name] = variable;

            return null;
        }

        public override object VisitFunctionDeclaration([NotNull] MiniLangParser.FunctionDeclarationContext context)
        {
            var returnType = context.typeSpecifier().GetText();
            var functionName = context.IDENTIFIER().GetText();

            if (!_isInGlobalScope)
            {
                _compilationUnit.AddError($"Function declarations must be at file scope - Line {context.Start.Line}");
                return null;
            }

            _currentFunction = new Function
            {
                Name = functionName,
                ReturnType = returnType,
                IsMain = functionName == "main",
                Parameters = new List<Variable>(),
                LocalVariables = new List<Variable>(),
                ControlStructures = new List<string>()
            };

            // Create new scope for function parameters and body
            _isInGlobalScope = false;
            _scopeStack.Push(new Dictionary<string, Variable>());

            if (context.parameterList() != null)
            {
                Visit(context.parameterList());
            }

            Visit(context.block());

            // Restore scope
            _scopeStack.Pop();
            _isInGlobalScope = true;

            _compilationUnit.Functions.Add(_currentFunction);
            _currentFunction = null;

            return null;
        }

        public override object VisitStructDeclaration([NotNull] MiniLangParser.StructDeclarationContext context)
        {
            if (!_isInGlobalScope)
            {
                _compilationUnit.AddError($"Struct declarations must be at file scope - Line {context.Start.Line}");
                return null;
            }

            var structName = context.IDENTIFIER().GetText();
            _currentStructure = new Structure
            {
                Name = structName,
                DeclarationLine = context.Start.Line,
                Fields = new List<Variable>(),
                Methods = new List<Function>()
            };

            foreach (var member in context.structMember())
            {
                Visit(member);
            }

            _compilationUnit.Structures.Add(_currentStructure);
            _currentStructure = null;

            return null;
        }

        public override object VisitStructField([NotNull] MiniLangParser.StructFieldContext context)
        {
            var type = context.typeSpecifier().GetText();
            var name = context.IDENTIFIER().GetText();

            _currentStructure.Fields.Add(new Variable
            {
                Name = name,
                Type = type,
                DeclarationLine = context.Start.Line
            });

            return null;
        }

        public override object VisitStructMethod([NotNull] MiniLangParser.StructMethodContext context)
        {
            var returnType = context.typeSpecifier().GetText();
            var methodName = context.IDENTIFIER().GetText();

            var method = new Function
            {
                Name = methodName,
                ReturnType = returnType,
                IsMethod = true,
                Parameters = new List<Variable>(),
                LocalVariables = new List<Variable>(),
                ControlStructures = new List<string>()
            };

            // Create new scope for method parameters and body
            _scopeStack.Push(new Dictionary<string, Variable>());

            if (context.parameterList() != null)
            {
                Visit(context.parameterList());
            }

            Visit(context.block());

            _scopeStack.Pop();

            _currentStructure.Methods.Add(method);

            return null;
        }

        public override object VisitStructConstructor([NotNull] MiniLangParser.StructConstructorContext context)
        {
            var constructorName = context.IDENTIFIER().GetText();
            if (constructorName != _currentStructure.Name)
            {
                _compilationUnit.AddError($"Constructor name must match struct name - Line {context.Start.Line}");
                return null;
            }

            var constructor = new Function
            {
                Name = constructorName,
                IsConstructor = true,
                Parameters = new List<Variable>(),
                LocalVariables = new List<Variable>(),
                ControlStructures = new List<string>()
            };

            _scopeStack.Push(new Dictionary<string, Variable>());

            if (context.parameterList() != null)
            {
                Visit(context.parameterList());
            }

            Visit(context.block());

            _scopeStack.Pop();

            _currentStructure.Methods.Add(constructor);

            return null;
        }

        public override object VisitStructDestructor([NotNull] MiniLangParser.StructDestructorContext context)
        {
            var destructorName = context.IDENTIFIER().GetText();
            if (destructorName != _currentStructure.Name)
            {
                _compilationUnit.AddError($"Destructor name must match struct name - Line {context.Start.Line}");
                return null;
            }

            var destructor = new Function
            {
                Name = destructorName,
                IsDestructor = true,
                Parameters = new List<Variable>(),
                LocalVariables = new List<Variable>(),
                ControlStructures = new List<string>()
            };

            _scopeStack.Push(new Dictionary<string, Variable>());
            Visit(context.block());
            _scopeStack.Pop();

            _currentStructure.Methods.Add(destructor);

            return null;
        }

        public override object VisitParameterList([NotNull] MiniLangParser.ParameterListContext context)
        {
            foreach (var param in context.parameter())
            {
                Visit(param);
            }
            return null;
        }

        public override object VisitParameter([NotNull] MiniLangParser.ParameterContext context)
        {
            var type = context.typeSpecifier().GetText();
            var name = context.IDENTIFIER().GetText();

            var parameter = new Variable
            {
                Name = name,
                Type = type,
                IsParameter = true,
                DeclarationLine = context.Start.Line
            };

            if (_currentFunction != null)
            {
                _currentFunction.Parameters.Add(parameter);
                _scopeStack.Peek()[name] = parameter;
            }

            return null;
        }

        public override object VisitVariableDeclaration([NotNull] MiniLangParser.VariableDeclarationContext context)
        {
            var type = context.typeSpecifier().GetText();
            var name = context.IDENTIFIER().GetText();
            string initialValue = null;

            if (context.expression() != null)
            {
                initialValue = context.expression().GetText();
            }

            var variable = new Variable
            {
                Name = name,
                Type = type,
                InitialValue = initialValue,
                IsGlobal = _isInGlobalScope,
                DeclarationLine = context.Start.Line
            };

            if (!_isInGlobalScope && _currentFunction != null)
            {
                _currentFunction.LocalVariables.Add(variable);
            }

            _scopeStack.Peek()[name] = variable;

            return null;
        }

        public override object VisitIfStatement([NotNull] MiniLangParser.IfStatementContext context)
        {
            if (_currentFunction != null)
            {
                _currentFunction.ControlStructures.Add($"if-else at line {context.Start.Line}");
            }

            _scopeStack.Push(new Dictionary<string, Variable>());
            Visit(context.statement(0));
            _scopeStack.Pop();

            if (context.statement().Length > 1)
            {
                _scopeStack.Push(new Dictionary<string, Variable>());
                Visit(context.statement(1));
                _scopeStack.Pop();
            }

            return null;
        }

        public override object VisitWhileStatement([NotNull] MiniLangParser.WhileStatementContext context)
        {
            if (_currentFunction != null)
            {
                _currentFunction.ControlStructures.Add($"while at line {context.Start.Line}");
            }

            _scopeStack.Push(new Dictionary<string, Variable>());
            Visit(context.statement());
            _scopeStack.Pop();

            return null;
        }

        public override object VisitForStatement([NotNull] MiniLangParser.ForStatementContext context)
        {
            if (_currentFunction != null)
            {
                _currentFunction.ControlStructures.Add($"for at line {context.Start.Line}");
            }

            _scopeStack.Push(new Dictionary<string, Variable>());

            if (context.forInit() != null)
                Visit(context.forInit());
            if (context.expression() != null)
                Visit(context.expression());
            if (context.forUpdate() != null)
                Visit(context.forUpdate());

            Visit(context.statement());

            _scopeStack.Pop();

            return null;
        }

        public override object VisitReturnStatement([NotNull] MiniLangParser.ReturnStatementContext context)
        {
            if (context.expression() != null)
            {
                Visit(context.expression());
            }
            return null;
        }

        public override object VisitFunctionCall([NotNull] MiniLangParser.FunctionCallContext context)
        {
            var functionName = context.IDENTIFIER().GetText();

            if (context.argumentList() != null)
            {
                Visit(context.argumentList());
            }

            // Check if this is a recursive call
            if (_currentFunction != null && functionName == _currentFunction.Name)
            {
                _currentFunction.IsRecursive = true;
            }

            return null;
        }

        // Expression handling methods - these might be expanded based on semantic analysis needs
        public override object VisitPrimaryExpr([NotNull] MiniLangParser.PrimaryExprContext context)
        {
            return Visit(context.primary());
        }

        public override object VisitPostfixExpr([NotNull] MiniLangParser.PostfixExprContext context)
        {
            return Visit(context.expression());
        }

        public override object VisitPrefixExpr([NotNull] MiniLangParser.PrefixExprContext context)
        {
            return Visit(context.expression());
        }

        public override object VisitUnaryExpr([NotNull] MiniLangParser.UnaryExprContext context)
        {
            return Visit(context.expression());
        }

        public override object VisitMultiplicativeExpr([NotNull] MiniLangParser.MultiplicativeExprContext context)
        {
            Visit(context.expression(0));
            Visit(context.expression(1));
            return null;
        }

        public override object VisitAdditiveExpr([NotNull] MiniLangParser.AdditiveExprContext context)
        {
            Visit(context.expression(0));
            Visit(context.expression(1));
            return null;
        }

        public override object VisitRelationalExpr([NotNull] MiniLangParser.RelationalExprContext context)
        {
            Visit(context.expression(0));
            Visit(context.expression(1));
            return null;
        }

        public override object VisitEqualityExpr([NotNull] MiniLangParser.EqualityExprContext context)
        {
            Visit(context.expression(0));
            Visit(context.expression(1));
            return null;
        }

        public override object VisitLogicalAndExpr([NotNull] MiniLangParser.LogicalAndExprContext context)
        {
            Visit(context.expression(0));
            Visit(context.expression(1));
            return null;
        }

        public override object VisitLogicalOrExpr([NotNull] MiniLangParser.LogicalOrExprContext context)
        {
            Visit(context.expression(0));
            Visit(context.expression(1));
            return null;
        }

        public override object VisitAssignmentExpr([NotNull] MiniLangParser.AssignmentExprContext context)
        {
            Visit(context.expression(0));
            Visit(context.expression(1));
            return null;
        }
    }
}