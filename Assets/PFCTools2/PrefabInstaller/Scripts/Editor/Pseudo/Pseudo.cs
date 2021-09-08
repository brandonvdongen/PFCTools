using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PFCTools2.Installer.PseudoParser {


    public partial class Pseudo {

        [MenuItem("Help/Clear Progress Bar")]
        public static void ClearProgressBar() {
            EditorUtility.ClearProgressBar();
        }


        public static List<PseudoAction> PseudoActions = new List<PseudoAction>{ new PseudoLayer(), new PseudoEntry(), new PseudoTransition() };

        public static void Parse(TextAsset asset) {
            string path = AssetDatabase.GetAssetPath(asset);
            Parse(Lexxer(asset),path);
        }
        public static void Parse(List<Token> tokenList, string path) {
            TokenStream Tokens = new TokenStream(tokenList);
            ControllerContext context = new ControllerContext(path);
            while (!Tokens.EOF()) {
                Debug.Log("Processing Pseudo Code: Progress :" + Tokens.progress);
                Token token = Tokens.Next();
                if((token.type & TokenType.Action) == TokenType.Mismatch) {
                    Tokens.Exception();
                }

                bool foundAction = false;
                foreach (PseudoAction action in PseudoActions) {                    
                    if(token.value == action.ActionKey) {
                       context = action.Process(context, Tokens);
                       foundAction = true;
                       break;
                    }
                }
                if (!foundAction) Tokens.Exception();
            }
            
        }
    }


    

    public class ProgramTree {
        Queue<PseudoAction> actions = new Queue<PseudoAction>();
    }
}