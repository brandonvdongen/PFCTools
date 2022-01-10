#if VRC_SDK_VRCSDK3
using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using PFCTools2.Utils;

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoVRCParameterDriver : PseudoAction {
        public override string ActionKey => "vrcparameterdriver";
        public override ControllerContext Process(ControllerContext context, TokenStream tokensStream, AvatarDefinition currentAvatar) {
            AnimatorLayerContext layerContext = context.layers[context.activeLayer];
            AnimatorState state = layerContext.lastInteractedState;
            VRCAvatarParameterDriver driver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();

            while (!tokensStream.EOF() && tokensStream.Peek().HasType(TokenType.String)) {
                Token modifier = tokensStream.Next(TokenType.String);
                //Debug.Log(modifier.value);
                
                if (modifier.value.ToLower() == "localonly") {
                    driver.localOnly = true;
                    continue;
                }
                Token nameToken = tokensStream.Next(TokenType.String);
                Token valueToken = tokensStream.Next(new[] { TokenType.Float, TokenType.Int, TokenType.Bool });
                TokenType tokenType = valueToken.type;
                AnimatorControllerParameterType parameterType = AnimatorControllerParameterType.Bool;
                if (tokenType == TokenType.Int) parameterType = AnimatorControllerParameterType.Int;
                if (tokenType == TokenType.Float) parameterType = AnimatorControllerParameterType.Float;
                AnimatorControllerParameter animatorParameter = context.GetParameter(nameToken.value, parameterType);
                if (animatorParameter.type == AnimatorControllerParameterType.Bool) {
                    VRCAvatarParameterDriver.Parameter driverParameter = new VRCAvatarParameterDriver.Parameter();
                    driverParameter.name = animatorParameter.name;
                    if (modifier.value.ToLower() == "set") {
                        if (valueToken.type != TokenType.Bool) tokensStream.Exception("Incorrect type", "Bool");
                        driverParameter.type = VRCAvatarParameterDriver.ChangeType.Set;
                        //Debug.Log(valueToken.value);
                        driverParameter.value = bool.Parse(valueToken.value)?1:0;
                        
                    }
                    else if (modifier.value.ToLower() == "random") {
                        if (valueToken.type != TokenType.Float) tokensStream.Exception("Incorrect type", "Float");
                        driverParameter.type = VRCAvatarParameterDriver.ChangeType.Random;
                        driverParameter.chance = float.Parse(valueToken.value);
                    }
                    else {
                        tokensStream.Exception("method " + modifier.value + " not supported on bool type parameters");
                    }
                    driver.parameters.Add(driverParameter);
                }
                else if(animatorParameter.type == AnimatorControllerParameterType.Float) {
                    if (valueToken.type != TokenType.Float) tokensStream.Exception("Incorrect type", "Float");
                    VRCAvatarParameterDriver.Parameter driverParameter = new VRCAvatarParameterDriver.Parameter();
                    driverParameter.name = animatorParameter.name;
                    if (modifier.value.ToLower() == "set") {
                        driverParameter.type = VRCAvatarParameterDriver.ChangeType.Set;
                        driverParameter.value = float.Parse(valueToken.value);
                    }
                    else if (modifier.value.ToLower() == "random") {
                        driverParameter.type = VRCAvatarParameterDriver.ChangeType.Random;
                        driverParameter.valueMin = float.Parse(valueToken.value);
                        driverParameter.valueMax = float.Parse(tokensStream.Next(new[] { TokenType.Float, TokenType.Int }).value);
                    }
                    else if (modifier.value.ToLower() == "add") {
                        driverParameter.type = VRCAvatarParameterDriver.ChangeType.Add;
                        driverParameter.value = float.Parse(valueToken.value);
                    }
                    else {
                        tokensStream.Exception("method " + modifier.value + " not supported on float type parameters");
                    }
                    driver.parameters.Add(driverParameter);
                }
                else if (animatorParameter.type == AnimatorControllerParameterType.Int) {
                    if (valueToken.type != TokenType.Int) tokensStream.Exception("Incorrect type", "Int");
                    VRCAvatarParameterDriver.Parameter driverParameter = new VRCAvatarParameterDriver.Parameter();
                    driverParameter.name = animatorParameter.name;
                    if (modifier.value.ToLower() == "set") {
                        driverParameter.type = VRCAvatarParameterDriver.ChangeType.Set;
                        driverParameter.value = int.Parse(valueToken.value);
                    }
                    else if (modifier.value.ToLower() == "random") {
                        driverParameter.type = VRCAvatarParameterDriver.ChangeType.Random;
                        driverParameter.valueMin = int.Parse(valueToken.value);
                        driverParameter.valueMax = int.Parse(tokensStream.Next(new[] { TokenType.Float, TokenType.Int }).value);
                    }
                    else if (modifier.value.ToLower() == "add") {
                        driverParameter.type = VRCAvatarParameterDriver.ChangeType.Add;
                        driverParameter.value = int.Parse(valueToken.value);
                    }
                    else {
                        tokensStream.Exception("method " + modifier.value + " not supported on int type parameters");
                    }
                    driver.parameters.Add(driverParameter);
                }
                else {
                    tokensStream.Exception("type " + animatorParameter.type + " not supported by parameterdriver");
                }
            }
            return context;
        }
    }
}
#endif