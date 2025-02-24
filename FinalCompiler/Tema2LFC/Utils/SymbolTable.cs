using System.Collections.Generic;
using System.Linq;
using MiniLangCompiler.Models;

namespace MiniLangCompiler.Utils
{
    public class SymbolTable
    {
        private class Scope
        {
            public Dictionary<string, Variable> Variables { get; } = new Dictionary<string, Variable>();
            public Dictionary<string, Function> Functions { get; } = new Dictionary<string, Function>();
            public Dictionary<string, Structure> Structures { get; } = new Dictionary<string, Structure>();
            public Scope Parent { get; set; }
        }

        private Scope _currentScope;
        private readonly Scope _globalScope;

        public SymbolTable()
        {
            _globalScope = new Scope();
            _currentScope = _globalScope;
        }

        public void EnterScope()
        {
            var newScope = new Scope { Parent = _currentScope };
            _currentScope = newScope;
        }

        public void ExitScope()
        {
            if (_currentScope.Parent != null)
            {
                _currentScope = _currentScope.Parent;
            }
        }

        // Variable management
        public bool DeclareVariable(Variable variable)
        {
            if (_currentScope.Variables.ContainsKey(variable.Name))
            {
                return false;
            }

            _currentScope.Variables[variable.Name] = variable;
            return true;
        }

        public Variable LookupVariable(string name)
        {
            var scope = _currentScope;
            while (scope != null)
            {
                if (scope.Variables.TryGetValue(name, out var variable))
                {
                    return variable;
                }
                scope = scope.Parent;
            }
            return null;
        }

        // Function management
        public bool DeclareFunction(Function function)
        {
            if (_currentScope.Functions.ContainsKey(function.Name))
            {
                return false;
            }

            _currentScope.Functions[function.Name] = function;
            return true;
        }

        public Function LookupFunction(string name)
        {
            var scope = _currentScope;
            while (scope != null)
            {
                if (scope.Functions.TryGetValue(name, out var function))
                {
                    return function;
                }
                scope = scope.Parent;
            }
            return null;
        }

        // Structure management
        public bool DeclareStructure(Structure structure)
        {
            if (_currentScope.Structures.ContainsKey(structure.Name))
            {
                return false;
            }

            _currentScope.Structures[structure.Name] = structure;
            return true;
        }

        public Structure LookupStructure(string name)
        {
            var scope = _currentScope;
            while (scope != null)
            {
                if (scope.Structures.TryGetValue(name, out var structure))
                {
                    return structure;
                }
                scope = scope.Parent;
            }
            return null;
        }

        // Scope information
        public bool IsGlobalScope()
        {
            return _currentScope == _globalScope;
        }

        public List<Variable> GetCurrentScopeVariables()
        {
            return _currentScope.Variables.Values.ToList();
        }

        public List<Function> GetCurrentScopeFunctions()
        {
            return _currentScope.Functions.Values.ToList();
        }

        public List<Structure> GetCurrentScopeStructures()
        {
            return _currentScope.Structures.Values.ToList();
        }

        // Symbol existence checks
        public bool VariableExistsInCurrentScope(string name)
        {
            return _currentScope.Variables.ContainsKey(name);
        }

        public bool FunctionExistsInCurrentScope(string name)
        {
            return _currentScope.Functions.ContainsKey(name);
        }

        public bool StructureExistsInCurrentScope(string name)
        {
            return _currentScope.Structures.ContainsKey(name);
        }

        // Global scope access
        public List<Variable> GetGlobalVariables()
        {
            return _globalScope.Variables.Values.ToList();
        }

        public List<Function> GetGlobalFunctions()
        {
            return _globalScope.Functions.Values.ToList();
        }

        public List<Structure> GetGlobalStructures()
        {
            return _globalScope.Structures.Values.ToList();
        }

        // Utility methods
        public void Clear()
        {
            _globalScope.Variables.Clear();
            _globalScope.Functions.Clear();
            _globalScope.Structures.Clear();
            _currentScope = _globalScope;
        }

        public bool IsValidType(string type)
        {
            // Check if it's a primitive type
            var primitiveTypes = new[] { "int", "float", "double", "string", "void" };
            if (primitiveTypes.Contains(type))
            {
                return true;
            }

            // Check if it's a defined structure type
            return LookupStructure(type) != null;
        }
    }
}