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
            "Binary   : Expr left, Token op, Expr right",
            "Grouping : Expr expression",
            "Literal  : Object value",
            "Unary    : Token op, Expr right"
        });
    }

    void DefineAST(string outputDir, string baseName, List<string> types) {
        string path = outputDir + '/' + baseName + ".cs";
        StreamWriter writer = new StreamWriter(path);
        writer.WriteLine("namespace cslox {");
        writer.WriteLine("abstract class " + baseName + " {");
        foreach(string type in types) {
            string className = type.Split(':')[0].Trim();
            string fields = type.Split(':')[1].Trim();
            DefineType(writer, baseName, className, fields);
        }
        writer.WriteLine("} \n }");
        writer.Flush();
        writer.Close();
    }

    void DefineType(StreamWriter writer, string baseName, string className, string fieldList) {
        writer.WriteLine(" class " + className + " : " + baseName + " {");
        writer.WriteLine("  " + className + "(" + fieldList + ") {");

        string[] fields = fieldList.Split(", ");
        foreach(string field in fields) {
            string name = field.Split(" ")[1];
            writer.WriteLine("   this." + name + " = " + name + ";");
        }

        writer.WriteLine("  }");
        writer.WriteLine();

        foreach(string field in fields) {
            writer.WriteLine("  readonly " + field + ";");
        }

        writer.WriteLine(" }");
    }

main();