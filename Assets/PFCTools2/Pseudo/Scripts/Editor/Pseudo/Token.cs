namespace PFCTools2.Installer.PseudoParser {

    public class Token {
        public string value;
        public TokenType type;
        public int line;
        public int pos;
        public Token(string part, TokenType type, int line, int pos) {
            this.value = part;
            this.type = type;
            this.line = line;
            this.pos = pos;
        }
        public bool HasType(TokenType type) {
            return (this.type & type) == type;
        }
        public bool HasType(TokenType[] types) {
            bool found = false;
            foreach (var type in types) {
                if (HasType(type)) {
                    found = true;
                }
            }
            return found;
        }
        public bool IsType(TokenType type) {
            return this.type == type;
        }
    }
    public enum TokenType {
        Mismatch = 0,
        Action = 1 << 1,
        Comp = 1 << 2,
        Operator = 1 << 3,
        String = 1 << 6,
        Float = 1 << 7,
        Int = 1 << 8,
        Bool = 1 << 9,
    }

}