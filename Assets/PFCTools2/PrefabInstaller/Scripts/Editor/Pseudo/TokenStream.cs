using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PFCTools2.Installer.PseudoParser {
    public class TokenStream {
        int index = 0;
        List<Token> _input;
        public float progress { get { return index / _input.Count; } }
        public TokenStream(TextAsset file) {
            _input = Pseudo.Lexxer(file);
        }
        public TokenStream(List<Token> tokens) {

            _input = tokens;
        }
        public Token Next() {
            return _input[index++];
        }
        public Token Next(TokenType type) {
            Token token = _input[index++];
            if (!token.HasType(type)) {
                Exception("",type.ToString());
            }
            return token;
        }
        public Token Next(TokenType[] types) {
            Token token = _input[index++];
            bool found = false;
            foreach(var type in types) {
                if (token.HasType(type)) {
                    found = true;
                }
            }
            if (!found) Exception();

            return token;
        }
        public Token Peek() {
            if (EOF()) Exception();
            return _input[index];
        }        
        public bool EOF() {
            return index >= _input.Count;
        }
        public void Exception(string additionalInfo = "", string expected = "") {
            EditorUtility.ClearProgressBar();
            if (additionalInfo == "") {
                throw new Exception("Unexpected " + _input[index - 1].type + " \"" + _input[index - 1].value + "\", on Line : " + _input[index - 1].line + " Position : " + _input[index - 1].pos + (expected != "" ? " expected: " + expected : ""));
            }
            else {
                throw new Exception(additionalInfo + " | on Line : " + _input[index - 1].line + " Position : " + _input[index - 1].pos + ( expected != "" ? " expected: " + expected : ""));
            }
        }

    }

}