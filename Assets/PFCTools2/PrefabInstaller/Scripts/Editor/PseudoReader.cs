using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Text.RegularExpressions;
using System.Linq;

namespace PFCTools2.Installer.Pseudo {
    public static class PseudoReader {

        public static void processFile(TextAsset file) {


            string filePath = AssetDatabase.GetAssetPath(file);
            string directory = Path.GetDirectoryName(filePath);
            Debug.Log(file.name);

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(directory + "/" + file.name + "_controller.controller");

            string[] lines = file.text.Split(char.Parse("\n"));

            Dictionary<string, AnimatorState> states = new Dictionary<string, AnimatorState>();
            Dictionary<string, AnimatorControllerParameter> parameters = new Dictionary<string, AnimatorControllerParameter>();
            foreach (AnimatorControllerParameter param in controller.parameters) {
                parameters.Add(param.name, param);
            }

            AnimatorStateMachine currentLayer = controller.layers[0].stateMachine;
            AnimatorStateTransition lastTransition = null;

            AnimatorState getState(string name) {
                if (states.ContainsKey(name)) {
                    return states[name];
                }
                else {
                    AnimatorState newState = currentLayer.AddState(name);
                    states.Add(name, newState);
                    return newState;
                }
            }

            foreach (string line in lines) {
                var parts = Regex.Matches(line, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

                if (parts.Count <= 0) continue;
                if (parts[0] == "layer") {
                    states = new Dictionary<string, AnimatorState>();
                    AnimatorControllerLayer newLayer = new AnimatorControllerLayer();
                    newLayer.stateMachine = new AnimatorStateMachine();
                    newLayer.name = parts[1];
                    newLayer.defaultWeight = 1;

                    controller.AddLayer(newLayer);

                    currentLayer = newLayer.stateMachine;

                }
                else if (parts[0] == "entry") {
                    AnimatorState state;
                    if (states.ContainsKey(parts[1])) state = states[parts[0]];
                    else {
                        state = currentLayer.AddState(parts[1]);
                        states.Add(parts[1], state);
                    }
                    currentLayer.defaultState = state;
                }
                else if (parts[0] == "goto") {
                    string startStateName = parts[1];
                    string endStateName = parts[3];
                    lastTransition = getState(string.Join(" ", startStateName)).AddTransition(getState(string.Join(" ", endStateName)));
                    lastTransition.duration = 0;

                }
                else if (parts[0] == "when") {
                    int i = 1;
                    while (i < parts.Count) {
                        AnimatorControllerParameterType paramType = AnimatorControllerParameterType.Bool;
                        if (parts[i] == "int") paramType = AnimatorControllerParameterType.Int;
                        if (parts[i] == "float") paramType = AnimatorControllerParameterType.Float;

                        string paramName = parts[i + 1];
                        string paramCondition = parts[i + 2];
                        string paramValue = parts[i + 3];

                        if (!parameters.ContainsKey(paramName)) {
                            controller.AddParameter(paramName, paramType);
                            parameters.Add(paramName, controller.parameters[controller.parameters.Length - 1]);
                        }

                        if (paramType == AnimatorControllerParameterType.Bool) {
                            if (paramCondition == "==") {
                                if (paramValue.ToLower() == "true") lastTransition.AddCondition(AnimatorConditionMode.If, 0, paramName);
                                if (paramValue.ToLower() == "false") lastTransition.AddCondition(AnimatorConditionMode.IfNot, 0, paramName);
                            }
                        }
                        else {
                            Debug.Log(paramValue);
                            AnimatorConditionMode conditionMode = AnimatorConditionMode.Greater;
                            if (paramCondition == "<") conditionMode = AnimatorConditionMode.Less;
                            if (paramCondition == "==") conditionMode = AnimatorConditionMode.Equals;
                            if (paramCondition == "!=") conditionMode = AnimatorConditionMode.NotEqual;
                            lastTransition.AddCondition(conditionMode, float.Parse(paramValue), paramName);
                        }
                        i += 5;
                    }
                }
                else if (parts[0] == "duration") {
                    if (parts[1] == "fixed") {
                        lastTransition.hasFixedDuration = true;
                        lastTransition.duration = float.Parse(parts[2]);
                    }
                    else {
                        lastTransition.duration = float.Parse(parts[1]);
                    }
                }
                else if (parts[0] == "exit") {
                    lastTransition.exitTime = float.Parse(parts[1]);
                    lastTransition.hasExitTime = true;
                }
            }
        }
    }
}