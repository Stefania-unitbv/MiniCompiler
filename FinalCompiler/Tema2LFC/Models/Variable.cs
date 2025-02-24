namespace MiniLangCompiler.Models
{
    public class Variable
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string InitialValue { get; set; }
        public bool IsParameter { get; set; }
        public bool IsGlobal { get; set; }
        public int DeclarationLine { get; set; }

        public override string ToString()
        {
            if (InitialValue != null)
            {
                return $"{Type} {Name} = {InitialValue}";
            }
            return $"{Type} {Name}";
        }
    }
}