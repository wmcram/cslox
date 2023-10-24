namespace cslox {
public abstract class Stmt {
 public interface Visitor<R> {
  public R VisitBlockStmt(Block stmt);
  public R VisitExpressionStmt(Expression stmt);
  public R VisitFunctionStmt(Function stmt);
  public R VisitIfStmt(If stmt);
  public R VisitPrintStmt(Print stmt);
  public R VisitReturnStmt(Return stmt);
  public R VisitVarStmt(Var stmt);
  public R VisitWhileStmt(While stmt);
 }
 public class Block : Stmt {
 public Block(List<Stmt> statements) {
   this.statements = statements;
  }

  public override R Accept<R>(Visitor<R> visitor) {
   return visitor.VisitBlockStmt(this);
  }
  public readonly List<Stmt> statements;
 }
 public class Expression : Stmt {
 public Expression(Expr expression) {
   this.expression = expression;
  }

  public override R Accept<R>(Visitor<R> visitor) {
   return visitor.VisitExpressionStmt(this);
  }
  public readonly Expr expression;
 }
 public class Function : Stmt {
 public Function(Token name, List<Token> parameters, List<Stmt> body) {
   this.name = name;
   this.parameters = parameters;
   this.body = body;
  }

  public override R Accept<R>(Visitor<R> visitor) {
   return visitor.VisitFunctionStmt(this);
  }
  public readonly Token name;
  public readonly List<Token> parameters;
  public readonly List<Stmt> body;
 }
 public class If : Stmt {
 public If(Expr condition, Stmt thenBranch, Stmt elseBranch) {
   this.condition = condition;
   this.thenBranch = thenBranch;
   this.elseBranch = elseBranch;
  }

  public override R Accept<R>(Visitor<R> visitor) {
   return visitor.VisitIfStmt(this);
  }
  public readonly Expr condition;
  public readonly Stmt thenBranch;
  public readonly Stmt elseBranch;
 }
 public class Print : Stmt {
 public Print(Expr expression) {
   this.expression = expression;
  }

  public override R Accept<R>(Visitor<R> visitor) {
   return visitor.VisitPrintStmt(this);
  }
  public readonly Expr expression;
 }
 public class Return : Stmt {
 public Return(Token keyword, Expr value) {
   this.keyword = keyword;
   this.value = value;
  }

  public override R Accept<R>(Visitor<R> visitor) {
   return visitor.VisitReturnStmt(this);
  }
  public readonly Token keyword;
  public readonly Expr value;
 }
 public class Var : Stmt {
 public Var(Token name, Expr initializer) {
   this.name = name;
   this.initializer = initializer;
  }

  public override R Accept<R>(Visitor<R> visitor) {
   return visitor.VisitVarStmt(this);
  }
  public readonly Token name;
  public readonly Expr initializer;
 }
 public class While : Stmt {
 public While(Expr condition, Stmt body) {
   this.condition = condition;
   this.body = body;
  }

  public override R Accept<R>(Visitor<R> visitor) {
   return visitor.VisitWhileStmt(this);
  }
  public readonly Expr condition;
  public readonly Stmt body;
 }

 public abstract R Accept<R>(Visitor<R> visitor);
} 
 }
