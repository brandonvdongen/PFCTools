using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PFCTools.EditorTools {

    [InitializeOnLoad]
    public class AvatarInspectorWindow : EditorWindow {

        AvatarCache Cache = new AvatarCache();

        Vector2 scrollPos;
        string search = "search";
        bool showAvatarSelector = false;

        bool autoRefresh = true;

        GameObject[] knownAvatars;

        static GameObject _target = null;
        [SerializeField] public GameObject Target { get { return _target; } set { _target = value; onAvatarChange(); } }

        readonly Dictionary<Component, ComponentEditorData> Editors = new Dictionary<Component, ComponentEditorData>();
        readonly Dictionary<GameObject, bool> GroupingBools = new Dictionary<GameObject, bool>();


        [MenuItem("PFCTools/Avatar Inspector")]

        public static EditorWindow ShowWindow() {
            EditorWindow window = EditorWindow.GetWindow(typeof(AvatarInspectorWindow), false, "Avatar Inspector");
            window.minSize = new Vector2(200, 50);
            return window;
        }

        private void OnGUI() {

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            autoRefresh = GUILayout.Toggle(autoRefresh,"Auto-Refresh",EditorStyles.toolbarButton);
            if (!autoRefresh && GUILayout.Button("Refresh",EditorStyles.toolbarButton)) {
                Cache.updateCache();
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
                    EditorGUILayout.BeginHorizontal();
                    //GUILayout.FlexibleSpace();
                    if (GUILayout.Button(avatar.name, EditorStyles.miniButton)) {
                        Target = avatar;
                        showAvatarSelector = false;
                        /*UPDATE MATERIAL*/
                    }
                    EditorGUILayout.EndHorizontal();
                }
                UIUtils.HorizontalLine();
            }
            else {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Target:");
                GameObject newTarget = (GameObject)EditorGUILayout.ObjectField("", Target, typeof(GameObject), true, GUILayout.MinWidth(100));
                if (newTarget != Target) Target = newTarget;
                EditorGUILayout.EndHorizontal();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            Transform lastParent = null;
            foreach (var component in Cache.AllComponents) {

                if (lastParent != component.transform) {
                    lastParent = component.transform;
                    GUILayout.BeginHorizontal(EditorStyles.toolbar);
                    GroupingBools[component.gameObject] = EditorGUILayout.Foldout(GroupingBools[component.gameObject],component.gameObject.name,true,EditorStyles.toolbarDropDown);
                    if(GUILayout.Button("Select", EditorStyles.toolbarButton,GUILayout.MaxWidth(100))) {
                        Selection.activeObject = component.gameObject;
                    }
                    GUILayout.EndHorizontal();
                }
                if (GroupingBools[component.gameObject]) {
                    GUILayout.Space(-2);
                    Editors[component].shown = EditorGUILayout.InspectorTitlebar(Editors[component].shown, component);
                    if (Editors[component].shown) Editors[component].editor.OnInspectorGUI();
                    GUILayout.Space(5);
                }
                

            }
            EditorGUILayout.EndScrollView();
        }

        public void onAvatarChange() {
            Cache.CacheAvatar(Target);
            Editors.Clear();
            GroupingBools.Clear();
            GameObject gameObject = null;
            foreach (var component in Cache.AllComponents) {
                if (gameObject != component.gameObject) {
                    gameObject = component.gameObject;
                    GroupingBools.Add(gameObject, false);
                }
                if (!Editors.ContainsKey(component)) {
                    Editors.Add(component, new ComponentEditorData(component));
                };

            }
        }

        public void OnHierarchyChange() {
            Cache.updateCache();
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