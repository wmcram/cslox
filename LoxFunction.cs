namespace cslox {
    public class LoxFunction : LoxCallable {
        private readonly Stmt.Function declaration;
        public LoxFunction(Stmt.Function declaration) {
            this.declaration = declaration;
        }

        public int Arity() => declaration.parameters.Count;

        public override string ToString() => $"<fn {declaration.name.lexeme}>";

        public object Call(Interpreter interpreter, List<object> arguments) {
            Environment environment = new(interpreter.globals);
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