using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using PFCTools2.Utils;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

namespace PFCTools2.CleanupTools
{
    public class AssetCollectorMenu : EditorWindow
    {

        bool validSelection { get { return Selection.activeObject is GameObject; } }

        static BoolPreferenceHandler includeTextures = new BoolPreferenceHandler("Include Textures", "includeTextures", true);
        static BoolPreferenceHandler includeMaterials = new BoolPreferenceHandler("Include Materials", "includeMaterials", true);
        static BoolPreferenceHandler includeMeshes = new BoolPreferenceHandler("Include Meshes", "includeMeshes", true);
        static BoolPreferenceHandler includeSounds = new BoolPreferenceHandler("Include Sounds", "includeSounds", true);
        static BoolPreferenceHandler includeAnimations = new BoolPreferenceHandler("Include Animations", "includeAnimations", true);
        static BoolPreferenceHandler includeShaders = new BoolPreferenceHandler("Include Shaders (DO NOT USE, INCOMPLETE)", "includeShaders", false);
#if VRC_SDK_VRCSDK3
        static BoolPreferenceHandler includeVRCFiles = new BoolPreferenceHandler("Include VRC Files", "includeVRC");
#endif
        static BoolPreferenceHandler moveFiles = new BoolPreferenceHandler("Move Files", "moveFiles", true);
        static BoolPreferenceHandler makeMovementManifest = new BoolPreferenceHandler("Make Movement Manifest File", "makeManifest", false);

        Button btn_export;
        Label selectionLabel;

        [MenuItem("PFCTools2/Cleanup/Asset Collector")]
        public static void OpenWindow()
        {
            AssetCollectorMenu wnd = GetWindow<AssetCollectorMenu>();
            wnd.titleContent = new GUIContent("Asset Collector");
            wnd.minSize = new Vector2(321, 200);
            wnd.maxSize = new Vector2(321, 200);
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            ObjectField ob = new ObjectField("Target:") { objectType = typeof(GameObject) };

            root.Add(GetPreferences());


            btn_export = new Button() { text = "Collect All Assets" };
            btn_export.clicked += () =>
            {
                ExportGameObject();
            };

            selectionLabel = new Label(validSelection ? "Selection: " + Selection.activeObject.name : "No Selection");
            root.Add(selectionLabel);

            root.Add(btn_export);
            bool isGameObject = Selection.activeObject is GameObject;
            btn_export.SetEnabled(validSelection);

        }

        private void OnSelectionChange()
        {
            if (btn_export != null)
            {
                btn_export.SetEnabled(validSelection);
            }
            if (selectionLabel != null)
            {
                selectionLabel.text = validSelection ? "Selection: " + Selection.activeObject.name : "No Selection";
            }
        }

        private static VisualElement GetPreferences()
        {
            VisualElement root = new VisualElement();
            foreach (var setting in PreferenceHandler.Preferences.Values)
            {
                if (setting is BoolPreferenceHandler)
                {
                    BoolPreferenceHandler boolSetting = setting as BoolPreferenceHandler;
                    Toggle btn = new Toggle();
                    btn.text = boolSetting.name;
                    btn.value = boolSetting.IsEnabled;
                    btn.RegisterValueChangedCallback((e) =>
                    {
                        boolSetting.IsEnabled = e.newValue;
                    });
                    root.Add(btn);
                }
            }
            return root;
        }

        private class DirectoryTable
        {

            public string rootPath = "";
            private bool _allowVRCSDKFiles = false;
            private bool _VRCSDKChecked = false;

            public DirectoryTable(string rootPath)
            {
                this.rootPath = rootPath;
            }

            private Dictionary<string, string> _paths = new Dictionary<string, string>();
            public Dictionary<string, string> Paths { get => _paths; }
            public void Add(string orgPath, string destPath = "")
            {
                if (Path.GetDirectoryName(orgPath) == $"Assets/{rootPath}/{destPath}") return;
                if (orgPath.Contains("VRCSDK"))
                {
                    if (orgPath.Contains("proxy_")) return;
                    if (!_VRCSDKChecked)
                    {
                        _allowVRCSDKFiles = EditorUtility.DisplayDialog("Are you sure?", $"One or more the files in this model were found to be part of the VRCSDK, do you wish to move SDK files (in case they where modified) or leave them in the VRCSDK Folder?\n\n File:{orgPath}", "Move to avatar folder", "leave in VRCSDK folder");
                        _VRCSDKChecked = true;
                    }
                    if (!_allowVRCSDKFiles) return;

                }

                if (_paths.ContainsKey(orgPath))
                {
                    if (_paths[orgPath] == destPath)
                    {
                        string newPath = Path.GetDirectoryName(destPath);
                        if (rootPath != "") _paths[orgPath] = $"Assets/{rootPath}/{newPath}/Shared";
                        else _paths[orgPath] = $"Assets/{rootPath}/{newPath}/Shared";

                    }
                }
                else
                {
                    string path = "Assets";
                    if (rootPath.Length > 0) path += "/" + rootPath;
                    if (destPath.Length > 0) path += "/" + destPath;
                    _paths.Add(orgPath, path);
                }
            }
            public void Add(Object _object, string destPath = "")
            {
                string orgPath = AssetDatabase.GetAssetPath(_object);
                Add(orgPath, destPath);
            }

            public void Rem(string orgPath)
            {
                if (_paths.ContainsKey(orgPath))
                {
                    _paths.Remove(orgPath);
                }
            }
            public void Rem(Object _object)
            {
                string orgPath = AssetDatabase.GetAssetPath(_object);
                Rem(orgPath);
            }
        }

        static void ExportGameObject()
        {

            System.DateTime startTime = System.DateTime.Now;

            GameObject prefab = Selection.activeGameObject as GameObject;
            DirectoryTable directories = new DirectoryTable(prefab.name);
            bool cancel;
            cancel = EditorUtility.DisplayCancelableProgressBar("Fetching Avatar Files", "Processing Avatar.", 0);


            //See if the Selected object is part of a scene asset, if so make it the root.
            Object root = prefab;
            if (prefab.scene != null)
            {
                if (prefab.scene.path != "" && prefab.scene.path != null)
                {
                    directories.Add(prefab.scene.path, "");
                    root = AssetDatabase.LoadAssetAtPath<Object>(prefab.scene.path);
                }
                else
                {
                    directories.Add(prefab);
                    root = prefab;
                }
            }

            //Get Materials and Textures and meshes
            foreach (Renderer _renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (includeMaterials.cachedValue || includeTextures.cachedValue)
                {
                    foreach (Material _material in _renderer.sharedMaterials)
                    {
                        if (includeMaterials.cachedValue) directories.Add(_material, "Materials");
                        if (includeTextures.cachedValue) getTexturesFromMaterial(directories, _material);
                        if (includeShaders.cachedValue) getShaderFromMaterial(directories, _material);
                    }
                }
                if (includeMeshes.cachedValue)
                {
                    if (_renderer is SkinnedMeshRenderer)
                    {
                        SkinnedMeshRenderer smr = _renderer as SkinnedMeshRenderer;
                        directories.Add(smr.sharedMesh, "Models");
                    }
                    else if (_renderer is MeshRenderer)
                    {
                        MeshFilter filter = _renderer.gameObject.GetComponent<MeshFilter>();
                        if (filter != null)
                        {
                            directories.Add(filter.sharedMesh, "Models");

                        }
                    }
                    else if (_renderer is ParticleSystemRenderer)
                    {

                        ParticleSystemRenderer psr = _renderer as ParticleSystemRenderer;
                        Mesh[] meshes = new Mesh[0];

                        if (psr.mesh != null)
                        {
                            directories.Add(psr.mesh, "Models");
                        }
                        else if (psr.GetMeshes(meshes) > 0)
                        {
                            foreach (var mesh in meshes)
                            {
                                directories.Add(mesh, "Models");
                            }
                        }
                    }
                }
            }

            //Get Sounds
            if (includeSounds.cachedValue)
            {
                foreach (var a in prefab.GetComponentsInChildren<AudioSource>(true))
                {
                    directories.Add(a.clip, "Sound");
                }
            }

            //Get Main Prefabs
            int maxDepth = 100;
            GameObject parentPrefab = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
            while (parentPrefab != null && maxDepth > 0)
            {
                if (PrefabUtility.GetPrefabAssetType(parentPrefab) == PrefabAssetType.Model)
                {
                    directories.Add(parentPrefab, "Models");
                }
                else
                {
                    directories.Add(parentPrefab, "Prefabs");
                }

                parentPrefab = PrefabUtility.GetCorrespondingObjectFromSource(parentPrefab);
                maxDepth--;
            }

            if (includeAnimations.cachedValue)
            {
                //Hanlde Animator Controller
                Animator animator = prefab.GetComponent<Animator>();
                if (animator)
                {
                    AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
                    if (controller)
                    {
                        ProcessAnimatorController(directories, controller);
                    }
                }
            }
            //Find potential world constraints
            foreach (IConstraint constraint in prefab.GetComponentsInChildren<IConstraint>())
            {
                List<ConstraintSource> sources = new List<ConstraintSource>();
                constraint.GetSources(sources);

                foreach (var source in sources)
                {
                    if (source.sourceTransform.gameObject.scene.path == null)
                    {
                        directories.Add(source.sourceTransform.gameObject, "Extras");
                    }
                }
            }

            if (includeVRCFiles.cachedValue)
            {
                //VRCDescriptor Files
                VRCAvatarDescriptor descriptor = prefab.GetComponent<VRCAvatarDescriptor>();
                if (descriptor)
                {
                    foreach (var layer in descriptor.baseAnimationLayers)
                    {
                        ProcessAnimatorController(directories, layer.animatorController as AnimatorController);
                    }
                    foreach (var layer in descriptor.specialAnimationLayers)
                    {
                        ProcessAnimatorController(directories, layer.animatorController as AnimatorController);
                    }

                    ProcessExpressionsMenu(directories, descriptor.expressionsMenu);

                    if (descriptor.expressionParameters != null)
                    {
                        directories.Add(descriptor.expressionParameters, "VRC/Expressions");
                    }
                }
            }



            ///////////Process files//////////
            float i = 1;
            List<string> _directoryList = new List<string>();
            foreach (string value in directories.Paths.Values)
            {
                cancel = EditorUtility.DisplayCancelableProgressBar("Compiling folder structure", value, i / directories.Paths.Count);

                if (!_directoryList.Contains(value))
                {
                    _directoryList.Add(value);
                }
                i++;
            }

            if (moveFiles.cachedValue)
            {
                i = 1;
                foreach (string dir in _directoryList)
                {
                    cancel = EditorUtility.DisplayCancelableProgressBar("Creating Directories", dir, i / _directoryList.Count);
                    Directory.CreateDirectory($"{dir}");
                    i++;
                }
            }
            AssetDatabase.Refresh();

            List<string> lines = new List<string>();

            //Move Files
            i = 1;
            foreach (var path in directories.Paths.Keys)
            {
                EditorUtility.DisplayCancelableProgressBar("Moving Avatar Files", path, i / directories.Paths.Keys.Count);
                string fileName = Path.GetFileName(path);
                string destPath = $"{directories.Paths[path]}/{fileName}";
                if (moveFiles.cachedValue)
                {
                    AssetDatabase.MoveAsset(path, destPath);
                    if (path != destPath) lines.Add($"{lines.Count}: {path} >>> {destPath}");
                }
                else
                {
                    if(lines.Count == 0)lines.Add($"Root: {path}");
                    else lines.Add($"{lines.Count}: {path}");
                }
                i++;
            }
            if (makeMovementManifest.cachedValue && lines.Count > 0)
            {
                if (moveFiles.cachedValue)File.WriteAllLines(Path.GetDirectoryName(AssetDatabase.GetAssetPath(root)) + "/MoveManifest.txt", lines);
                else File.WriteAllLines("Assets/MoveManifest.txt", lines);
            }
            if (moveFiles.cachedValue)Selection.activeObject = root;
            EditorUtility.ClearProgressBar();
            System.TimeSpan Time = System.DateTime.Now.Subtract(startTime);
            Debug.Log($"Processed avatar: {prefab.name} Took {Time.TotalMinutes:0}:{Time.Seconds:00}Minutes");
            AssetDatabase.Refresh();

        }


        //////////////////// METHODS //////////////////////////

        private static void ProcessExpressionsMenu(DirectoryTable directories, VRCExpressionsMenu menu)
        {
            directories.Add(menu, "VRC/Menus");
            if (menu != null)
            {
                foreach (VRCExpressionsMenu.Control control in menu.controls)
                {
                    if (control.icon != null)
                    {
                        directories.Add(control.icon, $"Textures/MenuIcons/{menu.name}");
                    }
                    if (control.subMenu != null)
                    {
                        //Debug.Log(path);
                        directories.Add(control.subMenu, "VRC/Menus");
                        //if (path != "") path = path + "/" + control.subMenu.name;
                        //else path = control.subMenu.name;
                        ProcessExpressionsMenu(directories, control.subMenu);

                    }
                }
            }
        }

        private static void getShaderFromMaterial(DirectoryTable directories, Material _material)
        {
            directories.Add(_material.shader, "Shaders");
        }

        private static void getTexturesFromMaterial(DirectoryTable directories, Material _material)
        {
            //TODO MOVE THIS
            //directories.Add(_material.shader, "Shaders");
            foreach (string name in _material.GetTexturePropertyNames())
            {
                Texture _texture = _material.GetTexture(name);
                if (_texture != null)
                {
                    directories.Add(_texture, "Textures");
                }
            }
        }

        private static void ProcessAnimatorController(DirectoryTable directories, AnimatorController controller)
        {
            if (controller == null) return;
            directories.Add(controller, "Controllers");
            if (controller.animationClips.Length > 0)
            {
                foreach (var layer in controller.layers)
                {
                    if (layer.stateMachine.name == "") layer.stateMachine.name = layer.name;
                    ProcessStateMachine(directories, layer.stateMachine);
                }
            }
        }

        private static void ProcessStateMachine(DirectoryTable directories, AnimatorStateMachine stateMachine, string path = "")
        {

            path = path == "" ? stateMachine.name : $"{path}/{stateMachine.name}";
            foreach (var childState in stateMachine.states)
            {
                Motion anim = childState.state.motion;
                if (anim == null) continue;
                processMotion(directories, anim, path);
            }

            foreach (var childStateMachine in stateMachine.stateMachines)
            {
                ProcessStateMachine(directories, childStateMachine.stateMachine, path);
            }
        }

        private static void processMotion(DirectoryTable directories, Motion motion, string path = "")
        {
            if (motion is AnimationClip)
            {
                processAnimationClip(directories, motion as AnimationClip, path);
            }
            else if (motion is BlendTree)
            {
                processBlendTree(directories, motion as BlendTree, path);
            }
        }

        private static void processBlendTree(DirectoryTable directories, BlendTree blendTree, string path = "")
        {
            if (AssetDatabase.IsMainAsset(blendTree))
            {
                directories.Add(blendTree, $"Animations/{path}/{blendTree.name}");
            }
            foreach (var childMotion in blendTree.children)
            {
                processMotion(directories, childMotion.motion, path + "/" + blendTree.name);
            }
        }

        private static void processAnimationClip(DirectoryTable directories, AnimationClip anim, string path = "")
        {
            if (path != "" && path != null) directories.Add(anim, $"Animations/{path}");
            else directories.Add(anim, "Animations");
            EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(anim);
            foreach (var binding in bindings)
            {
                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(anim, binding);

                foreach (var keyframe in keyframes)
                {
                    if (includeMaterials.cachedValue) directories.Add(keyframe.value, $"Materials/{path}");
                    getTexturesFromMaterial(directories, keyframe.value as Material);
                    if (includeShaders.cachedValue) getShaderFromMaterial(directories, keyframe.value as Material);
                }

            }
        }
    }
}