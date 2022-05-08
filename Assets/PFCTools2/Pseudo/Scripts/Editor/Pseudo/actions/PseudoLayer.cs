using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;
using PFCTools2.Utils;

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoLayer : PseudoAction {

        public override string ActionKey => "layer";
        public override ControllerContext Process(ControllerContext context, TokenStream tokenStream, AvatarDefinition currentAvatar) {
            Token token = tokenStream.Next();
            if((token.type & TokenType.String) != TokenType.String) {
                tokenStream.Exception();
            }
            else {
                AnimatorControllerLayer newLayer = new AnimatorControllerLayer();
                newLayer.name = context.Controller.MakeUniqueLayerName(token.value);
                newLayer.stateMachine = new AnimatorStateMachine();
                newLayer.stateMachine.name = newLayer.name;
                newLayer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
                newLayer.defaultWeight = 1;
                if (AssetDatabase.GetAssetPath(context.Controller) != "")AssetDatabase.AddObjectToAsset(newLayer.stateMachine, AssetDatabase.GetAssetPath(context.Controller));
                context.Controller.AddLayer(newLayer);
                context.layers.Add(new AnimatorLayerContext(newLayer));
                context.activeLayer = context.Controller.layers.Length - 1;
            }
            return context;
        }
    }
}