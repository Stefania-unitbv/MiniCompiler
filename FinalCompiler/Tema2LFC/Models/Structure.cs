using System.Collections.Generic;
using System.Linq;

namespace MiniLangCompiler.Models
{
    public class Structure
    {
        public string Name { get; set; }
        public List<Variable> Fields { get; set; } = new List<Variable>();
        public List<Function> Methods { get; set; } = new List<Function>();
        public int DeclarationLine { get; set; }

        public bool HasConstructor => Methods.Any(m => m.IsConstructor);
        public bool HasDestructor => Methods.Any(m => m.IsDestructor);

        public override string ToString()
        {
            return $"struct {Name}";
        }

        public Function GetConstructor()
        {
            return Methods.FirstOrDefault(m => m.IsConstructor);
        }

        public Function GetDestructor()
        {
            return Methods.FirstOrDefault(m => m.IsDestructor);
        }

        public List<Function> GetRegularMethods()
        {
            return Methods.Where(m => !m.IsConstructor && !m.IsDestructor).ToList();
        }
    }
}