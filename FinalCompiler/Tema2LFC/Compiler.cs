//using Antlr4.Runtime;
//using Antlr4.Runtime.Misc;
//using Antlr4.Runtime.Tree;
//using Tema2LFC.Grammar;

//namespace Tema2LFC
//{
//    public class Program
//    {
//        static void Main(string[] args)
//        {
//            string input = File.ReadAllText("input.txt");
//            // Use AntlrInputStream instead of CharStreams
//            ICharStream stream = new AntlrInputStream(input);
//            ITokenSource lexer = new MiniLangLexer(stream);
//            ITokenStream tokens = new CommonTokenStream(lexer);
//            MiniLangParser parser = new MiniLangParser(tokens);
//            var tree = parser.program();
//            var visitor = new MiniLangVisitor();
//            visitor.Visit(tree);
//        }
//    }
//}