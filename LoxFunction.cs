namespace cslox {
    public class LoxFunction : LoxCallable {
        private readonly Stmt.Function declaration;
        private readonly Environment closure;
        public LoxFunction(Stmt.Function declaration, Environment closure) {
            this.closure = closure;
            this.declaration = declaration;
        }

        public int Arity() => declaration.parameters.Count;

        public override string ToString() => $"<fn {declaration.name.lexeme}>";

        public object Call(Interpreter interpreter, List<object> arguments) {
            Environment environment = new(closure);
            for(int i = 0; i < declaration.parameters.Count; i++) {
                environment.Define(declaration.parameters[i].lexeme, arguments[i]);
            }
            try {
                interpreter.ExecuteBlock(declaration.body, environment);
            } catch(Return returnValue) {
                return returnValue.value;
            }
            return null;
        }
    }
}