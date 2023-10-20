using System.Collections.Generic;
using System.Data;
using static cslox.TokenType;

namespace cslox {
    class Scanner {
        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();
        private int start = 0;
        private int current = 0;
        private int line = 1;
        private static readonly Dictionary<String, TokenType> keywords;

        public Scanner(String source) {
            this.source = source;
        }

        static Scanner() {
            keywords = new Dictionary<String, TokenType>
            {
                { "and", AND },
                { "class", CLASS },
                { "else", ELSE },
                { "false", FALSE },
                { "for", FOR },
                { "fun", FUN },
                { "if", IF },
                { "nil", NIL },
                { "or", OR },
                { "print", PRINT },
                { "return", RETURN },
                { "super", SUPER },
                { "this", THIS },
                { "true", TRUE },
                { "var", VAR },
                { "while", WHILE }
            };
        }

        private Char Advance() {
            current++;
            return source[current - 1];
        }

        private void AddToken(TokenType type) {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, Object literal) {
            String text = source[start..current];
            tokens.Add(new Token(type, text, literal, line));
        }

        public List<Token> ScanTokens() {
            while(!IsAtEnd()) {
                start = current;
                ScanToken();
            }
            
            tokens.Add(new Token(EOF, "", null, line));
            return tokens;
        }

        private void ScanToken() {
            Char c = Advance();
            switch(c) {
                case '(': AddToken(LEFT_PAREN); break;
                case ')': AddToken(RIGHT_PAREN); break;
                case '{': AddToken(LEFT_BRACE); break;
                case '}': AddToken(RIGHT_BRACE); break;
                case ',': AddToken(COMMA); break;
                case '.': AddToken(DOT); break;
                case '-': AddToken(MINUS); break;
                case '+': AddToken(PLUS); break;
                case ';': AddToken(SEMICOLON); break;
                case '*': AddToken(STAR); break;

                case '!': AddToken(Match('=') ? BANG_EQUAL : BANG); break;
                case '=': AddToken(Match('=') ? EQUAL_EQUAL : EQUAL); break;
                case '<': AddToken(Match('=') ? LESS_EQUAL : LESS); break;
                case '>': AddToken(Match('=') ? GREATER_EQUAL : GREATER); break;

                case '/':
                    if(Match('/')) {
                        while(Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else {
                        AddToken(SLASH);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    break;

                case '\n':
                    line++;
                    break;

                case '"': String(); break;
                
                default:
                    if(IsDigit(c)) {
                        Number();
                    }
                    else if(IsAlpha(c)) {
                        Identifier();
                    }
                    else {
                        CSLox.Error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        private Boolean IsDigit(Char c) {
            return c >= '0' && c <= '9';
        }

        private Boolean IsAlpha(Char c) {
            return c >= 'a' && c <= 'z' ||
                   c >= 'A' && c <= 'Z' ||
                   c == '_';
        }

        private Boolean IsAlphaNumeric(Char c) {
            return IsAlpha(c) || IsDigit(c);
        }

        private Boolean Match(Char expected) {
            if(IsAtEnd()) return false;
            if(source[current] != expected) return false;

            current++;
            return true;
        }

        private Char Peek() {
            if(IsAtEnd()) return '\0';
            return source[current];
        }

        private Char PeekNext() {
            if(current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private void String() {
            while(Peek() != '"' && !IsAtEnd()) {
                if(Peek() == '\n') line++;
                Advance();
            }

            if(IsAtEnd()) {
                CSLox.Error(line, "Unterminated string.");
                return;
            }

            Advance();
            String value = source[(start + 1)..(current - 1)];
            AddToken(STRING, value);
        }

        private void Number() {
            while(IsDigit(Peek())) Advance();
            if(Peek() == '.' && IsDigit(PeekNext())) {
                Advance();
                while(IsDigit(Peek())) Advance();
            }
            AddToken(NUMBER, Double.Parse(source[start..current]));
        }

        private void Identifier() {
            while(IsAlphaNumeric(Peek())) Advance();
            String text = source[start..current];
            TokenType type = keywords.ContainsKey(text) ? keywords[text] : IDENTIFIER;
            AddToken(type);
        }

        private Boolean IsAtEnd() {
            return current >= source.Length;
        }
    }
}