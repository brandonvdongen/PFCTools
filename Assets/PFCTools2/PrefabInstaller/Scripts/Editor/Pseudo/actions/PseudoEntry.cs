using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;
using PFCTools2.Utils;

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoEntry : PseudoAction {

        public override string ActionKey => "entry";
        public override ControllerContext Process(ControllerContext context, TokenStream tokenStream, AvatarDefinition currentAvatar) {
            AnimatorLayerContext layerContext = context.layers[context.activeLayer];
            float x = float.Parse(tokenStream.Next(new[] { TokenType.Int, TokenType.Float }).value);
            float y = float.Parse(tokenStream.Next(new[] { TokenType.Int, TokenType.Float }).value);
            layerContext.layer.stateMachine.entryPosition = new SmallStateOffset(x, y).position;
            return context;
        }
    }
}