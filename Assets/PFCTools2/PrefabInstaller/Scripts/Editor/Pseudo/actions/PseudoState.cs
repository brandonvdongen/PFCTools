using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoState : PseudoAction {

        public override string ActionKey => "state";
        public override ControllerContext Process(ControllerContext Context, TokenStream Tokens) {
            Token token = Tokens.Next();
            if (!token.HasType(TokenType.String)) {
                Tokens.Exception();
            }
            else {
                AnimatorLayerContext layerContext = Context.layers[Context.activeLayer];
                AnimatorState state = layerContext.GetState(token.value);
                //layerContext.layer.stateMachine.defaultState = state;

                while (!Tokens.EOF() && Tokens.Peek().HasType(TokenType.String)) {
                    Token modifier = Tokens.Next(TokenType.String);
                    if (modifier.value.ToLower() == "motion") {
                        string clippath = Tokens.Next(TokenType.String).value;
                        AnimationClip clip = Resources.Load<AnimationClip>(clippath);
                        Debug.Log(clippath);
                        Debug.Log(clip);
                        state.motion = clip;
                    }
                    else if (modifier.value.ToLower() == "speed") {
                        state.speed = float.Parse(Tokens.Next(new[] { TokenType.Int, TokenType.Float }).value);
                    }

                    else if (modifier.value.ToLower() == "multiplier") {
                        state.speedParameter = Context.GetParameter(Tokens.Next(TokenType.String).value, AnimatorControllerParameterType.Bool).name; ;
                        state.speedParameterActive = true;
                    }
                    else if (modifier.value.ToLower() == "motiontime") {
                        state.timeParameter = Context.GetParameter(Tokens.Next(TokenType.String).value, AnimatorControllerParameterType.Bool).name; ;
                        state.timeParameterActive = true;
                    }
                    else if (modifier.value.ToLower() == "mirror") {
                        if (Tokens.Peek().HasType(TokenType.String)) {
                            state.mirrorParameterActive = true;
                            state.mirrorParameter = Context.GetParameter(Tokens.Next(TokenType.String).value, AnimatorControllerParameterType.Bool).name;
                        }
                        else if (Tokens.Peek().HasType(TokenType.Bool)) {
                            state.mirror = Tokens.Next(TokenType.Bool).value == "true" ? true : false;
                        }
                    }
                    else if (modifier.value.ToLower() == "cycle") {

                        if (Tokens.Peek().HasType(TokenType.String)) {
                            state.cycleOffsetParameterActive = true;
                            state.cycleOffsetParameter = Context.GetParameter(Tokens.Next(TokenType.String).value, AnimatorControllerParameterType.Bool).name;
                        }
                        else if (Tokens.Peek().HasType(new[] { TokenType.Int, TokenType.Float })) {
                            state.cycleOffset = float.Parse(Tokens.Next(new[] { TokenType.Int, TokenType.Float }).value);
                        }
                    }
                    else if (modifier.value.ToLower() == "writedefaults") {
                        bool on = Tokens.Next(TokenType.Bool).value == "true" ? true : false;
                        state.writeDefaultValues = on;
                    }
                    else if (modifier.value.ToLower() == "footik") {
                        bool on = Tokens.Next(TokenType.Bool).value == "true" ? true : false;
                        state.iKOnFeet = on;
                    }
                    else Tokens.Exception();

                }
            }
            return Context;
        }
    }
}