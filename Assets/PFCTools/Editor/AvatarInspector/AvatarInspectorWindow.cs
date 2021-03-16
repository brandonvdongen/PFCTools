using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PFCTools.Utils;
using System;
using UnityEditor.Experimental.UIElements;

namespace PFCTools.AvatarInspector {

    [InitializeOnLoad]
    public class AvatarInspectorWindow : EditorWindow {

        public delegate void CacheUpdateEvent();
        public static CacheUpdateEvent onCacheUpdate;

        public readonly ItemTree<Component> ComponentTree = new ItemTree<Component>();
        bool showAvatarSelector = false;
        bool autoRefresh = true;
        int toolbarSelection = 0;
        GameObject[] knownAvatars;
        static GameObject _target = null;
        public GameObject Target {
            get { return _target; }
            set {
                _target = value;
                UpdateCache();
            }
        }
        AvatarInspectorInternal avatarInspector;
        MaterialFinderInternal materialFinder;



        string[] Menus = new string[2] { "Inspector", "materialFinder" };

        private void OnEnable() {
            avatarInspector = new AvatarInspectorInternal(ComponentTree);
            materialFinder = new MaterialFinderInternal();
            ComponentTree.onAddNode += onAddNode;
            ComponentTree.onRemoveNode += onRemoveNode;
        }

        private void OnDisable() {
            avatarInspector = null;
            materialFinder = null;
            ComponentTree.onAddNode -= onAddNode;
            ComponentTree.onRemoveNode -= onRemoveNode;
        }



        [MenuItem("PFCTools/Avatar Inspector")]
        public static EditorWindow ShowWindow() {
            EditorWindow window = EditorWindow.GetWindow(typeof(AvatarInspectorWindow), false, "Avatar Inspector");
            window.minSize = new Vector2(200, 50);
            return window;
        }



        private void OnGUI() {

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            autoRefresh = GUILayout.Toggle(autoRefresh, "Auto-Refresh", EditorStyles.toolbarButton);
            if (!autoRefresh && GUILayout.Button("Refresh", EditorStyles.toolbarButton)) {
                ComponentTree.Clear();
            }
            GUILayout.FlexibleSpace();

            if (VRCSDK.installed) {
                if (GUILayout.Button("Select Avatar", EditorStyles.toolbarDropDown)) {
                    if (showAvatarSelector == false) {
                        knownAvatars = VRCSDK.GetAvatars();
                    }
                    if (knownAvatars.Length == 1) {
                        Target = knownAvatars[0];
                        showAvatarSelector = false;
                    }
                    else if (knownAvatars.Length == 0) {
                        showAvatarSelector = false;
                    }
                    else {
                        showAvatarSelector = !showAvatarSelector;
                    }
                }
            }
            if (GUILayout.Button("Select from scene", EditorStyles.toolbarButton)) {
                Target = Selection.activeGameObject;
            }
            EditorGUILayout.EndHorizontal();

            if (showAvatarSelector) {
                foreach (var avatar in knownAvatars) {
                    if (GUILayout.Button(avatar.name, EditorStyles.miniButton)) {
                        Target = avatar;
                        showAvatarSelector = false;
                    }
                }
                PFCGUI.HorizontalLine();
            }
            else {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Target:");
                GameObject newTarget = (GameObject)EditorGUILayout.ObjectField("", Target, typeof(GameObject), true, GUILayout.MinWidth(100));
                if (newTarget != Target) Target = newTarget;
                EditorGUILayout.EndHorizontal();
            }

            toolbarSelection = GUILayout.Toolbar(toolbarSelection, Menus);
            if (toolbarSelection == 0) {
                avatarInspector.drawAvatarInspector();
            }
            else if (toolbarSelection == 1) {
                materialFinder.drawMaterialFinder();
            }
        }

        private void UpdateCache() {

            ComponentTree.Clear();
            if (Target == null) return;
            ComponentTree.AddNodes(Target.GetComponentsInChildren<Component>());
        }

        private ItemTreeNode<Component> onAddNode(ItemTreeNode<Component> Node) {
            Transform Parent = Node.Value.transform.parent;
            if (Parent != null) {
                if (ComponentTree.Contains(Parent)) {
                    ComponentTree.AddChild(ComponentTree[Parent], Node);
                    Node.Parent = ComponentTree[Parent];
                    //Debug.Log(string.Format("Child Parent Connection made between {0}<>{1}", Parent.name, Node.Value.name));
                }
            }
            return Node;
        }
        private ItemTreeNode<Component> onRemoveNode(ItemTreeNode<Component> Node) {
            if (Node.Parent != null) {
                if (ComponentTree.Contains(Node.Parent)) {
                    ComponentTree.ClearParent(Node.Parent);
                }
            }
            if (Node.Children.Count > 1) {
                foreach (var ChildNode in Node.Children) {
                    ComponentTree.RemoveChild(Node.Parent, ChildNode);
                }
            }
            return Node;
        }

        public void OnHierarchyChange() {
            if (autoRefresh) {
                UpdateCache();
                Repaint();
            }
        }
    }

    class ComponentEditorData {

        public bool shown = false;
        public Editor editor;
        public ComponentEditorData(Component component) {
            editor = Editor.CreateEditor(component);
        }
    }
}