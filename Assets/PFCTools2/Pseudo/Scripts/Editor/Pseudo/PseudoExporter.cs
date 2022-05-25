using PFCTools2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace PFCTools2.Installer.PseudoParser
{
    public class PseudoExporter
    {
        internal static void Export(AnimatorController animatorController)
        {
            string export = "";
            ControllerContext Context = new ControllerContext(animatorController);

            foreach (KeyValuePair<string, AnimatorControllerParameter> data in Context.parameters)
            {
                export += "parameter " + data.Value.name;
                if (data.Value.type == AnimatorControllerParameterType.Float)
                {
                    export += " float";
                }

                if (data.Value.type == AnimatorControllerParameterType.Int)
                {
                    export += " int";
                }

                if (data.Value.type == AnimatorControllerParameterType.Bool)
                {
                    export += " bool";
                }

                if (data.Value.type == AnimatorControllerParameterType.Trigger)
                {
                    export += " trigger";
                }

                export += "\n";
            }
            export += "\n";

            foreach (AnimatorLayerContext layerContext in Context.layers)
            {
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

                foreach (ChildAnimatorState state in layerContext.layer.stateMachine.states)
                {
                    //Debug.Log(state.state.name);
                    layerExport += "state " + StringUtils.parseQuotes(state.state.name) + "\n";
                    if (layerContext.layer.stateMachine.defaultState == state.state)
                    {
                        layerExport += "default \n";
                    }
                    StateOffset offset = new StateOffset(state.position.x, state.position.y);
                    layerExport += "pos " + offset.reverse.x + " " + offset.reverse.y + "\n";
                    if (state.state.motion != null)
                    {
                        string path = AssetDatabase.GetAssetPath(state.state.motion);
                        path = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path);
                        List<string> parts = new List<string>(path.Split(char.Parse("\\")));
                        int index = parts.IndexOf("Resources");
                        if (index != -1)
                        {
                            index++;
                            path = string.Join("/", parts.GetRange(index, parts.Count - index));
                        }
                        else
                        {
                            throw new Exception("Path is not in a Resources folder, the pseudo parser only accepts paths that are in a resource folder for auto installer failsafe reasons.");
                        }

                        layerExport += "motion \"" + path + "\"\n";
                    }
                    if (state.state.speed != 1)
                    {
                        layerExport += "speed " + state.state.speed + "\n";
                    }
                    if (state.state.speedParameterActive)
                    {
                        layerExport += "multiplier " + state.state.speedParameter + "\n";
                    }
                    if (state.state.timeParameterActive)
                    {
                        layerExport += "motiontime " + state.state.timeParameter + "\n";
                    }
                    if (state.state.mirror && !state.state.mirrorParameterActive)
                    {
                        layerExport += "mirror true" + "\n";
                    }
                    if (state.state.mirrorParameterActive)
                    {
                        layerExport += "mirror " + state.state.mirrorParameter + "\n";
                    }
                    if (state.state.cycleOffset != 0 && !state.state.cycleOffsetParameterActive)
                    {
                        layerExport += "cycle " + state.state.cycleOffset + "\n";
                    }
                    if (state.state.cycleOffsetParameterActive)
                    {
                        layerExport += "cycle " + state.state.cycleOffsetParameter + "\n";
                    }
                    if (state.state.iKOnFeet)
                    {
                        layerExport += "footIK " + state.state.iKOnFeet + "\n";
                    }
                    if (!state.state.writeDefaultValues)
                    {
                        layerExport += "writedefaults " + state.state.writeDefaultValues + "\n";
                    }

                    foreach (StateMachineBehaviour behaviour in state.state.behaviours)
                    {
#if VRC_SDK_VRCSDK3
                        VRCAvatarParameterDriver driver = behaviour as VRCAvatarParameterDriver;
                        if (driver != null)
                        {
                            layerExport += "vrcparameterdriver\n";
                            if (driver.localOnly)
                            {
                                layerExport += "localonly\n";
                            }

                            foreach (VRCAvatarParameterDriver.Parameter parameter in driver.parameters)
                            {
                                AnimatorControllerParameterType type = Context.GetParameterType(parameter.name);
                                if (type == AnimatorControllerParameterType.Int)
                                {
                                    if (parameter.type == VRCAvatarParameterDriver.ChangeType.Set)
                                    {
                                        layerExport += "set " + parameter.name + " " + int.Parse(parameter.value.ToString()) + "\n";
                                    }
                                    else if (parameter.type == VRCAvatarParameterDriver.ChangeType.Add)
                                    {
                                        layerExport += "add " + parameter.name + " " + int.Parse(parameter.value.ToString()) + "\n";
                                    }
                                    else if (parameter.type == VRCAvatarParameterDriver.ChangeType.Random)
                                    {
                                        layerExport += "random " + parameter.name + " " + int.Parse(parameter.valueMin.ToString()) + " " + int.Parse(parameter.valueMax.ToString()) + "\n";
                                    }
                                }
                                else if (type == AnimatorControllerParameterType.Float)
                                {
                                    if (parameter.type == VRCAvatarParameterDriver.ChangeType.Set)
                                    {
                                        layerExport += "set " + parameter.name + " " + parameter.value.ToString(".0######") + "\n";
                                    }
                                    else if (parameter.type == VRCAvatarParameterDriver.ChangeType.Add)
                                    {
                                        layerExport += "add " + parameter.name + " " + parameter.value.ToString(".0######") + "\n";
                                    }
                                    else if (parameter.type == VRCAvatarParameterDriver.ChangeType.Random)
                                    {
                                        layerExport += "random " + parameter.name + " " + parameter.valueMin.ToString(".0######") + " " + parameter.valueMax.ToString(".0######") + "\n";
                                    }
                                }
                                else if (type == AnimatorControllerParameterType.Bool)
                                {
                                    if (parameter.type == VRCAvatarParameterDriver.ChangeType.Set)
                                    {
                                        layerExport += "set " + parameter.name + " " + (parameter.value == 1 ? "true" : "false") + "\n";
                                    }
                                    else if (parameter.type == VRCAvatarParameterDriver.ChangeType.Random)
                                    {
                                        layerExport += "random " + parameter.name + " " + (parameter.chance) + "\n";
                                    }
                                }
                                else if (type == AnimatorControllerParameterType.Trigger)
                                {
                                    if (parameter.type == VRCAvatarParameterDriver.ChangeType.Set)
                                    {
                                        layerExport += "set " + parameter.name + "\n";
                                    }
                                    else if (parameter.type == VRCAvatarParameterDriver.ChangeType.Random)
                                    {
                                        layerExport += "random " + parameter.name + " " + (parameter.chance) + "\n";
                                    }
                                }
                            }
                        }
#endif
                    }
                    foreach (AnimatorStateTransition transition in state.state.transitions)
                    {
                        ProcessTransition(state.state.name, transition, transitionSets);
                    }
                    layerExport += "\n";
                }

                foreach (AnimatorStateTransition transition in layerContext.layer.stateMachine.anyStateTransitions)
                {
                    ProcessTransition("Any", transition, transitionSets);
                }

                foreach (TransitionData data in transitionSets)
                {
                    layerExport += "transition " + StringUtils.parseQuotes(data.start) + " to " + StringUtils.parseQuotes(data.end.name);
                    string conditions = "";
                    bool firstTransition = true;
                    foreach (AnimatorStateTransition transition in data.transitions)
                    {
                        if (!firstTransition)
                        {
                            conditions += " or ";
                        }

                        bool firstCondition = true;
                        foreach (AnimatorCondition condition in transition.conditions)
                        {
                            if (!firstCondition)
                            {
                                conditions += " and ";
                            }

                            conditions += condition.parameter + " " + StringUtils.parseComparator(condition.mode);
                            if (!(condition.mode == AnimatorConditionMode.If || condition.mode == AnimatorConditionMode.IfNot))
                            {
                                conditions += " " + condition.threshold;
                            }
                            firstCondition = false;
                        }
                        firstTransition = false;
                    }
                    if (conditions != "")
                    {
                        layerExport += " when " + conditions;
                    }

                    layerExport += "\n";
                    if (data.hasExitTime)
                    {
                        layerExport += "exittime " + data.exitTime + "\n";
                    }

                    if (data.duration != 0)
                    {
                        layerExport += "duration " + data.duration + (data.hasFixedDuration ? " fixed\n" : "\n");
                    }

                    if (data.offset != 0)
                    {
                        layerExport += "offset " + data.offset + "\n";
                    }

                    if (data.interruptionSource != TransitionInterruptionSource.None)
                    {
                        if (data.interruptionSource == TransitionInterruptionSource.Destination)
                        {
                            layerExport += "interruption next";
                        }
                        else if (data.interruptionSource == TransitionInterruptionSource.Source)
                        {
                            layerExport += "interruption current";
                        }
                        else if (data.interruptionSource == TransitionInterruptionSource.DestinationThenSource)
                        {
                            layerExport += "interruption nextthencurrent";
                        }
                        else if (data.interruptionSource == TransitionInterruptionSource.SourceThenDestination)
                        {
                            layerExport += "interruption currentthennext";
                        }

                        if (data.orderedInterruption)
                        {
                            layerExport += " ordered";
                        }

                        layerExport += "\n";
                        if (data.canTransitionToSelf)
                        {
                            layerExport += "cantransitiontoself\n";
                        }
                    }
                    layerExport += "\n";
                }
                export += layerExport;
            }
            FileHelper.CreateNewTextFile(animatorController, animatorController.name + "_Pseudo.txt", export, true);

        }
        private static void ProcessTransition(string startName, AnimatorStateTransition transition, List<TransitionData> transitionSets)
        {
            bool found = false;
            foreach (TransitionData data in transitionSets)
            {
                if (data.start != startName)
                {
                    continue;
                }
                else if (data.end != transition.destinationState)
                {
                    continue;
                }
                else if (data.hasExitTime != transition.hasExitTime)
                {
                    continue;
                }
                else if (data.exitTime != transition.exitTime)
                {
                    continue;
                }
                else if (data.hasFixedDuration != transition.hasFixedDuration)
                {
                    continue;
                }
                else if (data.duration != transition.duration)
                {
                    continue;
                }
                else if (data.interruptionSource != transition.interruptionSource)
                {
                    continue;
                }
                else if (data.orderedInterruption != transition.orderedInterruption)
                {
                    continue;
                }
                else if (data.offset != transition.offset)
                {
                    continue;
                }
                else if (data.canTransitionToSelf != transition.canTransitionToSelf)
                {
                    continue;
                }
                else
                {
                    data.transitions.Add(transition);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
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
    }
}