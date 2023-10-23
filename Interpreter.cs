namespace cslox {
    using static TokenType;
    class Interpreter : Expr.Visitor<Object> {

        public void Interpret(Expr expr) {
            try {
                Object value = Evaluate(expr);
                System.Console.WriteLine(Stringify(value));
            } catch(RuntimeError error) {
                CSLox.RuntimeError(error);
            }
        }

        private string Stringify(Object obj) {
            if(obj == null) return "nil";

            // Hack. Work around Java adding ".0" to integer-valued doubles.
            if(obj is double) {
                string text = obj.ToString();
                if(text.EndsWith(".0")) {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return obj.ToString();
        }
        public Object VisitLiteralExpr(Expr.Literal expr) {
            return expr.value;
        }

        public Object VisitGroupingExpr(Expr.Grouping expr) {
            return Evaluate(expr.expression);
        }

        public Object VisitUnaryExpr(Expr.Unary expr) {
            Object right = Evaluate(expr.right);

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

        public Object VisitBinaryExpr(Expr.Binary expr) {
            Object left = Evaluate(expr.left);
            Object right = Evaluate(expr.right);

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

        private Object Evaluate(Expr expr) {
            return expr.Accept<Object>(this);
        }

        private bool IsTruthy(Object obj) {
            if(obj == null) return false;
            if(obj is bool) return (bool)obj;
            return true;
        }

        private bool IsEqual(Object a, Object b) {
            // nil is only equal to nil.
            if(a == null && b == null) return true;
            if(a == null) return false;

            return a.Equals(b);
        }

        private void CheckNumberOperand(Token op, Object operand) {
            if(operand is double) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token op, Object left, Object right) {
            if(left is double && right is double) return;
            throw new RuntimeError(op, "Operands must be numbers.");
        }
    }
}