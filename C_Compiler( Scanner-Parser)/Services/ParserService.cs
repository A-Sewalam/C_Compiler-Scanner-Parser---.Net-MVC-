using C_Compiler__Scanner_Parser_.Models;
using System;
using System.Collections.Generic;

namespace C_Compiler__Scanner_Parser_.Services
{
    public class ParserService : IParserService
    {
        private List<Token> _tokens;
        private int _position;

        public ParserNode Parse(List<Token> tokens)
        {
            _tokens = tokens.Where(t => t.Type != TokenType.UNKNOWN).ToList(); // Filter bad tokens if any
            _position = 0;

            // Start Parsing from the top (Program)
            var programNode = ParseProgram();

            if (_position < _tokens.Count)
            {
                throw new Exception($"Parsing finished but extra tokens found starting at {_tokens[_position].Value}");
            }

            return programNode;
        }

        // --- HELPER METHODS ---

        private Token Current => _position < _tokens.Count ? _tokens[_position] : new Token(TokenType.UNKNOWN, "EOF");

        // Checks if current token matches expected type/value and consumes it
        private Token Match(TokenType type, string value = null)
        {
            if (Current.Type == type && (value == null || Current.Value == value))
            {
                var token = Current;
                _position++;
                return token;
            }
            return null; // Mismatch
        }

        private Token Consume(TokenType type, string value = null)
        {
            var token = Match(type, value);
            if (token == null)
            {
                throw new Exception($"Syntax Error: Expected '{value ?? type.ToString()}' but found '{Current.Value}' at token {_position}");
            }
            return token;
        }

        // --- GRAMMAR RULES ---

        // Program -> Function
        private ParserNode ParseProgram()
        {
            // We assume the program is just one main function for this snippet
            var func = ParseFunction();
            return new NonTerminalNode("Program", func);
        }

        // Function -> Type Identifier ( ) Block
        private ParserNode ParseFunction()
        {
            var typeToken = Match(TokenType.KEYWORD); // int, void, etc.
            if (typeToken == null) throw new Exception("Expected return type for function.");

            var idToken = Consume(TokenType.IDENTIFIER); // main
            var openParen = Consume(TokenType.SPECIAL_CHARACTER, "(");
            var closeParen = Consume(TokenType.SPECIAL_CHARACTER, ")");

            var block = ParseBlock();

            return new NonTerminalNode("Function",
                new TerminalNode(typeToken),
                new TerminalNode(idToken),
                new TerminalNode(openParen),
                new TerminalNode(closeParen),
                block
            );
        }

        // Block -> { StatementList }
        private ParserNode ParseBlock()
        {
            var openBrace = Consume(TokenType.SPECIAL_CHARACTER, "{");
            var statements = new List<ParserNode>();

            // Keep parsing statements until we hit '}'
            while (Current.Value != "}" && _position < _tokens.Count)
            {
                statements.Add(ParseStatement());
            }

            var closeBrace = Consume(TokenType.SPECIAL_CHARACTER, "}");

            // Create a specific node for the statement list
            var stmtListNode = new NonTerminalNode("StatementList", statements.ToArray());

            return new NonTerminalNode("Block",
                new TerminalNode(openBrace),
                stmtListNode,
                new TerminalNode(closeBrace)
            );
        }

        // Statement -> Declaration | Assignment | If | Return
        private ParserNode ParseStatement()
        {
            // 1. IF Statement
            if (Current.Type == TokenType.KEYWORD && Current.Value == "if")
            {
                return ParseIfStatement();
            }

            // 2. RETURN Statement
            if (Current.Type == TokenType.KEYWORD && Current.Value == "return")
            {
                var retKw = Consume(TokenType.KEYWORD, "return");
                var expr = ParseExpression();
                var semi = Consume(TokenType.SPECIAL_CHARACTER, ";");
                return new NonTerminalNode("Statement", new TerminalNode(retKw), expr, new TerminalNode(semi));
            }

            // 3. DECLARATION: Starts with a Type (int, float, etc.)
            if (Current.Type == TokenType.KEYWORD && IsTypeKeyword(Current.Value))
            {
                return ParseDeclaration();
            }

            // 4. BLOCK (Nested block)
            if (Current.Value == "{")
            {
                return ParseBlock();
            }

            // 5. ASSIGNMENT: Identifier = ...
            // We assume if it starts with Identifier, it's an assignment for now
            if (Current.Type == TokenType.IDENTIFIER)
            {
                return ParseAssignment();
            }

            throw new Exception($"Unexpected token in statement: {Current.Value}");
        }

        private bool IsTypeKeyword(string k) => k == "int" || k == "float" || k == "double" || k == "char" || k == "void";

        // IfStatement -> if ( Expr ) Statement [else Statement]
        private ParserNode ParseIfStatement()
        {
            var ifKw = Consume(TokenType.KEYWORD, "if");
            var open = Consume(TokenType.SPECIAL_CHARACTER, "(");
            var expr = ParseExpression();
            var close = Consume(TokenType.SPECIAL_CHARACTER, ")");
            var thenStmt = ParseStatement(); // Often a Block

            if (Current.Type == TokenType.KEYWORD && Current.Value == "else")
            {
                var elseKw = Consume(TokenType.KEYWORD, "else");
                var elseStmt = ParseStatement();
                return new NonTerminalNode("IfStatement",
                    new TerminalNode(ifKw), new TerminalNode(open), expr, new TerminalNode(close), thenStmt,
                    new TerminalNode(elseKw), elseStmt);
            }

            return new NonTerminalNode("IfStatement",
                new TerminalNode(ifKw), new TerminalNode(open), expr, new TerminalNode(close), thenStmt);
        }

        // Declaration -> Type IdList ;
        private ParserNode ParseDeclaration()
        {
            var typeToken = Consume(TokenType.KEYWORD);
            var idList = ParseIdList();
            var semi = Consume(TokenType.SPECIAL_CHARACTER, ";");

            return new NonTerminalNode("Declaration", new TerminalNode(typeToken), idList, new TerminalNode(semi));
        }

        // IdList -> ID [, ID]*
        private ParserNode ParseIdList()
        {
            var nodes = new List<ParserNode>();
            nodes.Add(new TerminalNode(Consume(TokenType.IDENTIFIER)));

            while (Current.Value == ",")
            {
                nodes.Add(new TerminalNode(Consume(TokenType.SPECIAL_CHARACTER, ",")));
                nodes.Add(new TerminalNode(Consume(TokenType.IDENTIFIER)));
            }
            return new NonTerminalNode("IdList", nodes.ToArray());
        }

        // Assignment -> ID = Expr ;
        private ParserNode ParseAssignment()
        {
            var id = Consume(TokenType.IDENTIFIER);
            var eq = Consume(TokenType.OPERATOR, "=");
            var expr = ParseExpression();
            var semi = Consume(TokenType.SPECIAL_CHARACTER, ";");

            return new NonTerminalNode("Assignment", new TerminalNode(id), new TerminalNode(eq), expr, new TerminalNode(semi));
        }

        // Expression -> SimpleTerm [OP SimpleTerm]*
        // Handles x, x-3, x==42
        private ParserNode ParseExpression()
        {
            var left = ParseSimpleTerm();

            // While we have an operator (+, -, ==, etc.)
            while (Current.Type == TokenType.OPERATOR || Current.Value == "==" || Current.Value == "<" || Current.Value == ">")
            {
                var op = Consume(Current.Type); // Consume the operator
                var right = ParseSimpleTerm();
                left = new NonTerminalNode("BinaryExpr", left, new TerminalNode(op), right);
            }

            return left;
        }

        // SimpleTerm -> ID | Number | ( Expr )
        private ParserNode ParseSimpleTerm()
        {
            if (Current.Type == TokenType.IDENTIFIER)
                return new TerminalNode(Consume(TokenType.IDENTIFIER));

            if (Current.Type == TokenType.INTEGER_LITERAL)
                return new TerminalNode(Consume(TokenType.INTEGER_LITERAL));

            if (Current.Type == TokenType.FLOAT_LITERAL)
                return new TerminalNode(Consume(TokenType.FLOAT_LITERAL));

            if (Current.Value == "(")
            {
                Consume(TokenType.SPECIAL_CHARACTER, "(");
                var expr = ParseExpression();
                Consume(TokenType.SPECIAL_CHARACTER, ")");
                return new NonTerminalNode("ParenExpr", expr);
            }

            throw new Exception($"Unexpected token in expression: {Current.Value}");
        }
    }
}