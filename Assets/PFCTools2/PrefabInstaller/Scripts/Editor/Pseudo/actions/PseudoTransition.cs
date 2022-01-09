using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoTransition : PseudoAction {

        public override string ActionKey => "transition";
        public override ControllerContext Process(ControllerContext Context, TokenStream Tokens) {
            AnimatorLayerContext layerContext = Context.layers[Context.activeLayer];
            List<AnimatorStateTransition> transitions = new List<AnimatorStateTransition>();

            //Get Start State
            Token startToken = Tokens.Next(TokenType.String);
            AnimatorState startState;
            if(startToken.value.ToLower() == "any") {
                startState = new AnimatorState() { name = "any" };
            }
            else {
                startState = layerContext.GetState(startToken.value);
            }
            //Check if next state if the 'to' operator
            if (Tokens.Next(TokenType.Operator).value != "to") Tokens.Exception();
            //Get End State
            AnimatorState endState = layerContext.GetState(Tokens.Next(TokenType.String).value);
            
            ProcessTransitions(Context, Tokens, transitions, startState, endState);
            //if OR operator exists, rerun transition creation for a secondary transition.
            while (!Tokens.EOF() && Tokens.Peek().HasType(TokenType.Operator) && Tokens.Peek().value == "or") {
                ProcessTransitions(Context, Tokens, transitions, startState, endState);
            }

            //read and set config for transitions for that transitions.
            while(!Tokens.EOF() && Tokens.Peek().HasType(TokenType.String)) {
                Token token = Tokens.Next(TokenType.String);
                if (token.value.ToLower() == "exittime") {
                    float exitTime = float.Parse(Tokens.Next(new[] { TokenType.Int, TokenType.Float }).value);
                    foreach (AnimatorStateTransition transition in transitions) {
                        transition.hasExitTime = true;
                        transition.exitTime = exitTime;
                    }
                }
                else if (token.value.ToLower() == "duration") {
                    float duration = float.Parse(Tokens.Next(new[] { TokenType.Int, TokenType.Float }).value);
                    bool fixedDuration = false;

                    if (Tokens.Peek().HasType(TokenType.String) && Tokens.Peek().value.ToLower() == "fixed") {
                        Tokens.Next();//Consume fixed token;
                        fixedDuration = true;
                    }

                    foreach (AnimatorStateTransition transition in transitions) {
                        transition.hasFixedDuration = fixedDuration;
                        transition.duration = duration;
                    }
                }
                else if (token.value.ToLower() == "offset") {
                    float offset = float.Parse(Tokens.Next(new[] { TokenType.Int, TokenType.Float }).value);
                    foreach (AnimatorStateTransition transition in transitions) {
                        transition.offset = offset;
                    }
                }
                else if (token.value.ToLower() == "interruption") {
                    Token type = Tokens.Next(TokenType.String);
                    TransitionInterruptionSource source = TransitionInterruptionSource.None;
                    bool ordered = false;
                    switch (type.value.ToLower()) {
                        case "current":
                            source = TransitionInterruptionSource.Source;
                            break;
                        case "next":
                            source = TransitionInterruptionSource.Destination;
                            break;
                        case "currentthennext":
                            source = TransitionInterruptionSource.SourceThenDestination;
                            break;
                        case "nextthencurrent":
                            source = TransitionInterruptionSource.DestinationThenSource;
                            break;
                    }
                    if (Tokens.Peek().HasType(TokenType.String) && Tokens.Peek().value.ToLower() == "ordered") {
                        Tokens.Next();//Consume Ordered;
                        ordered = true;
                    }
                    foreach (AnimatorStateTransition transition in transitions) {
                        transition.interruptionSource = source;
                        transition.orderedInterruption = ordered;
                    }
                }
                else if (token.value.ToLower() == "cantransitiontoself") {
                    foreach(AnimatorStateTransition transition in transitions) {
                        transition.canTransitionToSelf = true;
                    }
                }
                else Tokens.Exception();

            }

            return Context;
        }

        private void ProcessTransitions(ControllerContext Context, TokenStream Tokens, List<AnimatorStateTransition> transitions, AnimatorState startState, AnimatorState endState) {
            //Create Transition
            AnimatorStateTransition Transition;
            AnimatorLayerContext layerContext = Context.layers[Context.activeLayer];
            if (endState.name.ToLower() == "any") Tokens.Exception("Any cannot be used as a target, it does not accept incoming transitions.");
            if (startState.name.ToLower() == "any") {
                Transition = layerContext.layer.stateMachine.AddAnyStateTransition(endState);
            }
            else {
               Transition = startState.AddTransition(endState);
            }


            transitions.Add(Transition);
            Transition.hasExitTime = false;
            Transition.exitTime = 1;
            Transition.duration = 0;
            Transition.canTransitionToSelf = false;

            //Check if the token was a 'when' or an 'or' to denote that there should be one or multiple conditions.
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
                if (comp == "==")
                if(valueToken.value.ToLower() == "true")conditionMode = AnimatorConditionMode.If;
                else if (valueToken.value.ToLower() == "false") conditionMode = AnimatorConditionMode.IfNot;
                else Tokens.Exception();

                else Tokens.Exception("Unsupported operation '" + comp + "' on type : " + valueToken.type);
            }
            else if (valueToken.type == TokenType.Int) {
                paramType = AnimatorControllerParameterType.Int;
                if (comp == "==") conditionMode = AnimatorConditionMode.Equals;
                else if (comp == "!=") conditionMode = AnimatorConditionMode.NotEqual;
                else if (comp == ">") conditionMode = AnimatorConditionMode.Greater;
                else if (comp == "<") conditionMode = AnimatorConditionMode.Less;

                else Tokens.Exception("Unsupported operation '" + comp + "' on type : " + valueToken.type);
            }
            else if (valueToken.type == TokenType.Float) {
                paramType = AnimatorControllerParameterType.Float;
                if (comp == ">") conditionMode = AnimatorConditionMode.Greater;
                else if (comp == "<") conditionMode = AnimatorConditionMode.Less;

                else Tokens.Exception("Unsupported operation '" + comp + "' on type : " + valueToken.type);
            }

            AnimatorControllerParameter param = Context.GetParameter(paramName, paramType);

            Transition.AddCondition(conditionMode, float.TryParse(valueToken.value, out float result) ? result : 0, param.name);
        }
    }
}