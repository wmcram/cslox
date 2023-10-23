namespace cslox {
    using System.Net.Mail;
    using static TokenType;
    class Interpreter : Expr.Visitor<object>, Stmt.Visitor<Object> {

        private Environment environment = new();

        public void Interpret(List<Stmt> stmts) {
            try {
                foreach(Stmt stmt in stmts) {
                    Execute(stmt);
                }
            }
            catch(RuntimeError error) {
                CSLox.RuntimeError(error);
            }
        }

        private string Stringify(object obj) {
            if(obj == null) return "nil";
            if(obj is double) {
                string text = obj.ToString();
                if(text.EndsWith(".0")) {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }
            return obj.ToString();
        }
        public object VisitLiteralExpr(Expr.Literal expr) {
            return expr.value;
        }

        public object VisitGroupingExpr(Expr.Grouping expr) {
            return Evaluate(expr.expression);
        }

        public object VisitUnaryExpr(Expr.Unary expr) {
            object right = Evaluate(expr.right);

            switch(expr.op.type) {
                case BANG:
                    return !IsTruthy(right);
                case MINUS:
                    CheckNumberOperand(expr.op, right);
                    return -(double)right;
            }

            // Unreachable.
            return null;
        }

        public object VisitBinaryExpr(Expr.Binary expr) {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch(expr.op.type) {
                case GREATER:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left > (double)right;
                case GREATER_EQUAL:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left >= (double)right;
                case LESS:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left < (double)right;
                case LESS_EQUAL:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left <= (double)right;
                case BANG_EQUAL:
                    return !IsEqual(left, right);
                case EQUAL_EQUAL:
                    return IsEqual(left, right);
                case MINUS:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left - (double)right;
                case PLUS:
                    if(left is double && right is double) {
                        return (double)left + (double)right;
                    }
                    if(left is string && right is string) {
                        return (string)left + (string)right;
                    }
                    throw new RuntimeError(expr.op, "Operands must be two numbers or two strings.");
                case SLASH:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left / (double)right;
                case STAR:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left * (double)right;
            }
            // Unreachable.
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr) {
            return environment.Get(expr.name);
        }

        public object VisitAssignExpr(Expr.Assign expr) {
            object value = Evaluate(expr.value);
            environment.Assign(expr.name, value);
            return value;
        }

        public object VisitLogicalExpr(Expr.Logical expr) {
            object left = Evaluate(expr.left);
            if(expr.op.type == OR) {
                if(IsTruthy(left)) return left;
            }
            else {
                if(!IsTruthy(left)) return left;
            }
            return Evaluate(expr.right);
        }

        public object VisitExpressionStmt(Stmt.Expression stmt) {
            Evaluate(stmt.expression);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt) {
            while(IsTruthy(stmt.condition)) {
                Execute(stmt.body);
            }
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt) {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt) {
            Object value = null;
            if(stmt.initializer != null) {
                value = Evaluate(stmt.initializer);
            }
            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt) {
            if(IsTruthy(Evaluate(stmt.condition))) {
                Execute(stmt.thenBranch);
            }
            else if(stmt.elseBranch != null) {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public object VisitBlockStmt(Stmt.Block stmt) {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        private object Evaluate(Expr expr) {
            return expr.Accept<object>(this);
        }

        private void Execute(Stmt stmt) {
            stmt.Accept<object>(this);
        }

        private void ExecuteBlock(List<Stmt> statements, Environment environment) {
            Environment previous = this.environment;
            try {
                this.environment = environment;
                foreach(Stmt stmt in statements) {
                    Execute(stmt);
                }
            }
            finally {
                this.environment = previous;
            }
        }

        private bool IsTruthy(object obj) {
            if(obj == null) return false;
            if(obj is bool) return (bool)obj;
            return true;
        }

        private bool IsEqual(object a, object b) {
            // nil is only equal to nil.
            if(a == null && b == null) return true;
            if(a == null) return false;

            return a.Equals(b);
        }

        private void CheckNumberOperand(Token op, object operand) {
            if(operand is double) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token op, object left, object right) {
            if(left is double && right is double) return;
            throw new RuntimeError(op, "Operands must be numbers.");
        }
    }
}