using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;
using PFCTools2.Utils;

using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace PFCTools2.Installer.PseudoParser {


    public partial class Pseudo {

        [MenuItem("Help/Clear Progress Bar")]
        public static void ClearProgressBar() {
            EditorUtility.ClearProgressBar();
        }


        public static List<PseudoAction> EnabledActions = new List<PseudoAction> { new PseudoLayer(), new PseudoEntry(), new PseudoTransition(), new PseudoState(), new PseudoAny(), new PseudoEntry(), new PseudoExit(), new PseudoParameter() };
        public static List<PseudoAction> EnabledVRCActions = new List<PseudoAction> { new PseudoVRCParameterDriver() };
        public static List<PseudoAction> getEnabledActions() {
            List<PseudoAction> availableActions = new List<PseudoAction>();
            availableActions.AddRange(EnabledActions);
            availableActions.AddRange(EnabledVRCActions);
            return availableActions;
        }
        public static void Parse(TextAsset asset, AvatarDefinition currentAvatar, AnimatorController controller = null) {
            string path = AssetDatabase.GetAssetPath(asset);
            Parse(Lexxer(asset), path, currentAvatar, controller);
        }
        public static void Parse(List<Token> tokenList, string path, AvatarDefinition currentAvatar, AnimatorController controller = null) {
            TokenStream Tokens = new TokenStream(tokenList);
            ControllerContext controllerContext;
            if (controller) {
                controllerContext = new ControllerContext(controller);
            }
            else {
                controllerContext = new ControllerContext(path);
            }
            while (!Tokens.EOF()) {
                Token token = Tokens.Next();
                if ((token.type & TokenType.Action) == TokenType.Mismatch) {
                    Tokens.Exception();
                }

                bool foundAction = false;
                foreach (PseudoAction action in getEnabledActions()) {
                    if (token.value == action.ActionKey) {
                        controllerContext = action.Process(controllerContext, Tokens, currentAvatar);
                        foundAction = true;
                        break;
                    }
                }
                if (!foundAction) Tokens.Exception();
                if (controller) EditorUtility.SetDirty(controller);
            }

        }

        public static void Remove(TextAsset asset, AvatarDefinition currentAvatar, AnimatorController controller) {
            string path = AssetDatabase.GetAssetPath(asset);
            Remove(Lexxer(asset), path, currentAvatar, controller);
        }
        public static void Remove(List<Token> tokenList, string path, AvatarDefinition currentAvatar, AnimatorController controller) {
            TokenStream Tokens = new TokenStream(tokenList);
            ControllerContext controllerContext;
            if (controller == null) {
                throw new Exception("No controller was given, aborting.");
            }
            else {
                controllerContext = new ControllerContext(controller);
            }
            while (!Tokens.EOF()) {
                Token token = Tokens.Next();
                if ((token.type & TokenType.Action) == TokenType.Mismatch) {
                    Tokens.Exception();
                }

                bool foundAction = false;
                foreach (PseudoAction action in getEnabledActions()) {
                    if (token.value == action.ActionKey) {
                        controllerContext = action.Remove(controllerContext, Tokens, currentAvatar);
                        foundAction = true;
                        break;
                    }
                }
                if (!foundAction) Tokens.Exception();
                if (controller) EditorUtility.SetDirty(controller);
            }

        }

        /*
        public static void RemoveLayers(TextAsset asset, AvatarDefinition currentAvatar, AnimatorController controller) {
            string path = AssetDatabase.GetAssetPath(asset);
            RemoveLayers(Lexxer(asset), path, currentAvatar, controller);
        }
        public static void RemoveLayers(List<Token> tokenList, string path, AvatarDefinition currentAvatar, AnimatorController controller) {
            string layerKey = new PseudoLayer().ActionKey;
            string parameterKey = new PseudoParameter().ActionKey;

            TokenStream Tokens = new TokenStream(tokenList);
            while (!Tokens.EOF()) {
                Token token = Tokens.Next();
                if (token.value == layerKey) {
                    string layerName = Tokens.Next().value;
                    Debug.Log("Found layer: "+ layerName);

                    for (var i = 0; i < controller.layers.Length; i++) {
                        if(controller.layers[i].name == layerName) {
                            controller.RemoveLayer(i);
                            EditorUtility.SetDirty(controller);
                            break;
                        }
                    }

                }
                else if(token.value == parameterKey) {
                    string parameterName = Tokens.Next().value;
                    Debug.Log("Found parameter: " + parameterName + " in animator, removing...");

                    for(var i = 0; i < controller.parameters.Length; i++) {
                        if(controller.parameters[i].name == parameterName) {
                            controller.RemoveParameter(i);
                            EditorUtility.SetDirty(controller);

                            List<VRCExpressionParameters.Parameter> parameters = new List<VRCExpressionParameters.Parameter>(currentAvatar.ExpressionParameters.parameters);
                            VRCExpressionParameters.Parameter param = currentAvatar.ExpressionParameters.FindParameter(parameterName);
                            if (param != null) {
                                Debug.Log("Found Parameter: " + parameterName + " in expression parameters, removing...");
                                parameters.Remove(param);
                                currentAvatar.ExpressionParameters.parameters = parameters.ToArray();
                                EditorUtility.SetDirty(currentAvatar.ExpressionParameters);
                            }
                            break;
                        }
                        
                    }
                }
                
                
                

            }

        }
        */
        internal static void Export(AnimatorController animatorController) {
            string export = "";
            ControllerContext Context = new ControllerContext(animatorController);

            foreach (KeyValuePair<string, AnimatorControllerParameter> data in Context.parameters) {
                export += "parameter " + data.Value.name;
                if(data.Value.type == AnimatorControllerParameterType.Float)export += " float";
                if(data.Value.type == AnimatorControllerParameterType.Int)export += " int";
                if(data.Value.type == AnimatorControllerParameterType.Bool)export += " bool";
                export += "\n";
            }
            export += "\n";

            foreach (var layerContext in Context.layers) {
                string layerExport = "";
                layerExport += "layer " + StringUtils.parseQuotes(layerContext.layer.name) + "\n\n";

                SmallStateOffset entry = new SmallStateOffset(layerContext.layer.stateMachine.entryPosition.x, layerContext.layer.stateMachine.entryPosition.y);
                SmallStateOffset exit = new SmallStateOffset(layerContext.layer.stateMachine.exitPosition.x, layerContext.layer.stateMachine.exitPosition.y);
                SmallStateOffset any = new SmallStateOffset(layerContext.layer.stateMachine.anyStatePosition.x, layerContext.layer.stateMachine.anyStatePosition.y);

                List<TransitionData> transitionSets = new List<TransitionData>();

                layerExport += "entry " + entry.reverse.x + " " + entry.reverse.y + "\n\n";
                layerExport += "exit " + exit.reverse.x + " " + exit.reverse.y + "\n\n";
                layerExport += "any " + any.reverse.x + " " + any.reverse.y + "\n\n";
                //Debug.Log("*****LAYER CHANGE: "+layerContext.layer.name+" *****");
                
                foreach (var state in layerContext.layer.stateMachine.states) {
                    //Debug.Log(state.state.name);
                    layerExport += "state " + StringUtils.parseQuotes(state.state.name) + "\n";
                    if(layerContext.layer.stateMachine.defaultState == state.state) {
                        layerExport += "default \n";
                    }
                    StateOffset offset = new StateOffset(state.position.x, state.position.y);
                    layerExport += "pos " + offset.reverse.x + " " + offset.reverse.y + "\n";
                    if (state.state.motion != null) {
                        string path = AssetDatabase.GetAssetPath(state.state.motion);
                        path = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path);
                        List<string> parts = new List<string>(path.Split(char.Parse("\\")));
                        int index = parts.IndexOf("Resources");
                        if (index != -1) {
                            index++;
                            path = string.Join("/", parts.GetRange(index, parts.Count - index));
                        }
                        else {
                            throw new Exception("Path is not in a Resources folder, the pseudo parser only accepts paths that are in a resource folder for auto installer failsafe reasons.");
                        }

                        layerExport += "motion \"" + path + "\"\n";
                    }
                    if (state.state.speed != 1) {
                        layerExport += "speed " + state.state.speed + "\n";
                    }
                    if (state.state.speedParameterActive) {
                        layerExport += "multiplier " + state.state.speedParameter + "\n";
                    }
                    if (state.state.timeParameterActive) {
                        layerExport += "motiontime " + state.state.timeParameter + "\n";
                    }
                    if (state.state.mirror && !state.state.mirrorParameterActive) {
                        layerExport += "mirror true" + "\n";
                    }
                    if (state.state.mirrorParameterActive) {
                        layerExport += "mirror " + state.state.mirrorParameter + "\n";
                    }
                    if (state.state.cycleOffset != 0 && !state.state.cycleOffsetParameterActive) {
                        layerExport += "cycle " + state.state.cycleOffset + "\n";
                    }
                    if (state.state.cycleOffsetParameterActive) {
                        layerExport += "cycle " + state.state.cycleOffsetParameter + "\n";
                    }
                    if (state.state.iKOnFeet) {
                        layerExport += "footIK " + state.state.iKOnFeet + "\n";
                    }
                    if (!state.state.writeDefaultValues) {
                        layerExport += "writedefaults " + state.state.writeDefaultValues + "\n";
                    }

                    foreach(StateMachineBehaviour behaviour in state.state.behaviours) {
#if VRC_SDK_VRCSDK3
                        VRCAvatarParameterDriver driver = behaviour as VRCAvatarParameterDriver;
                        if(driver != null) {
                            layerExport += "vrcparameterdriver\n";
                            if (driver.localOnly) layerExport += "localonly\n";
                            foreach (VRCAvatarParameterDriver.Parameter parameter in driver.parameters) {
                                AnimatorControllerParameterType type = Context.GetParameterType(parameter.name);
                                if (type == AnimatorControllerParameterType.Int) {
                                    if (parameter.type == VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set) {
                                        layerExport += "set " + parameter.name + " " + int.Parse(parameter.value.ToString()) + "\n";
                                    }
                                    else if (parameter.type == VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Add) {
                                        layerExport += "add " + parameter.name + " " + int.Parse(parameter.value.ToString()) + "\n";
                                    }
                                    else if (parameter.type == VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Random) {
                                        layerExport += "random " + parameter.name + " " + int.Parse(parameter.valueMin.ToString()) + " " + int.Parse(parameter.valueMax.ToString()) + "\n";
                                    }
                                }
                                else if (type == AnimatorControllerParameterType.Float) {
                                    if (parameter.type == VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set) {
                                        layerExport += "set " + parameter.name + " " + parameter.value.ToString(".0######") + "\n";
                                    }
                                    else if (parameter.type == VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Add) {
                                        layerExport += "add " + parameter.name + " " + parameter.value.ToString(".0######") + "\n";
                                    }
                                    else if (parameter.type == VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Random) {
                                        layerExport += "random " + parameter.name + " " + parameter.valueMin.ToString(".0######") + " " + parameter.valueMax.ToString(".0######") + "\n";
                                    }
                                }
                                else if(type == AnimatorControllerParameterType.Bool) {
                                    if(parameter.type == VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set) {
                                        layerExport += "set " + parameter.name + " " + (parameter.value == 1 ? "true" : "false") + "\n";
                                    }
                                    else if(parameter.type == VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Random) {
                                        layerExport += "random " + parameter.name + " " + (parameter.chance) + "\n";
                                    }
                                }
                            }
                        }
#endif
                    }
                    foreach (var transition in state.state.transitions) {
                        ProcessTransition(state.state.name, transition, transitionSets);
                    }
                    layerExport += "\n";
                }

                foreach (var transition in layerContext.layer.stateMachine.anyStateTransitions) {
                    ProcessTransition("Any", transition, transitionSets);
                }

                foreach (TransitionData data in transitionSets) {
                    layerExport += "transition " + StringUtils.parseQuotes(data.start) + " to " + StringUtils.parseQuotes(data.end.name);
                    string conditions = "";
                    bool firstTransition = true;
                    foreach (AnimatorStateTransition transition in data.transitions) {
                        if (!firstTransition) conditions += " or ";
                        bool firstCondition = true;
                        foreach(AnimatorCondition condition in transition.conditions) {
                            if (!firstCondition) conditions += " and ";
                            conditions += condition.parameter + " " + StringUtils.parseComparator(condition.mode);
                            if (!(condition.mode == AnimatorConditionMode.If || condition.mode == AnimatorConditionMode.IfNot)) {
                                conditions += " " + condition.threshold;
                            }
                            firstCondition = false;
                        }
                        firstTransition = false;
                    }
                    if(conditions != "")layerExport += " when " + conditions;
                    layerExport += "\n";
                    if(data.hasExitTime) layerExport += "exittime " + data.exitTime + "\n";
                    if(data.duration != 0)layerExport += "duration " + data.duration + (data.hasFixedDuration? " fixed\n":"\n");
                    if(data.offset != 0)layerExport += "offset " + data.offset + "\n";
                    if(data.interruptionSource != TransitionInterruptionSource.None) {
                        if (data.interruptionSource == TransitionInterruptionSource.Destination) layerExport += "interruption next";
                        else if (data.interruptionSource == TransitionInterruptionSource.Source) layerExport += "interruption current";
                        else if (data.interruptionSource == TransitionInterruptionSource.DestinationThenSource) layerExport += "interruption nextthencurrent";
                        else if (data.interruptionSource == TransitionInterruptionSource.SourceThenDestination) layerExport += "interruption currentthennext";
                        if (data.orderedInterruption) layerExport += " ordered";
                        layerExport += "\n";
                        if (data.canTransitionToSelf) layerExport += "cantransitiontoself\n";
                        
                    }
                    layerExport += "\n";   
                }
                export += layerExport;
            }
            FileHelper.CreateNewTextFile(animatorController, animatorController.name + "_Export.txt", export, true);

        }

        private static void ProcessTransition(string startName, AnimatorStateTransition transition, List<TransitionData> transitionSets) {
            bool found = false;
            foreach (TransitionData data in transitionSets) {
                if (data.start != startName) continue;
                else if (data.end != transition.destinationState) continue;
                else if (data.hasExitTime != transition.hasExitTime) continue;
                else if (data.exitTime != transition.exitTime) continue;
                else if (data.hasFixedDuration != transition.hasFixedDuration) continue;
                else if (data.duration != transition.duration) continue;
                else if (data.interruptionSource != transition.interruptionSource) continue;
                else if (data.orderedInterruption != transition.orderedInterruption) continue;
                else if (data.offset != transition.offset) continue;
                else if (data.canTransitionToSelf != transition.canTransitionToSelf) continue;
                else {
                    data.transitions.Add(transition);
                    found = true;
                    break;
                }
            }
            if (!found) {
                TransitionData data = new TransitionData();
                data.start = startName;
                data.end = transition.destinationState;
                data.hasExitTime = transition.hasExitTime;
                data.exitTime = transition.exitTime;
                data.hasFixedDuration = transition.hasFixedDuration;
                data.duration = transition.duration;
                data.interruptionSource = transition.interruptionSource;
                data.orderedInterruption = transition.orderedInterruption;
                data.offset = transition.offset;
                data.canTransitionToSelf = transition.canTransitionToSelf;
                data.transitions = new List<AnimatorStateTransition>();
                data.transitions.Add(transition);
                transitionSets.Add(data);
            }
        }

        private struct TransitionData {
            public bool isDefault;
            public string start;
            public AnimatorState end;
            public bool hasExitTime;
            public float exitTime;
            public bool hasFixedDuration;
            public float duration;
            public float offset;
            public TransitionInterruptionSource interruptionSource;
            public bool orderedInterruption;
            public bool canTransitionToSelf;
            public List<AnimatorStateTransition> transitions;
        }
    }
}