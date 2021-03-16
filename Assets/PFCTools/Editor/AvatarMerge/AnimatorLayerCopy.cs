using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.AnimatedValues;

#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.ScriptableObjects;
#endif


namespace PFCTools.Utils {

    [InitializeOnLoad]
    public class AnimatorLayerCopy : EditorWindow {

        AnimatorLayerCopy instance;
        [SerializeField] public static AnimatorController AnimatorControllerSource;
        [SerializeField] public static AnimatorController AnimatorControllerTarget;
#if VRC_SDK_VRCSDK3
        [SerializeField] public static VRCExpressionParameters ExpressionSource;
        [SerializeField] public static VRCExpressionParameters ExpressionTarget;
        [SerializeField] public static VRCExpressionsMenu MenuSource;
        [SerializeField] public static VRCExpressionsMenu MenuTarget;
#endif
        public static AnimBool ShowAnimatorMergeTools;
        public static AnimBool ShowExpressionMergeTools;
        public static AnimBool ShowMenuMergeTools;

        public static bool ShowParameterMatches;
        public static bool ShowLayerMatches;

        Vector2 scrollPos;

        GUIStyle style;


        /*static AnimatorLayerCopy() {
            List<string> symbols = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';'));
            if (!symbols.Contains("PFCTOOLS")) {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, String.Join(";", symbols) + ";PFCTOOLS");
            }
        }*/


        private void OnEnable() {
            ShowAnimatorMergeTools = new AnimBool(false);
            ShowAnimatorMergeTools.valueChanged.AddListener(Repaint);
            ShowExpressionMergeTools = new AnimBool(false);
            ShowExpressionMergeTools.valueChanged.AddListener(Repaint);
            ShowMenuMergeTools = new AnimBool(false);
            ShowMenuMergeTools.valueChanged.AddListener(Repaint);

        }


        [MenuItem("PFCTools/Animator merge tools")]
        public static EditorWindow ShowWindow() {
            return EditorWindow.GetWindow(typeof(AnimatorLayerCopy), true, "Animator transfer tool");
        }
        private void OnGUI() {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
                ShowAnimatorMergeTools.target = GUILayout.Toggle(ShowAnimatorMergeTools.target, "Animator layer transfer tool", EditorStyles.toolbarButton);
            EditorGUILayout.BeginFadeGroup(ShowAnimatorMergeTools.faded);
            if (ShowAnimatorMergeTools.faded > 0) {
                AnimatorControllerSource = (AnimatorController)EditorGUILayout.ObjectField("Source Controller", AnimatorControllerSource, typeof(AnimatorController), false);
                AnimatorControllerTarget = (AnimatorController)EditorGUILayout.ObjectField("Target Controller", AnimatorControllerTarget, typeof(AnimatorController), false);

                PFCGUI.HorizontalLine();

                if (AnimatorControllerSource == null) {
                    EditorGUILayout.LabelField("No Source Animator.");

                }
                if (AnimatorControllerTarget == null) {
                    EditorGUILayout.LabelField("No Target Animator.");
                }
                if (AnimatorControllerSource != null && AnimatorControllerTarget != null) {
                    GUILayout.Label("(Parameters are automatically transfered as needed)");
                    ShowLayerMatches = EditorGUILayout.Foldout(ShowLayerMatches, "Copy Layers", true);
                    if (ShowLayerMatches) {
                        Dictionary<string, AnimatorControllerLayer> Difference = new Dictionary<string, AnimatorControllerLayer>();
                        foreach (AnimatorControllerLayer layer in AnimatorControllerSource.layers) {
                            Difference.Add(layer.name, layer);
                        }
                        foreach (AnimatorControllerLayer layer in AnimatorControllerTarget.layers) {
                            if (Difference.ContainsKey(layer.name)) {
                                Difference.Remove(layer.name);
                            }
                        }
                        if (Difference.Count > 1) {
                            if (GUILayout.Button("Copy All >>", GUILayout.Width(150))) {
                                foreach (KeyValuePair<string, AnimatorControllerLayer> label in Difference) {
                                    addLayerToTarget(label.Key, label.Value);
                                }
                            }
                        }
                        if (Difference.Count > 0) {
                            foreach (KeyValuePair<string, AnimatorControllerLayer> label in Difference) {
                                GUILayout.BeginHorizontal();
                                if (GUILayout.Button("Copy", GUILayout.Width(80))) {
                                    addLayerToTarget(label.Key, label.Value);

                                }
                                GUILayout.Label(label.Key);
                                GUILayout.EndHorizontal();

                            }
                        }
                        else {
                            GUILayout.Label("Full Match.");
                        }
                    }


                    ShowParameterMatches = EditorGUILayout.Foldout(ShowParameterMatches, "Copy Parameters", true);
                    if (ShowParameterMatches) {
                        Dictionary<string, AnimatorControllerParameterType> Difference = new Dictionary<string, AnimatorControllerParameterType>();
                        foreach (AnimatorControllerParameter param in AnimatorControllerSource.parameters) {
                            Difference.Add(param.name, param.type);
                        }
                        foreach (AnimatorControllerParameter param in AnimatorControllerTarget.parameters) {
                            if (Difference.ContainsKey(param.name)) {
                                Difference.Remove(param.name);
                            }
                        }
                        if (Difference.Count > 1) {
                            if (GUILayout.Button("Copy All >>", GUILayout.Width(150))) {
                                foreach (KeyValuePair<string, AnimatorControllerParameterType> label in Difference) {
                                    addParameterToTarget(label.Key, label.Value);
                                }
                            }
                        }
                        if (Difference.Count > 0) {
                            foreach (KeyValuePair<string, AnimatorControllerParameterType> label in Difference) {
                                GUILayout.BeginHorizontal();
                                if (GUILayout.Button("Copy", GUILayout.Width(80))) {
                                    addParameterToTarget(label.Key, label.Value);

                                }
                                GUILayout.Label(label.Key);
                                GUILayout.EndHorizontal();

                            }
                        }
                        else {
                            GUILayout.Label("Full Match.");
                        }
                    }
                }
            }
            EditorGUILayout.EndFadeGroup();

#if VRC_SDK_VRCSDK3
            
                ShowExpressionMergeTools.target = GUILayout.Toggle(ShowExpressionMergeTools.target, "Parameter transfer tools", EditorStyles.toolbarButton);

            if (ShowExpressionMergeTools.faded > 0) {
                EditorGUILayout.BeginFadeGroup(ShowExpressionMergeTools.faded);
                ExpressionSource = (VRCExpressionParameters)EditorGUILayout.ObjectField("Source", ExpressionSource, typeof(VRCExpressionParameters), false);
                ExpressionTarget = (VRCExpressionParameters)EditorGUILayout.ObjectField("Target", ExpressionTarget, typeof(VRCExpressionParameters), false);

                PFCGUI.HorizontalLine();

                if (ExpressionSource == null) {
                    EditorGUILayout.LabelField("No Source ExpressionParameters.");

                }
                if (ExpressionTarget == null) {
                    EditorGUILayout.LabelField("No Target ExpressionParameters.");
                }
                if (ExpressionSource != null && ExpressionTarget != null) {
                    int slots = 16;
                    int lastEmpty = -1;
                    int index = 0;
                    GUILayout.Label("Target parameters:");
                    foreach (VRCExpressionParameters.Parameter parameter in ExpressionTarget.parameters) {
                        if (String.IsNullOrWhiteSpace(parameter.name)) {
                            slots--;
                            lastEmpty = index;
                        }
                        else {
                            EditorGUILayout.BeginHorizontal();
                            if (ExpressionSource.FindParameter(parameter.name) != null) {
                                if (GUILayout.Button("Remove", GUILayout.Width(80))) {
                                    VRCExpressionParameters.Parameter newParam = new VRCExpressionParameters.Parameter();
                                    ExpressionTarget.parameters[index] = newParam;
                                }
                            }
                            else {
                                GUILayout.Button("-", GUILayout.Width(80));
                            }
                            GUILayout.Label(parameter.name);
                            EditorGUILayout.EndHorizontal();
                        }
                        index++;
                    }
                    if (slots < 16) {
                        foreach (VRCExpressionParameters.Parameter parameter in ExpressionSource.parameters) {
                            if (ExpressionTarget.FindParameter(parameter.name) == null) {
                                GUILayout.BeginHorizontal();
                                if (GUILayout.Button("Add", GUILayout.Width(80))) {
                                    ExpressionTarget.parameters[lastEmpty] = parameter;
                                }
                                GUILayout.Label(parameter.name);
                                GUILayout.EndHorizontal();

                            }
                        }
                    }
                    GUILayout.Label("Target Expressions Used (" + slots + "/16)");

                }
                EditorGUILayout.EndFadeGroup();
            }

            ShowMenuMergeTools.target = GUILayout.Toggle(ShowMenuMergeTools.target,"Menu transfer tool", EditorStyles.toolbarButton);


            if (ShowMenuMergeTools.faded > 0) {
                EditorGUILayout.BeginFadeGroup(ShowMenuMergeTools.faded);
                MenuSource = (VRCExpressionsMenu)EditorGUILayout.ObjectField("Source", MenuSource, typeof(VRCExpressionsMenu), false);
                MenuTarget = (VRCExpressionsMenu)EditorGUILayout.ObjectField("Target", MenuTarget, typeof(VRCExpressionsMenu), false);

                PFCGUI.HorizontalLine();

                if (MenuSource == null) {
                    EditorGUILayout.LabelField("No Source ExpressionMenu.");

                }
                if (MenuTarget == null) {
                    EditorGUILayout.LabelField("No Target ExpressionMenu.");
                }
                if (MenuSource != null && MenuTarget != null) {

                    GUILayout.Label("Source Controls:");
                    foreach (VRCExpressionsMenu.Control control in MenuSource.controls) {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Add", GUILayout.Width(80))) {
                            VRCExpressionsMenu.Control newControl = new VRCExpressionsMenu.Control();
                            newControl.icon = control.icon;
                            newControl.labels = control.labels;
                            newControl.name = control.name;
                            newControl.parameter = control.parameter;
                            newControl.style = control.style;
                            newControl.subMenu = control.subMenu;
                            newControl.subParameters = control.subParameters;
                            newControl.type = control.type;
                            newControl.value = control.value;

                            MenuTarget.controls.Add(newControl);
                        }
                        GUILayout.Label(control.name);

                        EditorGUILayout.EndHorizontal();

                    }

                    GUILayout.Label("Target Controls:");
                    foreach (VRCExpressionsMenu.Control control in MenuTarget.controls) {

                        if (!MenuSource.controls.Contains(control)) {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Button("-", GUILayout.Width(80));
                            GUILayout.Label(control.name);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.Label("Target Controls Used (" + MenuTarget.controls.Count + "/8)");

                }
                EditorGUILayout.EndFadeGroup();
            }
            EditorGUILayout.EndScrollView();
#endif
        }
        AnimatorControllerParameter getParameter(AnimatorController animator, string name) {
            foreach (AnimatorControllerParameter parameter in animator.parameters) {
                if (parameter.name == name) {
                    AnimatorControllerParameter newParameter = new AnimatorControllerParameter();
                    newParameter.name = parameter.name;
                    newParameter.type = parameter.type;
                    return newParameter;
                }
            }
            return null;
        }

        void ensureParameterExists(AnimatorController animator, string name) {
            if (getParameter(AnimatorControllerTarget, name) == null) {
                AnimatorControllerParameter parameter = getParameter(AnimatorControllerSource, name);
                if (parameter != null) {
                    AnimatorControllerTarget.AddParameter(parameter);
                }
            }
        }

        void addParameterToTarget(string name, AnimatorControllerParameterType type) {
            AnimatorControllerParameter param = new AnimatorControllerParameter();
            param.name = name;
            param.type = type;
            AnimatorControllerTarget.AddParameter(param);
        }
        void addLayerToTarget(string name, AnimatorControllerLayer copyLayer) {

            AnimatorControllerLayer newLayer = new AnimatorControllerLayer();
            AnimatorStateMachine newStateMachine = new AnimatorStateMachine();

            if (AssetDatabase.GetAssetPath(newStateMachine).Length == 0) {
                AssetDatabase.AddObjectToAsset(newStateMachine, AnimatorControllerTarget);
                newStateMachine.hideFlags = HideFlags.HideInHierarchy;
            }
            EditorUtility.SetDirty(newStateMachine);


            newLayer.name = copyLayer.name;
            newLayer.defaultWeight = copyLayer.defaultWeight;
            newLayer.stateMachine = newStateMachine;
            newLayer.stateMachine.anyStatePosition = copyLayer.stateMachine.anyStatePosition;
            newLayer.stateMachine.exitPosition = copyLayer.stateMachine.exitPosition;
            newLayer.stateMachine.entryPosition = copyLayer.stateMachine.entryPosition;

            Dictionary<int, StateData> states = new Dictionary<int, StateData>();

            foreach (ChildAnimatorState copyState in copyLayer.stateMachine.states) {
                StateData stateData = new StateData();

                AnimatorState newState = newStateMachine.AddState(copyState.state.name, copyState.position);

                if (AssetDatabase.GetAssetPath(newState).Length == 0) {
                    AssetDatabase.AddObjectToAsset(newState, newStateMachine);
                    newState.hideFlags = HideFlags.HideInHierarchy;
                }
                EditorUtility.SetDirty(newState);


                CopyFields<AnimatorState>(copyState.state, newState);

                if (!String.IsNullOrEmpty(copyState.state.cycleOffsetParameter)) { ensureParameterExists(AnimatorControllerTarget, copyState.state.cycleOffsetParameter); }
                if (!String.IsNullOrEmpty(copyState.state.mirrorParameter)) { ensureParameterExists(AnimatorControllerTarget, copyState.state.mirrorParameter); }
                if (!String.IsNullOrEmpty(copyState.state.speedParameter)) { ensureParameterExists(AnimatorControllerTarget, copyState.state.speedParameter); }
                if (!String.IsNullOrEmpty(copyState.state.timeParameter)) { ensureParameterExists(AnimatorControllerTarget, copyState.state.timeParameter); }


                if (copyState.state.motion is AnimationClip) {
                    newState.motion = copyState.state.motion;
                }
                if (copyState.state.motion is BlendTree) {
                    BlendTree copyTree = (BlendTree)copyState.state.motion;
                    BlendTree blendTree = new BlendTree();
                    CopyFields<BlendTree>(copyState.state.motion as BlendTree, blendTree);
                    blendTree.children = copyTree.children;
                    blendTree.name = copyTree.name;
                    blendTree.blendType = copyTree.blendType;
                    newState.motion = blendTree;
                    if (!String.IsNullOrEmpty(blendTree.blendParameter)) {
                        ensureParameterExists(AnimatorControllerTarget, blendTree.blendParameter);
                    }
                    if (!String.IsNullOrEmpty(blendTree.blendParameterY)) {
                        ensureParameterExists(AnimatorControllerTarget, blendTree.blendParameterY);
                    }

                    if (AssetDatabase.GetAssetPath(blendTree).Length == 0) {
                        AssetDatabase.AddObjectToAsset(blendTree, AnimatorControllerTarget);
                        blendTree.hideFlags = HideFlags.HideInHierarchy;
                    }
                    EditorUtility.SetDirty(blendTree);

                }


                stateData.state = newState;
                stateData.copyState = copyState.state;
                stateData.InstanceID = copyState.state.GetInstanceID();

                stateData.state.behaviours = copyState.state.behaviours;

                states.Add(stateData.InstanceID, stateData);

            }



            foreach (KeyValuePair<int, StateData> kvp in states) {
                StateData stateData = kvp.Value;

                foreach (AnimatorStateTransition copyTransition in stateData.copyState.transitions) {
                    AnimatorStateTransition transition = new AnimatorStateTransition();
                    if (AssetDatabase.GetAssetPath(transition).Length == 0) {
                        AssetDatabase.AddObjectToAsset(transition, stateData.state);
                        transition.hideFlags = HideFlags.HideInHierarchy;
                    }
                    EditorUtility.SetDirty(transition);


                    CopyFields<AnimatorStateTransition>(copyTransition, transition);

                    foreach (AnimatorCondition copyCondition in copyTransition.conditions) {
                        ensureParameterExists(AnimatorControllerTarget, copyCondition.parameter);
                        transition.AddCondition(copyCondition.mode, copyCondition.threshold, copyCondition.parameter);
                    }
        ;
                    if (!copyTransition.isExit) {
                        transition.destinationState = states[copyTransition.destinationState.GetInstanceID()].state;
                    }
                    else {
                        transition.isExit = true;
                    }
                    stateData.state.AddTransition(transition);

                }
            }
            foreach (AnimatorStateTransition copyTransition in copyLayer.stateMachine.anyStateTransitions) {
                if (copyTransition.destinationState != null) {
                    AnimatorStateTransition transition = newLayer.stateMachine.AddAnyStateTransition(states[copyTransition.destinationState.GetInstanceID()].state);

                    CopyFields<AnimatorStateTransition>(copyTransition, transition);

                    foreach (AnimatorCondition copyCondition in copyTransition.conditions) {
                        ensureParameterExists(AnimatorControllerTarget, copyCondition.parameter);
                        transition.AddCondition(copyCondition.mode, copyCondition.threshold, copyCondition.parameter);
                    }
                }
            }

            foreach (AnimatorTransition copyTransition in copyLayer.stateMachine.entryTransitions) {
                if (copyTransition.destinationState != null) {
                    AnimatorTransition transition = newLayer.stateMachine.AddEntryTransition(states[copyTransition.destinationState.GetInstanceID()].state);

                    CopyFields<AnimatorTransition>(copyTransition, transition);

                    foreach (AnimatorCondition copyCondition in copyTransition.conditions) {
                        ensureParameterExists(AnimatorControllerTarget, copyCondition.parameter);
                        transition.AddCondition(copyCondition.mode, copyCondition.threshold, copyCondition.parameter);
                    }
                }
            }

            newLayer.stateMachine.defaultState = states[copyLayer.stateMachine.defaultState.GetInstanceID()].state;

            AnimatorControllerTarget.AddLayer(newLayer);
            AssetDatabase.SaveAssets();
        }

        static void CopyFields<T>(T source, T target) {
            foreach (var property in typeof(T).GetProperties()) {
                if (property.PropertyType == typeof(int) ||
                    property.PropertyType == typeof(float) ||
                    property.PropertyType == typeof(string) ||
                    property.PropertyType == typeof(bool)) {
                    MethodInfo setMethod = property.GetSetMethod();
                    if (setMethod != null) {
                        property.SetValue(target, property.GetValue(source));
                    }
                }
            }
        }


    }

    struct StateData {
        public int InstanceID;
        public AnimatorState state;
        public AnimatorState copyState;
    }

}