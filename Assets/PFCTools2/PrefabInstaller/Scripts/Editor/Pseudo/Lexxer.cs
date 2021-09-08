using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PFCTools2.Installer.PseudoParser {
    public partial class Pseudo {
        public static List<Token> Lexxer(TextAsset file) {
            string[] lines = file.text.Split(char.Parse("\n"));
            List<Token> tokenList = new List<Token>();
            int lineIndex = 0;
            foreach (string line in lines) {
                lineIndex++;
                int posIndex = 0;
                string[] parts = Regex.Split(line, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                TokenType previousLexType = TokenType.Mismatch;
                foreach (string part in parts) {
                    TokenType type = TokenType.String;
                    if (string.IsNullOrWhiteSpace(part)) continue;//if empty line
                    else if (part.StartsWith("//")) break;//if comment exit line loop
                    else if (int.TryParse(part, out int intOut)) type = TokenType.Int;
                    else if (float.TryParse(part, out float floatOut)) type = TokenType.Float;
                    
                    else switch (part) {
                            case "to":
                            case "then":
                            case "when":
                            case "and":
                            case "or":
                                type = TokenType.Operator;
                                break;
                            case "==":
                            case "!=":
                            case ">":
                            case "<":
                                type = TokenType.Comp;
                                break;
                            case "layer":
                            case "entry":
                            case "transition":
                                type = TokenType.Action;
                                break;
                            case "true":
                            case "false":
                                type = TokenType.Bool;
                                break;
                        };

                    posIndex++;
                    previousLexType = type;
                    tokenList.Add(new Token(part, type, lineIndex,posIndex));
                }
            }
            return tokenList;
        }
    }

}