using System;
using System.IO;
using System.Security.Principal;

namespace cslox
{
    internal class CSLox
    {
        private static readonly Interpreter interpreter = new();
        static bool hadError = false;
        static bool hadRuntimeError = false;
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
            if(hadRuntimeError) System.Environment.Exit(70);
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
            List<Stmt> statements = parser.Parse();


            if(hadError) return;

            interpreter.Interpret(statements);
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

        public static void RuntimeError(RuntimeError error) {
            Console.Error.WriteLine(error.Message + "\n[line " + error.token.line + "]");
            hadRuntimeError = true;
        }

        private static void Report(int line, String where, String message) {
            Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }
    }
}