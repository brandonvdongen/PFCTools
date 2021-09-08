using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoTransition : PseudoAction {

        TokenType[] actionTokens = new[] { TokenType.String, TokenType.Operator, TokenType.String };
        TokenType[] transitionTokens = new[] { TokenType.String, TokenType.Comp, TokenType.Bool | TokenType.Float | TokenType.Int };

        Queue<TokenType> expectedTokens;
        public override string ActionKey => "transition";
        public override ControllerContext Process(ControllerContext Context, TokenStream Tokens) {
            AnimatorLayerContext layerContext = Context.layers[Context.activeLayer];

            AnimatorState startState = layerContext.GetState(Tokens.Next(TokenType.String).value);
            if (Tokens.Next(TokenType.Operator).value != "to") Tokens.Exception();
            AnimatorState endState = layerContext.GetState(Tokens.Next(TokenType.String).value);
            List<AnimatorStateTransition> transitions = new List<AnimatorStateTransition>();
            ProcessTransitions(Context, Tokens, transitions, startState, endState);
            while (!Tokens.EOF() && Tokens.Peek().HasType(TokenType.Operator) && Tokens.Peek().value == "or") {
                ProcessTransitions(Context, Tokens, transitions, startState, endState);
            }
            return Context;
        }

        private void ProcessTransitions(ControllerContext Context, TokenStream Tokens, List<AnimatorStateTransition> transitions, AnimatorState startState, AnimatorState endState) {
            AnimatorStateTransition Transition = startState.AddTransition(endState);
            transitions.Add(Transition);

            if (Tokens.EOF() || !Tokens.Peek().HasType(TokenType.Operator) || (Tokens.Peek().value != "when" && Tokens.Peek().value !="or")) return;
            Tokens.Next();//Consume the when

            ProcessParameter(Tokens, Transition, Context);
            while (!Tokens.EOF() && Tokens.Peek().HasType(TokenType.Operator) && Tokens.Peek().value == "and") {
                Tokens.Next();//consume the and
                ProcessParameter(Tokens, Transition, Context);
            }
        }
        private void ProcessParameter(TokenStream Tokens, AnimatorStateTransition Transition, ControllerContext Context) {
            string paramName = Tokens.Next(TokenType.String).value;
            string comp = Tokens.Next(TokenType.Comp).value;
            Token valueToken = Tokens.Next(new[] { TokenType.Bool, TokenType.Float, TokenType.Int });

            AnimatorConditionMode conditionMode = AnimatorConditionMode.Greater;
            AnimatorControllerParameterType paramType = AnimatorControllerParameterType.Int;

            if (valueToken.type == TokenType.Bool) {
                paramType = AnimatorControllerParameterType.Bool;
                if (comp == "==") conditionMode = AnimatorConditionMode.If;
                else if (comp == "!=") conditionMode = AnimatorConditionMode.IfNot;

                else Debug.LogError("Unsupported operation '" + comp + "' on type : " + valueToken.type);
            }
            else if (valueToken.type == TokenType.Int) {
                paramType = AnimatorControllerParameterType.Int;
                if (comp == "==") conditionMode = AnimatorConditionMode.Equals;
                else if (comp == "!=") conditionMode = AnimatorConditionMode.NotEqual;
                else if (comp == ">") conditionMode = AnimatorConditionMode.Greater;
                else if (comp == "<") conditionMode = AnimatorConditionMode.Less;

                else Debug.LogError("Unsupported operation '" + comp + "' on type : " + valueToken.type);
            }
            else if (valueToken.type == TokenType.Float) {
                paramType = AnimatorControllerParameterType.Float;
                if (comp == ">") conditionMode = AnimatorConditionMode.Greater;
                else if (comp == "<") conditionMode = AnimatorConditionMode.Less;

                else Debug.LogError("Unsupported operation '" + comp + "' on type : " + valueToken.type);
            }

            AnimatorControllerParameter param = Context.GetParameter(paramName, paramType);

            Transition.AddCondition(conditionMode, float.TryParse(valueToken.value, out float result) ? result : 0, param.name);
        }
    }
}