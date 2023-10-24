namespace cslox {
    enum FunctionType {
        NONE,
        FUNCTION
    }
    public class Resolver : Expr.Visitor<object>, Stmt.Visitor<object> {
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes = new();
        private FunctionType currentFunction = FunctionType.NONE;

        public Resolver(Interpreter interpreter) {
            this.interpreter = interpreter;
        }

        public object VisitBlockStmt(Stmt.Block stmt) {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt) {
            Declare(stmt.name);
            if(stmt.initializer != null) {
                Resolve(stmt.initializer);
            }
            Define(stmt.name);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr) {
            if(scopes.Count != 0 && scopes.Peek()[expr.name.lexeme] == false) {
                CSLox.Error(expr.name, "Cannot read local variable in its own initializer.");
            }
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr) {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt) {
            Declare(stmt.name);
            Define(stmt.name);
            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt) {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt) {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if(stmt.elseBranch != null) {
                Resolve(stmt.elseBranch);
            }
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt) {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt) {
            if(currentFunction == FunctionType.NONE) {
                CSLox.Error(stmt.keyword, "Cannot return from top-level code.");
            }
            if(stmt.value != null) {
                Resolve(stmt.value);
            }
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt) {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }

        public object VisitBinaryExpr(Expr.Binary expr) {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitCallExpr(Expr.Call expr) {
            Resolve(expr.callee);
            foreach(Expr argument in expr.arguments) {
                Resolve(argument);
            }
            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr) {
            Resolve(expr.expression);
            return null;
        }

        public object VisitLiteralExpr(Expr.Literal expr) {
            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr) {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitUnaryExpr(Expr.Unary expr) {
            Resolve(expr.right);
            return null;
        }

        void ResolveFunction(Stmt.Function function, FunctionType type) {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;
            BeginScope();
            foreach(Token parameter in function.parameters) {
                Declare(parameter);
                Define(parameter);
            }
            Resolve(function.body);
            EndScope();
            currentFunction = enclosingFunction;
        }

        void ResolveLocal(Expr expr, Token name) {
            for(int i = scopes.Count - 1; i >= 0; i--) {
                if(scopes.ElementAt(i).ContainsKey(name.lexeme)) {
                    interpreter.Resolve(expr, scopes.Count - 1 - i);
                    return;
                }
            }
        }

        void Declare(Token name) {
            if(scopes.Count == 0) return;
            Dictionary<string, bool> scope = scopes.Peek();
            if(scope.ContainsKey(name.lexeme)) {
                CSLox.Error(name, "Variable with this name already declared in this scope.");
            }
            scope[name.lexeme] = false;
        }

        void Define(Token name) {
            if(scopes.Count == 0) return;
            scopes.Peek()[name.lexeme] = true;
        }

        void BeginScope() {
            scopes.Push(new Dictionary<string, bool>());
        }

        void EndScope() {
            scopes.Pop();
        }

        public void Resolve(List<Stmt> stmts) {
            foreach(Stmt stmt in stmts) {
                Resolve(stmt);
            }
        }

        void Resolve(Stmt stmt) {
            stmt.Accept(this);
        }

        void Resolve(Expr expr) {
            expr.Accept(this);
        }
    }
}