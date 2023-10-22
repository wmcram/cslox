namespace cslox {
abstract class Expr {
 class Binary : Expr {
  Binary(Expr left, Token op, Expr right) {
   this.left = left;
   this.op = op;
   this.right = right;
  }

  readonly Expr left;
  readonly Token op;
  readonly Expr right;
 }
 class Grouping : Expr {
  Grouping(Expr expression) {
   this.expression = expression;
  }

  readonly Expr expression;
 }
 class Literal : Expr {
  Literal(Object value) {
   this.value = value;
  }

  readonly Object value;
 }
 class Unary : Expr {
  Unary(Token op, Expr right) {
   this.op = op;
   this.right = right;
  }

  readonly Token op;
  readonly Expr right;
 }
} 
 }
