using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoExit : PseudoAction {

        public override string ActionKey => "exit";
        public override ControllerContext Process(ControllerContext Context, TokenStream Tokens) {
            AnimatorLayerContext layerContext = Context.layers[Context.activeLayer];
            float x = float.Parse(Tokens.Next(new[] { TokenType.Int, TokenType.Float }).value);
            float y = float.Parse(Tokens.Next(new[] { TokenType.Int, TokenType.Float }).value);
            layerContext.layer.stateMachine.exitPosition = new SmallStateOffset(x,y).position;
            return Context;
        }
    }
}