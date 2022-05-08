using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;
using PFCTools2.Utils;
#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.Components;
#endif

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoState : PseudoAction {
        public override string ActionKey => "state";
        public override ControllerContext Process(ControllerContext context, TokenStream tokenStream, AvatarDefinition currentAvatar) {
            Token token = tokenStream.Next(TokenType.String);
            AnimatorLayerContext layerContext = context.layers[context.activeLayer];
            AnimatorState state = layerContext.GetState(token.value);
            //layerContext.layer.stateMachine.defaultState = state;

            while (!tokenStream.EOF() && tokenStream.Peek().HasType(TokenType.String)) {
                Token modifier = tokenStream.Next(TokenType.String);
                if (modifier.value.ToLower() == "motion") {
                    string clippath = tokenStream.Next(TokenType.String).value;
                    Motion clip = Resources.Load<Motion>(clippath);
                    //Debug.Log(clippath);
                    //Debug.Log(clip);
                    if (clip == null) tokenStream.Warning("Motion " + clippath + " not found!");
                    state.motion = clip;
                }
                else if (modifier.value.ToLower() == "default") {
                    layerContext.layer.stateMachine.defaultState = state;
                }
                else if (modifier.value.ToLower() == "pos") {
                    float x = float.Parse(tokenStream.Next(new[] { TokenType.Int, TokenType.Float }).value);
                    float y = float.Parse(tokenStream.Next(new[] { TokenType.Int, TokenType.Float }).value);

                    ChildAnimatorState childState = layerContext.GetChildState(state);
                    layerContext.SetStatePos(state, x, y);

                }
                else if (modifier.value.ToLower() == "speed") {
                    state.speed = float.Parse(tokenStream.Next(new[] { TokenType.Int, TokenType.Float }).value);
                }

                else if (modifier.value.ToLower() == "multiplier") {
                    state.speedParameter = context.GetParameter(tokenStream.Next(TokenType.String).value, AnimatorControllerParameterType.Bool).name; ;
                    state.speedParameterActive = true;
                }
                else if (modifier.value.ToLower() == "motiontime") {
                    state.timeParameter = context.GetParameter(tokenStream.Next(TokenType.String).value, AnimatorControllerParameterType.Bool).name; ;
                    state.timeParameterActive = true;
                }
                else if (modifier.value.ToLower() == "mirror") {
                    if (tokenStream.Peek().HasType(TokenType.String)) {
                        state.mirrorParameterActive = true;
                        state.mirrorParameter = context.GetParameter(tokenStream.Next(TokenType.String).value, AnimatorControllerParameterType.Bool).name;
                    }
                    else if (tokenStream.Peek().HasType(TokenType.Bool)) {
                        state.mirror = tokenStream.Next(TokenType.Bool).value == "true" ? true : false;
                    }
                }
                else if (modifier.value.ToLower() == "cycle") {

                    if (tokenStream.Peek().HasType(TokenType.String)) {
                        state.cycleOffsetParameterActive = true;
                        state.cycleOffsetParameter = context.GetParameter(tokenStream.Next(TokenType.String).value, AnimatorControllerParameterType.Bool).name;
                    }
                    else if (tokenStream.Peek().HasType(new[] { TokenType.Int, TokenType.Float })) {
                        state.cycleOffset = float.Parse(tokenStream.Next(new[] { TokenType.Int, TokenType.Float }).value);
                    }
                }
                else if (modifier.value.ToLower() == "writedefaults") {
                    bool on = tokenStream.Next(TokenType.Bool).value == "true" ? true : false;
                    state.writeDefaultValues = on;
                }
                else if (modifier.value.ToLower() == "footik") {
                    bool on = tokenStream.Next(TokenType.Bool).value == "true" ? true : false;
                    state.iKOnFeet = on;
                }
                else tokenStream.Exception();

            }
            return context;
        }
    }
}