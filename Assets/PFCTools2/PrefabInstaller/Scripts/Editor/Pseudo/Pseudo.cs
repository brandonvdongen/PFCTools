using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;
using PFCTools2.Utils;

namespace PFCTools2.Installer.PseudoParser {


    public partial class Pseudo {

        [MenuItem("Help/Clear Progress Bar")]
        public static void ClearProgressBar() {
            EditorUtility.ClearProgressBar();
        }


        public static List<PseudoAction> EnabledActions = new List<PseudoAction>{ new PseudoLayer(), new PseudoEntry(), new PseudoTransition(), new PseudoState(), new PseudoAny(), new PseudoEntry(), new PseudoExit() };

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

        internal static void Export(AnimatorController animatorController) {
            string export = "";
            ControllerContext Context = new ControllerContext(animatorController);
            foreach (var layer in Context.layers) {
                export += "layer " + layer.layer.name + "\n\n";
                foreach ( var state in layer.layer.stateMachine.states) {
                    if (state.state.name.Contains(" ")) {
                        export += "state \"" + state.state.name + "\"\n";
                    }
                    else {
                        export += "state " + state.state.name + "\n";
                    }
                        StateOffset offset = new StateOffset(state.position.x,state.position.y);
                    export += "pos " + offset.reverse.x + " " + offset.reverse.y + "\n"; 
                    if(state.state.motion != null) {
                        string path = AssetDatabase.GetAssetPath(state.state.motion);
                        path = Path.GetDirectoryName(path) +"\\"+ Path.GetFileNameWithoutExtension(path);
                        List<string> parts = new List<string>(path.Split(char.Parse("\\")));
                        int index = parts.IndexOf("Resources");
                        if (index != -1) {
                            index++;
                            path = string.Join("/",parts.GetRange(index, parts.Count - index));
                        }
                        else {
                            throw new Exception("Path is not in a Resources folder, the pseudo parser only accepts paths that are in a resource folder for auto installer failsafe reasons.");
                        }

                        export += "motion \"" + path + "\"";
                    }
                    export += "\n\n";
                }
            }
            FileHelper.CreateNewTextFile(animatorController, animatorController.name+"StateExport.txt", export, true);

        }
    }
}