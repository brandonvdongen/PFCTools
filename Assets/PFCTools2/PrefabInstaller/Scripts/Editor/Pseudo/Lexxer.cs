using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PFCTools2.Installer.PseudoParser {
    public partial class Pseudo {
        public static List<Token> Lexxer(TextAsset file) {

            List<string> actionKeys = new List<string>();
            foreach(PseudoAction action in EnabledActions) {
                actionKeys.Add(action.ActionKey);
            }


            string[] lines = file.text.Split(char.Parse("\n"));
            List<Token> tokenList = new List<Token>();
            int lineIndex = 0;
            foreach (string line in lines) {
                lineIndex++;
                int posIndex = 0;
                string[] parts = Regex.Split(line, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                TokenType previousLexType = TokenType.Mismatch;
                foreach (string part in parts) {
                    TokenType type = TokenType.Mismatch;
                    if (string.IsNullOrWhiteSpace(part)) continue;//if empty line
                    else if (part.StartsWith("//")) break;//if comment exit line loop
                    else if (int.TryParse(part, out int intOut)) type = TokenType.Int;
                    else if (float.TryParse(part, out float floatOut)) type = TokenType.Float;
                    else if (actionKeys.Contains(part)) type = TokenType.Action;
                    else { 
                        switch (part) {
                            case "to":
                            case "from":
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
                            case "true":
                            case "false":
                                type = TokenType.Bool;
                                break;
                            default:
                                type = TokenType.String;
                                break;
                        };
                    }
                    string output = part;
                    if(part.StartsWith("\"") && part.EndsWith("\"")) {
                        output = part.Substring(1, part.Length - 2);
                    }
                    posIndex++;
                    previousLexType = type;

                    tokenList.Add(new Token(output, type, lineIndex,posIndex));
                }
            }
            return tokenList;
        }
    }

}