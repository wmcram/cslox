namespace cslox {
    using System.Collections;

    interface LoxCallable {
        public object Call(Interpreter interpreter, List<object> arguments);
        public int Arity();
    }
}