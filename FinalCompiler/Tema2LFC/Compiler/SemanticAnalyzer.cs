using Antlr4.Runtime.Misc;
using System.Collections.Generic;
using System.Linq;
using MiniLangCompiler.Models;
using Tema2LFC.Grammar;
using Antlr4.Runtime;

namespace MiniLangCompiler.Compiler
{
    public class SemanticAnalyzer : MiniLangBaseVisitor<object>
    {
        private readonly CompilerContext _context;
        private readonly Stack<Dictionary<string, Variable>> _scopeStack;
        private Function _currentFunction;
        private Structure _currentStructure;
        private HashSet<string> _calledFunctions;
        private bool _isInGlobalScope;
        private readonly HashSet<string> _validTypes;
        private HashSet<string> _processedFunctions; 


        public SemanticAnalyzer(CompilerContext context)
        {
            _context = context;
            _scopeStack = new Stack<Dictionary<string, Variable>>();
            _scopeStack.Push(new Dictionary<string, Variable>());
            _calledFunctions = new HashSet<string>();
            _isInGlobalScope = true;
            _validTypes = new HashSet<string> { "int", "float", "double", "string", "void" };
            _processedFunctions = new HashSet<string>(); 
        }

        public override object VisitProgram(MiniLangParser.ProgramContext context)
        {
          
            _context.Functions.Clear();
             _processedFunctions.Clear();

      

            foreach (var child in context.children)
            {
               
                Visit(child);
            
            }

            var mainFunction = _context.Functions.FirstOrDefault(f => f.Name == "main");
            if (mainFunction == null)
            {
                AddError("Program must have a 'main' function", 1);
            }
            else if (mainFunction.ReturnType != "int")
            {
                AddError("Function 'main' must return int", 1);
            }

            return null;
        }
        public override object VisitFunctionDeclaration(MiniLangParser.FunctionDeclarationContext context)
        {
            var returnType = context.typeSpecifier().GetText();
            var functionName = context.IDENTIFIER().GetText();
            var paramCount = context.parameterList()?.parameter().Length ?? 0;

            string functionSignature = $"{functionName}_{paramCount}";

            if (_processedFunctions.Contains(functionSignature))
            {
                AddError($"Function '{functionName}' is already defined with {paramCount} parameters", context.Start.Line);
                return null;
            }

            _processedFunctions.Add(functionSignature);

            _currentFunction = new Function
            {
                Name = functionName,
                ReturnType = returnType,
                Parameters = new List<Variable>(),
                LocalVariables = new List<Variable>(),
                ControlStructures = new List<string>()
            };

            _isInGlobalScope = false;

            if (context.parameterList() != null)
            {
                Visit(context.parameterList());
            }
            _context.Functions.Add(_currentFunction); ////////////////ai modificat aici , functia era pusa dupa visit context.block
            Visit(context.block());
           
            _currentFunction = null;
            _isInGlobalScope = true;

            return null;
        }


        public override object VisitParameterList(MiniLangParser.ParameterListContext context)
        {
            if (context.parameter() != null)
            {
                foreach (var param in context.parameter())
                {
                    var type = param.typeSpecifier().GetText();
                    var name = param.IDENTIFIER().GetText();

                    _currentFunction.Parameters.Add(new Variable
                    {
                        Name = name,
                        Type = type,
                        IsGlobal = false
                    });
                }
            }
            return null;
        }
        public override object VisitAdditiveExpr(MiniLangParser.AdditiveExprContext context)
        {
            var leftType = GetExpressionType(context.expression(0));
            var rightType = GetExpressionType(context.expression(1));
            if (leftType == "int" && rightType == "int")
            {
                return "int";
            }
            if (leftType == "float" || rightType == "float")
            {
                return "float";
            }
            if (leftType == "double" || rightType == "double")
            {
                return "double";
            }

            return "unknown";
        }

        public override object VisitPrimaryExpr(MiniLangParser.PrimaryExprContext context)
        {
            return GetExpressionType(context);
        }

        public override object VisitIdentifierExpr(MiniLangParser.IdentifierExprContext context)
        {
            var name = context.IDENTIFIER().GetText();
            return GetVariableType(name);
        }
        public override object VisitIfStatement(MiniLangParser.IfStatementContext context)
        {
            // Verifică dacă statement-ul este doar ;
            if (context.statement(0) != null &&
                context.statement(0).GetText().Trim() == ";")
            {
                AddError("Empty if statement (just ;) is not allowed", context.Start.Line);
                return null;
            }

            // Verificarea condiției
            if (context.expression() != null)
            {
                var conditionType = GetExpressionType(context.expression());
                if (conditionType == "unknown")
                {
                    AddError("Invalid condition in if statement", context.Start.Line);
                }
            }

            // Vizitează statement-ul principal (if)
            if (context.statement(0) != null)
            {
                Visit(context.statement(0));
            }

            // Verifică și vizitează statement-ul else dacă există
            if (context.statement(1) != null)
            {
                Visit(context.statement(1));
            }

            if (_currentFunction != null)
            {
                _currentFunction.ControlStructures.Add($"if-else at line {context.Start.Line}");
            }

            return null;
        }
        public override object VisitVariableDeclaration(MiniLangParser.VariableDeclarationContext context)
        {
            var type = context.typeSpecifier().GetText();
            var name = context.IDENTIFIER().GetText();

            if (!_validTypes.Contains(type))
            {
                AddError($"Invalid type '{type}'", context.Start.Line);
                return null;
            }
            if (_scopeStack.Peek().ContainsKey(name))
            {
                AddError($"Variable '{name}' is already declared in this scope", context.Start.Line);
                return null;
            }
            if (_currentFunction != null && _currentFunction.Parameters.Any(p => p.Name == name))
            {
                AddError($"Variable '{name}' conflicts with function parameter", context.Start.Line);
                return null;
            }

            if (context.expression() != null)
            {
                // Folosim GetExpressionType în loc de DetermineValueType
                var valueType = GetExpressionType(context.expression());

                if (!IsTypeCompatible(type, valueType))
                {
                    AddError($"Cannot initialize variable of type '{type}' with value of type '{valueType}'", context.Start.Line);
                    return null;
                }
            }

            var variable = new Variable
            {
                Name = name,
                Type = type,
                IsGlobal = _isInGlobalScope,
                InitialValue = context.expression()?.GetText()
            };

            _scopeStack.Peek()[name] = variable;

            if (_isInGlobalScope)
                _context.GlobalVariables.Add(variable);
            else if (_currentFunction != null)
                _currentFunction.LocalVariables.Add(variable);

            return null;
        }
        public override object VisitVariableDeclarationNoSemi(MiniLangParser.VariableDeclarationNoSemiContext context)
        {
            Visit(context.typeSpecifier());

            if (context.expression() != null)
            {
                Visit(context.expression());
            }

            // Apoi putem refolosi GetText() și altele pentru aceleași câmpuri
            var type = context.typeSpecifier().GetText();
            var name = context.IDENTIFIER().GetText();

            // Refolosim direct metodele existente pentru verificări
            var variable = new Variable
            {
                Name = name,
                Type = type,
                IsGlobal = _isInGlobalScope,
                InitialValue = context.expression()?.GetText()
            };

            _scopeStack.Peek()[name] = variable;

            if (_isInGlobalScope)
                _context.GlobalVariables.Add(variable);
            else if (_currentFunction != null)
                _currentFunction.LocalVariables.Add(variable);

            return null;
        }

   
        public override object VisitAssignmentExpr(MiniLangParser.AssignmentExprContext context) 
        {
            var leftSide = context.expression(0).GetText();
            var rightSide = context.expression(1).GetText();

            var leftType = GetVariableType(leftSide);


            if (leftType == "unknown")
            {
                return null; 
            }

       
            if (rightSide.Contains("(") && rightSide.Contains(")"))
            {
                var functionName = rightSide.Substring(0, rightSide.IndexOf('('));
                var function = _context.Functions.FirstOrDefault(f => f.Name == functionName);
                if (function != null)
                {
                    if (!IsTypeCompatible(leftType, function.ReturnType))
                    {
                        AddError($"Cannot assign return value of function '{functionName}' (type {function.ReturnType}) to variable of type {leftType}",
                            context.Start.Line);
                    }
                }
                else
                {
                    AddError($"Function '{functionName}' is not defined", context.Start.Line);
                }
            }
            else
            {
                var rightType = GetExpressionType(context.expression(1));

                if (!IsTypeCompatible(leftType, rightType))
                {
                    AddError($"Cannot assign value of type '{rightType}' to variable of type '{leftType}'",
                        context.Start.Line);
                }
            }

            return leftType; 
        }




        private string ValidateFunctionCall(string functionName, List<MiniLangParser.ExpressionContext> arguments, int line)
        {
            var function = _context.Functions.FirstOrDefault(f => f.Name == functionName);
            if (function == null)
            {
                AddError($"Called function '{functionName}' is not defined", line);
                return "unknown";
            }

            if (arguments.Count != function.Parameters.Count)
            {
                AddError($"Function '{functionName}' expects {function.Parameters.Count} arguments but got {arguments.Count}", line);
                return "unknown";
            }

            for (int i = 0; i < arguments.Count; i++)
            {
                var argumentType = GetExpressionType(arguments[i]);
                var parameterType = function.Parameters[i].Type;

                if (argumentType == "unknown")
                {
                    AddError($"Cannot determine type of argument {i + 1} in function call to '{functionName}'", line);
                    return "unknown";
                }

                if (!IsTypeCompatible(parameterType, argumentType))
                {
                    AddError($"Argument {i + 1} of function '{functionName}' expects type '{parameterType}' but got '{argumentType}'", line);
                }
            }

            // If this is a recursive call within the same function, mark it as recursive
            if (_currentFunction != null && functionName == _currentFunction.Name)
            {
                _currentFunction.IsRecursive = true;
            }

            return function.ReturnType;


        }

        public override object VisitFunctionCall(MiniLangParser.FunctionCallContext context)
        {
            var functionName = context.IDENTIFIER().GetText();
            var arguments = context.argumentList()?.expression().ToList() ?? new List<MiniLangParser.ExpressionContext>();

            return ValidateFunctionCall(functionName, arguments, context.Start.Line);
        }

        private string GetExpressionType(MiniLangParser.ExpressionContext context)
        {
            // Pentru && și ||
            if (context is MiniLangParser.LogicalAndExprContext logicAnd)
            {
                var leftType = GetExpressionType(logicAnd.expression(0));
                var rightType = GetExpressionType(logicAnd.expression(1));

                if (leftType != "unknown" && rightType != "unknown")
                {
                    return "int"; // Operațiile logice returnează int (folosit ca boolean)
                }
                return "unknown";
            }

            if (context is MiniLangParser.LogicalOrExprContext logicOr)
            {
                var leftType = GetExpressionType(logicOr.expression(0));
                var rightType = GetExpressionType(logicOr.expression(1));

                if (leftType != "unknown" && rightType != "unknown")
                {
                    return "int";
                }
                return "unknown";
            }

            // Pentru operații relaționale (<, >, <=, >=)
            if (context is MiniLangParser.RelationalExprContext relExpr)
            {
                var leftType = GetExpressionType(relExpr.expression(0));
                var rightType = GetExpressionType(relExpr.expression(1));

                if (leftType == "unknown" || rightType == "unknown")
                {
                    return "unknown";
                }

                if (!IsValidComparisonTypes(leftType, rightType))
                {
                    AddError($"Cannot compare values of types '{leftType}' and '{rightType}'", relExpr.Start.Line);
                    return "unknown";
                }

                return "int";
            }

            // Pentru operații de egalitate (==, !=)
            if (context is MiniLangParser.EqualityExprContext eqExpr)
            {
                var leftType = GetExpressionType(eqExpr.expression(0));
                var rightType = GetExpressionType(eqExpr.expression(1));

                if (leftType == "unknown" || rightType == "unknown")
                {
                    return "unknown";
                }

                if (!IsValidComparisonTypes(leftType, rightType))
                {
                    AddError($"Cannot compare values of types '{leftType}' and '{rightType}'", eqExpr.Start.Line);
                    return "unknown";
                }

                return "int";
            }

            // Pentru expresii postfix (i++, i--)
            if (context is MiniLangParser.PostfixExprContext postExpr)
            {
                var operandType = GetExpressionType(postExpr.expression());
                if (operandType == "int" || operandType == "float" || operandType == "double")
                {
                    return operandType;
                }
                AddError($"Cannot apply {postExpr.op.Text} to type {operandType}", postExpr.Start.Line);
                return "unknown";
            }

            // Pentru expresii prefix (++i, --i)
            if (context is MiniLangParser.PrefixExprContext prefExpr)
            {
                var operandType = GetExpressionType(prefExpr.expression());
                if (operandType == "int" || operandType == "float" || operandType == "double")
                {
                    return operandType;
                }
                AddError($"Cannot apply {prefExpr.op.Text} to type {operandType}", prefExpr.Start.Line);
                return "unknown";
            }

            // Pentru expresii aditive
            if (context is MiniLangParser.AdditiveExprContext addExpr)
            {
                var leftType = GetExpressionType(addExpr.expression(0));
                if (leftType == "unknown") return "unknown";
                var rightType = GetExpressionType(addExpr.expression(1));
                if (leftType == "int" && rightType == "int") return "int";
                if (leftType == "float" || rightType == "float") return "float";
                if (leftType == "double" || rightType == "double") return "double";
                return "unknown";
            }

            // Pentru expresii multiplicative
            if (context is MiniLangParser.MultiplicativeExprContext multExpr)
            {
                var leftType = GetExpressionType(multExpr.expression(0));
                var rightType = GetExpressionType(multExpr.expression(1));
                if (leftType == "int" && rightType == "int") return "int";
                if (leftType == "float" || rightType == "float") return "float";
                if (leftType == "double" || rightType == "double") return "double";
                return "unknown";
            }

            // Pentru expresii primare
            if (context is MiniLangParser.PrimaryExprContext primaryExpr)
            {
                var primary = primaryExpr.primary();
                if (primary is MiniLangParser.IntLiteralContext) return "int";
                if (primary is MiniLangParser.FloatLiteralContext) return "float";
                if (primary is MiniLangParser.StringLiteralContext) return "string";

                if (primary is MiniLangParser.FunctionCallContext funcCall)
                {
                    var functionName = funcCall.IDENTIFIER().GetText();
                    var arguments = funcCall.argumentList()?.expression().ToList()
                                  ?? new List<MiniLangParser.ExpressionContext>();
                    return ValidateFunctionCall(functionName, arguments, funcCall.Start.Line);
                }

                if (primary is MiniLangParser.IdentifierExprContext idExpr)
                {
                    var name = idExpr.IDENTIFIER().GetText();
                    var variable = _context.GlobalVariables.FirstOrDefault(v => v.Name == name);
                    if (variable != null) return variable.Type;
                    if (_currentFunction != null)
                    {
                        var param = _currentFunction.Parameters.FirstOrDefault(p => p.Name == name);
                        if (param != null) return param.Type;
                        var localVar = _currentFunction.LocalVariables.FirstOrDefault(v => v.Name == name);
                        if (localVar != null) return localVar.Type;
                    }
                    AddError($"Variable '{name}' is not declared", idExpr.Start.Line);
                }
            }

            return "unknown";
        }

        private bool IsValidComparisonTypes(string type1, string type2)
        {
            // Același tip
            if (type1 == type2) return true;

            // Tipuri numerice pot fi comparate între ele
            var numericTypes = new HashSet<string> { "int", "float", "double" };
            if (numericTypes.Contains(type1) && numericTypes.Contains(type2))
                return true;

            // String-urile pot fi comparate doar cu string-uri
            if (type1 == "string" || type2 == "string")
                return type1 == type2;

            return false;
        }
        private bool IsTypeCompatible(string targetType, string valueType)
        {
            if (targetType == valueType) return true;
            if (targetType == "double" && (valueType == "float" || valueType == "int")) return true;
            if (targetType == "float" && valueType == "int") return true;

            return false;
        }

        private string DetermineValueType(string value, int line = 0)
        {
            // Pentru apeluri de funcții, ar trebui să evităm acest caz
            // și să folosim în schimb GetExpressionType cu ExpressionContext
            if (value.Contains("(") && value.Contains(")"))
            {
                // Acest caz ar trebui tratat în GetExpressionType
                return "unknown";
            }

            if (value.StartsWith("\"") && value.EndsWith("\"")) return "string";
            if (value.Contains(".")) return "float";
            if (int.TryParse(value, out _)) return "int";

            // Caută în variabile
            var variable = _context.GlobalVariables.FirstOrDefault(v => v.Name == value);
            if (variable != null) return variable.Type;

            if (_currentFunction != null)
            {
                var localVar = _currentFunction.LocalVariables.FirstOrDefault(v => v.Name == value);
                if (localVar != null) return localVar.Type;

                var param = _currentFunction.Parameters.FirstOrDefault(p => p.Name == value);
                if (param != null) return param.Type;
            }

            return "unknown";
        }
        private string GetVariableType(string name)
        {
            // 1. Verificăm mai întâi în scope stack (inclusiv scope-ul for-ului)
            foreach (var scope in _scopeStack)
            {
                if (scope.TryGetValue(name, out var variable))
                {
                    return variable.Type;
                }
            }

            // 2. Verificăm în parametrii funcției curente
            if (_currentFunction != null)
            {
                var param = _currentFunction.Parameters.FirstOrDefault(p => p.Name == name);
                if (param != null) return param.Type;

                var localVar = _currentFunction.LocalVariables.FirstOrDefault(v => v.Name == name);
                if (localVar != null) return localVar.Type;
            }

            // 3. Verificăm în variabilele globale
            var globalVar = _context.GlobalVariables.FirstOrDefault(v => v.Name == name);
            if (globalVar != null) return globalVar.Type;

            // 4. Dacă nu găsim variabila, adăugăm o eroare
            AddError($"Variable '{name}' is not declared", 0);
            return "unknown";
        }
      
        public override object VisitForStatement(MiniLangParser.ForStatementContext context)
        {
            // Verifică dacă există ; după paranteza închisă
            var forText = context.GetText();
            var lastParenIndex = forText.LastIndexOf(')');
            if (lastParenIndex < forText.Length - 1)
            {
                var textAfterParen = forText.Substring(lastParenIndex + 1).Trim();
                if (textAfterParen == ";")
                {
                    AddError("Invalid for statement: empty statement (';') is not allowed", context.Start.Line);
                    return null;
                }
            }

            // Restul logicii existente
            _scopeStack.Push(new Dictionary<string, Variable>());

            try
            {
                if (context.forInit() != null)
                {
                    Visit(context.forInit());
                }

                if (context.expression() != null)
                {
                    var conditionType = GetExpressionType(context.expression());
                    if (conditionType != "int")
                    {
                        AddError($"For condition must be of type int, got {conditionType}", context.Start.Line);
                    }
                }

                if (context.forUpdate() != null)
                {
                    Visit(context.forUpdate());
                }

                if (context.statement() != null)
                {
                    Visit(context.statement());
                }

                if (_currentFunction != null)
                {
                    _currentFunction.ControlStructures.Add($"for at line {context.Start.Line}");
                }
            }
            finally
            {
                _scopeStack.Pop();
            }

            return null;
        }
        public override object VisitReturnStatement(MiniLangParser.ReturnStatementContext context)
        {
            if (_currentFunction == null) return null;

            // Verifică dacă funcția void are return cu valoare
            if (_currentFunction.ReturnType == "void" && context.expression() != null)
            {
                AddError($"Void function '{_currentFunction.Name}' cannot return a value", context.Start.Line);
                return null;
            }

            // Verifică dacă funcția non-void are return fără valoare
            if (_currentFunction.ReturnType != "void" && context.expression() == null)
            {
                AddError($"Function '{_currentFunction.Name}' must return a value of type {_currentFunction.ReturnType}",
                    context.Start.Line);
                return null;
            }

            // Verifică dacă tipul returnat se potrivește cu tipul funcției
            if (context.expression() != null)
            {
                var returnValueType = GetExpressionType(context.expression());
                if (!IsTypeCompatible(_currentFunction.ReturnType, returnValueType))
                {
                    AddError($"Cannot return value of type '{returnValueType}' from function of type '{_currentFunction.ReturnType}'",
                        context.Start.Line);
                }
            }

            return null;
        }



        private void AddError(string message, int line)
        {
            _context.Errors.Add($"Semantic error at line {line}: {message}");
        }
    }
}