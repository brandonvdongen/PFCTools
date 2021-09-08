using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoLayer : PseudoAction {

        public override string ActionKey => "layer";
        public override ControllerContext Process(ControllerContext Context, TokenStream Tokens) {
            Token token = Tokens.Next();
            if((token.type & TokenType.String) != TokenType.String) {
                Tokens.Exception();
            }
            else {
                AnimatorControllerLayer newLayer = new AnimatorControllerLayer();
                newLayer.name = Context.Controller.MakeUniqueLayerName(token.value);
                newLayer.stateMachine = new AnimatorStateMachine();
                newLayer.stateMachine.name = newLayer.name;
                newLayer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
                newLayer.defaultWeight = 1;
                if (AssetDatabase.GetAssetPath(Context.Controller) != "")AssetDatabase.AddObjectToAsset(newLayer.stateMachine, AssetDatabase.GetAssetPath(Context.Controller));
                Context.Controller.AddLayer(newLayer);
                Context.layers.Add(new AnimatorLayerContext(newLayer));
                Context.activeLayer = Context.Controller.layers.Length - 1;
            }
            return Context;
        }
    }
}