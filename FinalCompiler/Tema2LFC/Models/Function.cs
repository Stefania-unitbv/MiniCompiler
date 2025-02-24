using System.Collections.Generic;

namespace MiniLangCompiler.Models
{
    public class Function
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<Variable> Parameters { get; set; } = new List<Variable>();
        public List<Variable> LocalVariables { get; set; } = new List<Variable>();
        public List<string> ControlStructures { get; set; } = new List<string>();
        public bool IsMain { get; set; }
        public bool IsRecursive { get; set; }
        public bool IsMethod { get; set; }
        public bool IsConstructor { get; set; }
        public bool IsDestructor { get; set; }

        public override string ToString()
        {
            return $"{ReturnType} {Name}";
        }
    }
}