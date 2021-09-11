using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;

namespace PFCTools2.Installer.PseudoParser {


    public partial class Pseudo {

        [MenuItem("Help/Clear Progress Bar")]
        public static void ClearProgressBar() {
            EditorUtility.ClearProgressBar();
        }


        public static List<PseudoAction> EnabledActions = new List<PseudoAction>{ new PseudoLayer(), new PseudoEntry(), new PseudoTransition(), new PseudoState() };

        public static void Parse(TextAsset asset, AnimatorController controller = null) {
            string path = AssetDatabase.GetAssetPath(asset);
            Parse(Lexxer(asset),path, controller);
        }
        public static void Parse(List<Token> tokenList, string path, AnimatorController controller = null) {
            TokenStream Tokens = new TokenStream(tokenList);
            ControllerContext context;
            if (controller) {
                 context = new ControllerContext(controller);
            }
            else {
                context = new ControllerContext(path);
            }
            while (!Tokens.EOF()) {
                Token token = Tokens.Next();
                if((token.type & TokenType.Action) == TokenType.Mismatch) {
                    Tokens.Exception();
                }

                bool foundAction = false;
                foreach (PseudoAction action in EnabledActions) {                    
                    if(token.value == action.ActionKey) {
                       context = action.Process(context, Tokens);
                       foundAction = true;
                       break;
                    }
                }
                if (!foundAction) Tokens.Exception();
                if(controller)EditorUtility.SetDirty(controller);
            }
            
        }
    }
}