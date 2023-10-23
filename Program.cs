using System;
using System.IO;
using System.Security.Principal;

namespace cslox
{
    internal class CSLox
    {
        static bool hadError = false;
        static void Main(string[] args)  {
            if(args.Length > 1) {
                Console.WriteLine("Usage: cslox [script]");
                System.Environment.Exit(64);
            }
            else if(args.Length == 1) {
                RunFile(args[0]);
            }
            else {
                RunPrompt();
            }
        }

        static void RunFile(String path) {
            byte[] bytes = File.ReadAllBytes(path);
            Run(System.Text.Encoding.Default.GetString(bytes));

            if(hadError) System.Environment.Exit(65);
        }

        static void RunPrompt() {
            TextReader input = Console.In;

            while(true) {
                Console.Write("> ");
                String? line = input.ReadLine();
                if(line == null) break;
                Run(line);
                hadError = false;
            }
        }

        static void Run(String source) {
            Scanner scanner = new(source);
            List<Token> tokens = scanner.ScanTokens();
            Parser parser = new(tokens);
            Expr expression = parser.Parse();

            if(hadError) return;

            Console.WriteLine(new AstPrinter().Print(expression));
        }

        public static void Error(int line, String message) {
            Report(line, "", message);
        }

        public static void Error(Token token, String message) {
            if(token.type == TokenType.EOF) {
                Report(token.line, " at end", message);
            }
            else {
                Report(token.line, " at '" + token.lexeme + "'", message);
            }
        }

        private static void Report(int line, String where, String message) {
            Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }
    }
}