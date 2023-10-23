namespace cslox {
    using System.Collections;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using static TokenType;

    public class Parser {
        private class ParseError : System.Exception {}
        private readonly List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens) {
            this.tokens = tokens;
        }

        public List<Stmt> Parse() {
            List<Stmt> stmts = new List<Stmt>();
            while(!IsAtEnd()) {
                stmts.Add(Declaration());
            }
            return stmts;
        }

        private Stmt Declaration() {
            try {
                if(Match(VAR)) return VarDeclaration();
                return Statement();
            }
            catch(ParseError error) {
                Synchronize();
                return null;
            }
        }

        private Stmt VarDeclaration() {
            Token name = Consume(IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if(Match(EQUAL)) {
                initializer = Expression();
            }

            Consume(SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        private Stmt Statement() {
            if(Match(FOR)) return ForStatement();
            if(Match(WHILE)) return WhileStatement();
            if(Match(IF)) return IfStatement();
            if(Match(PRINT)) return PrintStatement();
            if(Match(LEFT_BRACE)) return new Stmt.Block(Block());
            return ExpressionStatement();
        }

        private Stmt ForStatement() {
            Consume(LEFT_PAREN, "Expect '(' after 'for'.");
            Stmt initializer;
            if(Match(SEMICOLON)) {
                initializer = null;
            }
            else if(Match(VAR)) {
                initializer = Declaration();
            }
            else {
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if(!Check(SEMICOLON)) {
                condition = Expression();
            }
            Consume(SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if(!Check(RIGHT_PAREN)) {
                increment = Expression();
            }
            Consume(RIGHT_PAREN, "Expect ')' after for clauses.");
            Stmt body = Statement();
            if(increment != null) {
                body = new Stmt.Block(new List<Stmt> {
                    body,
                    new Stmt.Expression(increment)
                });
            }
            if(condition == null) condition = new Expr.Literal(true);
            body = new Stmt.While(condition, body);
            if(initializer != null) {
                body = new Stmt.Block(new List<Stmt> {
                    initializer, 
                    body
                });
            }
            return body;
        }

        private Stmt WhileStatement() {
            Consume(LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after condition.");
            Stmt body = Statement();
            return new Stmt.While(condition, body);
        }

        private Stmt IfStatement() {
            Consume(LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();
            Stmt elseBranch = null;
            if(Match(ELSE)) {
                elseBranch = Statement();
            }
            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private Stmt PrintStatement() {
            Expr value = Expression();
            Consume(SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt ExpressionStatement() {
            Expr expr = Expression();
            Consume(SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private List<Stmt> Block() {
            List<Stmt> statements = new();
            while(!Check(RIGHT_BRACE) && !IsAtEnd()) {
                statements.Add(Declaration());
            }
            Consume(RIGHT_BRACE, "Expected closing '}' after block");
            return statements;
        }

        private Expr Assignment() {
            Expr expr = Or();
            if(Match(EQUAL)) {
                Token equals = Previous();
                Expr value = Assignment();

                if(expr is Expr.Variable) {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.Assign(name, value);
                }

                Error(equals, "Invalid Assignment Target.");
            }

            return expr;
        }

        private Expr Or() {
            Expr expr = And();
            while(Match(OR)) {
                Token op = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, op, right);
            }
            return expr;
        }

        private Expr And() {
            Expr expr = Equality();
            while(Match(AND)) {
                Token op = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, op, right);
            }
            return expr;
        }

        private Expr Expression() {
            return Assignment();
        }

        private Expr Equality() {
            Expr expr = Comparison();

            while(Match(BANG_EQUAL, EQUAL_EQUAL)) {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Comparison() {
            Expr expr = Term();

            while(Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL)) {
                Token op = Previous();
                Expr right = Term();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Term() {
            Expr expr = Factor();

            while(Match(MINUS, PLUS)) {
                Token op = Previous();
                Expr right = Factor();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Factor() {
            Expr expr = Unary();

            while(Match(SLASH, STAR)) {
                Token op = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Unary() {
            if(Match(BANG, MINUS)) {
                Token op = Previous();
                Expr right = Unary();
                return new Expr.Unary(op, right);
            }

            return Primary();
        }

        private Expr Primary() {
            if(Match(FALSE)) return new Expr.Literal(false);
            if(Match(TRUE)) return new Expr.Literal(true);
            if(Match(NIL)) return new Expr.Literal(null);
            if(Match(NUMBER, STRING)) {
                return new Expr.Literal(Previous().literal);
            }
            if(Match(IDENTIFIER)) {
                return new Expr.Variable(Previous());
            }
            if(Match(LEFT_PAREN)) {
                Expr expr = Expression();
                Consume(RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }
            throw Error(Peek(), "Expect expression.");
        }


        private bool Match(params TokenType[] types) {
            foreach(TokenType type in types) {
                if(Check(type)) {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type) {
            if(IsAtEnd()) {
                return false;
            }

            return Peek().type == type;
        }

        private Token Advance() {
            if(!IsAtEnd()) {
                current++;
            }

            return Previous();
        }

        private bool IsAtEnd() {
            return Peek().type == EOF;
        }

        private Token Peek() {
            return tokens[current];
        }

        private Token Previous() {
            return tokens[current - 1];
        }

        private Token Consume(TokenType type, string message) {
            if(Check(type)) {
                return Advance();
            }

            throw Error(Peek(), message);
        }

        private ParseError Error(Token token, string message) {
            CSLox.Error(token, message);
            return new ParseError();
        }

        private void Synchronize() {
            Advance();

            while(!IsAtEnd()) {
                if(Previous().type == SEMICOLON) return;

                switch(Peek().type) {
                    case CLASS:
                    case FUN:
                    case VAR:
                    case FOR:
                    case IF:
                    case WHILE:
                    case PRINT:
                    case RETURN:
                        return;
                }

                Advance();
            }
        }
    }
}