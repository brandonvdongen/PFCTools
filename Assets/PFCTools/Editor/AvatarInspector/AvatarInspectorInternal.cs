using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using PFCTools.Utils;

namespace PFCTools.AvatarInspector {

    public class AvatarInspectorInternal {

        ItemTree<Component> _ComponentTree;
        Dictionary<Type, bool> DisplayableTypes = new Dictionary<Type, bool>();
        public string search = "";
        private GameObject _target = null;
        private bool showSettings = false;
        private List<Type> OrderedTypes = new List<Type>();
        private bool groupByParent;
        private Vector2 scrollPos;
        private Vector2 typeScrollPos;

        private readonly Dictionary<Component, ComponentEditorData> Editors = new Dictionary<Component, ComponentEditorData>();
        private readonly Dictionary<string, bool> GroupingBools = new Dictionary<string, bool>();

        private void UpdateCache() {
            DisplayableTypes.Clear();
            GroupingBools.Clear();
        }

        private void onChanged() {
            OrderedTypes = new List<Type>(DisplayableTypes.Keys);
            OrderedTypes.Sort((x, y) => string.Compare(y.Name, x.Name));
        }

        public AvatarInspectorInternal(ItemTree<Component> componentTree) {
            this._ComponentTree = componentTree;

            _ComponentTree.onClear += UpdateCache;
            _ComponentTree.onAddNode += onAddNode;
            _ComponentTree.onChanged += onChanged;

            Debug.Log("AvatarInspector Internal Loaded");
        }

        private ItemTreeNode<Component> onAddNode(ItemTreeNode<Component> Node) {
            if (!DisplayableTypes.ContainsKey(Node.Value.GetType())) {
                if (Node.Value is Transform) { DisplayableTypes.Add(Node.Value.GetType(), false); }
                else DisplayableTypes.Add(Node.Value.GetType(), true);
            }

            if (!Editors.ContainsKey(Node.Value)) {
                Editors.Add(Node.Value, new ComponentEditorData(Node.Value));
            }

            if (!GroupingBools.ContainsKey(Node.Value.name)) {
                GroupingBools.Add(Node.Value.name, false);
            }

            return Node;
        }

        public void drawAvatarInspector() {

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            showSettings = GUILayout.Toggle(showSettings, EditorGUIUtility.IconContent("_Popup"), EditorStyles.toolbarButton, GUILayout.MaxWidth(30));
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            search = EditorGUILayout.TextField("", search, GUI.skin.FindStyle("ToolbarSeachTextField"));
            if (EditorGUI.EndChangeCheck()) {
                if (string.IsNullOrWhiteSpace(search)) {
                    search = "";
                }
            }
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton"))) {
                GUI.FocusControl(null);
                search = "";
            }
            GUILayout.EndHorizontal();

            if (showSettings) {
                if (OrderedTypes.Count > 0) {
                    GUILayout.Label("Displayed types:");
                    typeScrollPos = EditorGUILayout.BeginScrollView(typeScrollPos, GUILayout.MinHeight(100), GUILayout.MaxHeight(200));
                    foreach (var type in OrderedTypes) {
                        EditorGUILayout.BeginHorizontal();
                        GUIContent ObjectData = EditorGUIUtility.ObjectContent(null, type);
                        GUILayout.Label(ObjectData.image, GUILayout.MaxHeight(20), GUILayout.MaxWidth(20));
                        DisplayableTypes[type] = GUILayout.Toggle(DisplayableTypes[type], type.Name);
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();
                    PFCGUI.HorizontalLine();
                }
                GUILayout.Label("Settings:");
                groupByParent = GUILayout.Toggle(groupByParent, "Group by parent");
                PFCGUI.HorizontalLine();
            }

            if (_ComponentTree == null) return;
            if (_ComponentTree.Nodes.Count <= 0) return;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            Transform lastTransform = null;
            foreach (var Node in _ComponentTree.Nodes.Values) {
                Component component = Node.Value;
                if (Node.Value == null) continue;
                if (search != "") {
                    if (!Node.Value.name.ToLower().Contains(search.ToLower()) && !Node.Value.GetType().ToString().ToLower().Contains(search.ToLower())) { continue; }
                }
                if (!DisplayableTypes.ContainsKey(component.GetType())) continue;
                if (DisplayableTypes[component.GetType()] == false) continue;
                if (groupByParent) {
                    if (lastTransform != component.transform) {
                        lastTransform = component.transform;
                        GUILayout.BeginHorizontal(EditorStyles.toolbar);
                        GroupingBools[component.transform.name] = EditorGUILayout.Foldout(GroupingBools[component.transform.name], component.name, true, EditorStyles.toolbarDropDown);
                        if (GUILayout.Button("Select", EditorStyles.toolbarButton, GUILayout.MaxWidth(100))) {
                            Selection.activeObject = component.transform;
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                if (GroupingBools[component.transform.name] || !groupByParent) {
                    GUILayout.Space(-2);
                    if (!groupByParent) EditorGUILayout.BeginHorizontal();
                    Editors[component].shown = EditorGUILayout.InspectorTitlebar(Editors[component].shown, component);
                    if (!groupByParent) {
                        if (GUILayout.Button(PrefabUtility.GetIconForGameObject(component.gameObject), GUILayout.MaxWidth(25), GUILayout.MaxHeight(25))) {
                            Selection.activeObject = component.transform;
                        }
                        EditorGUILayout.EndHorizontal();
                    }


                    if (Editors[component].shown) Editors[component].editor.OnInspectorGUI();
                    if (groupByParent) GUILayout.Space(5);
                }
            }
            PFCGUI.HorizontalLine();
            EditorGUILayout.EndScrollView();
        }
    }
}