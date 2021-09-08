using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoEntry : PseudoAction {

        public override string ActionKey => "entry";
        public override ControllerContext Process(ControllerContext Context, TokenStream Tokens) {
            Token token = Tokens.Next();
            if (!token.HasType(TokenType.String)) {
                Tokens.Exception();
            }
            else {
                AnimatorLayerContext layerContext = Context.layers[Context.activeLayer];
                AnimatorState state = layerContext.GetState(token.value);
                layerContext.layer.stateMachine.defaultState = state;
            }
            return Context;
        }
    }
}