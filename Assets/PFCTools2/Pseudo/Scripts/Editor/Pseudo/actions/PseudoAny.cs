using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;
using PFCTools2.Utils;

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoAny : PseudoAction {

        public override string ActionKey => "any";
        public override ControllerContext Process(ControllerContext context, TokenStream tokenStream, AvatarDefinition currentAvatar) {
            AnimatorLayerContext layerContext = context.layers[context.activeLayer];
            float x = float.Parse(tokenStream.Next(new[] { TokenType.Int, TokenType.Float }).value);
            float y = float.Parse(tokenStream.Next(new[] { TokenType.Int, TokenType.Float }).value);
            layerContext.layer.stateMachine.anyStatePosition = new SmallStateOffset(x, y).position;
            return context;
        }
    }
}