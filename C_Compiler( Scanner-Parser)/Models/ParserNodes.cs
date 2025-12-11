using System.Collections.Generic;
using System.Text;

namespace C_Compiler__Scanner_Parser_.Models
{
    // Base class for all nodes in the Parse Tree
    public abstract class ParserNode
    {
        public string Name { get; set; } // e.g., "Program", "Statement", "Expression"

        public abstract string Print(int level = 0);

        protected string Indent(int level)
        {
            return new string(' ', level * 2);
        }
    }

    // Represents a leaf node (a Token from the Lexer)
    public class TerminalNode : ParserNode
    {
        public Token Token { get; set; }

        public TerminalNode(Token token)
        {
            Name = token.Type.ToString();
            Token = token;
        }

        public override string Print(int level = 0)
        {
            return $"{Indent(level)}{Name}: {Token.Value}";
        }
    }

    // Represents a non-terminal node (e.g., Block, IfStatement) containing child nodes
    public class NonTerminalNode : ParserNode
    {
        public List<ParserNode> Children { get; set; } = new List<ParserNode>();

        public NonTerminalNode(string name, params ParserNode[] children)
        {
            Name = name;
            Children.AddRange(children);
        }

        public override string Print(int level = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{Indent(level)}{Name}");
            foreach (var child in Children)
            {
                sb.AppendLine(child.Print(level + 1));
            }
            return sb.ToString().TrimEnd();
        }
    }
}