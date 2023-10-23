#r "System.IO"
#r "System.Collections"

    void main() {
        Console.WriteLine("Generating AST...");
        if(Args.Count != 1) {
            Console.Error.WriteLine("Usage: generate_ast <output directory>");
            System.Environment.Exit(64);
        }
        string outputDir = Args[0];
        DefineAST(outputDir, "Expr", new List<string> {
            "Assign   : Token name, Expr value",
            "Binary   : Expr left, Token op, Expr right",
            "Grouping : Expr expression",
            "Literal  : Object value",
            "Logical  : Expr left, Token op, Expr right",
            "Unary    : Token op, Expr right",
            "Variable : Token name"
        });

        DefineAST(outputDir, "Stmt", new List<string> {
            "Block      : List<Stmt> statements",
            "Expression : Expr expression",
            "If         : Expr condition, Stmt thenBranch, Stmt elseBranch",
            "Print      : Expr expression",
            "Var        : Token name, Expr initializer",
            "While      : Expr condition, Stmt body",
        });
    }

    void DefineAST(string outputDir, string baseName, List<string> types) {
        string path = outputDir + '/' + baseName + ".cs";
        StreamWriter writer = new StreamWriter(path);
        writer.WriteLine("namespace cslox {");
        writer.WriteLine("public abstract class " + baseName + " {");
        DefineVisitor(writer, baseName, types);
        foreach(string type in types) {
            string className = type.Split(':')[0].Trim();
            string fields = type.Split(':')[1].Trim();
            DefineType(writer, baseName, className, fields);
        }
        writer.WriteLine();
        writer.WriteLine(" public abstract R Accept<R>(Visitor<R> visitor);");
        writer.WriteLine("} \n }");
        writer.Flush();
        writer.Close();
    }

    void DefineType(StreamWriter writer, string baseName, string className, string fieldList) {
        writer.WriteLine(" public class " + className + " : " + baseName + " {");
        writer.WriteLine(" public " + className + "(" + fieldList + ") {");

        string[] fields = fieldList.Split(", ");
        foreach(string field in fields) {
            string name = field.Split(" ")[1];
            writer.WriteLine("   this." + name + " = " + name + ";");
        }

        writer.WriteLine("  }");
        writer.WriteLine();
        writer.WriteLine("  public override R Accept<R>(Visitor<R> visitor) {");
        writer.WriteLine("   return visitor.Visit" + className + baseName + "(this);");
        writer.WriteLine("  }");

        foreach(string field in fields) {
            writer.WriteLine("  public readonly " + field + ";");
        }

        writer.WriteLine(" }");
    }

    void DefineVisitor(StreamWriter writer, string baseName, List<string> types) {
        writer.WriteLine(" public interface Visitor<R> {");
        foreach(string type in types) {
            string typeName = type.Split(':')[0].Trim();
            writer.WriteLine("  public R Visit" + typeName + baseName + "(" + typeName + " " + baseName.ToLower() + ");");
        }
        writer.WriteLine(" }");
    }

main();