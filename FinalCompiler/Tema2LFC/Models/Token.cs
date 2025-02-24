namespace MiniLangCompiler.Models
{
    public class Token
    {
        public string Type { get; set; }
        public string Lexeme { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public Token(string type, string lexeme, int line, int column)
        {
            Type = type;
            Lexeme = lexeme;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"<{Type}, {Lexeme}, {Line}>";
        }
    }
}