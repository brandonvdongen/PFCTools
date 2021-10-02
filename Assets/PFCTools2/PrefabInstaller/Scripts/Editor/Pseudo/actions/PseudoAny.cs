using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoAny : PseudoAction {

        public override string ActionKey => "any";
        public override ControllerContext Process(ControllerContext Context, TokenStream Tokens) {
            AnimatorLayerContext layerContext = Context.layers[Context.activeLayer];
            float x = float.Parse(Tokens.Next(new[] { TokenType.Int, TokenType.Float }).value);
            float y = float.Parse(Tokens.Next(new[] { TokenType.Int, TokenType.Float }).value);
            layerContext.layer.stateMachine.anyStatePosition = new SmallStateOffset(x, y).position;
            return Context;
        }
    }
}