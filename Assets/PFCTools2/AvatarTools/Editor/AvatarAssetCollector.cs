using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using PFCTools2.Utils;


namespace PFCTools2.AvatarTools {
    public class AvatarAssetCollector {
        [MenuItem("GameObject/PFCTools/Folderize Asset Dependencies", false, 0)]
        static void HierarchyExport() { 
            ExportAvatar();
        }
        [MenuItem("GameObject/PFCTools/Folderize Asset Dependencies", true, 0)]
        static bool HierarchyValidation() {
            return ValidateIfAvatar();
        }

        [MenuItem("Assets/PFCTools/Folderize Asset Dependencies", false, 303)]
        static void ProjectExport() {
            ExportAvatar();
        }

        [MenuItem("Assets/PFCTools/Folderize Asset Dependencies", true, 303)]
        static bool ProjectValidate() {
            return ValidateIfAvatar();
        }

        static bool ValidateIfAvatar() {
            bool isValid = false;
            GameObject go = Selection.activeGameObject;

            if (go != null) {
                VRCAvatarDescriptor descriptor = go.GetComponent<VRCAvatarDescriptor>();
                isValid = (descriptor != null);
            }
            //return isValid;
            return true;
        }

        private class DirectoryTable {

            public string rootPath = "";
            private bool _allowVRCSDKFiles = false;
            private bool _VRCSDKChecked = false;

            public DirectoryTable(string rootPath) {
                this.rootPath = rootPath;
            }

            private Dictionary<string, string> _paths = new Dictionary<string, string>();
            public Dictionary<string, string> Paths { get => _paths; }
            public void Add(string orgPath, string destPath = "") {
                Debug.Log($"{orgPath} > {destPath}");
                if (Path.GetDirectoryName(orgPath) == $"Assets/{rootPath}/{destPath}") return;
                if (orgPath.Contains("VRCSDK")) {
                    if (orgPath.Contains("proxy_")) return;
                    if (!_VRCSDKChecked) {
                        _allowVRCSDKFiles = EditorUtility.DisplayDialog("Are you sure?", $"One or more the files in this model were found to be part of the VRCSDK, do you wish to move SDK files (in case they where modified) or leave them in the VRCSDK Folder?\n\n File:{orgPath}", "Move to avatar folder", "leave in VRCSDK folder");
                        _VRCSDKChecked = true;
                    }
                    if (!_allowVRCSDKFiles) return;

                }
                
                if (_paths.ContainsKey(orgPath)) {
                    if (_paths[orgPath] == destPath) {
                        string newPath = Path.GetDirectoryName(destPath);
                        if(rootPath != "")_paths[orgPath] = $"Assets/{rootPath}/{newPath}/Shared";
                        else _paths[orgPath] = $"Assets/{rootPath}/{newPath}/Shared";
                        
                    }
                }
                else {
                    Debug.Log($"wtf: {orgPath} | Assets/{rootPath}/{destPath}");
                    string path = "Assets";
                    if (rootPath.Length > 0) path += "/" + rootPath;
                    if (destPath.Length > 0) path += "/" + destPath;
                    _paths.Add(orgPath, path);
                }
            }
            public void Add(Object _object, string destPath = "") {
                string orgPath = AssetDatabase.GetAssetPath(_object);
                Add(orgPath, destPath);
            }

            public void Rem(string orgPath) {
                if(_paths.ContainsKey(orgPath)){
                    _paths.Remove(orgPath);
                }
            }
            public void Rem(Object _object) {
                string orgPath = AssetDatabase.GetAssetPath(_object);
                Rem(orgPath);
            }
        }

        static void ExportAvatar() {

            System.DateTime startTime = System.DateTime.Now;

            GameObject prefab = Selection.activeGameObject as GameObject;
            DirectoryTable directories = new DirectoryTable(prefab.name);
            bool cancel;
            cancel = EditorUtility.DisplayCancelableProgressBar("Fetching Avatar Files", "Processing Avatar.", 0);


            //See if the Selected object is part of a scene asset, if so make it the root.
            Object root = prefab;
            if (prefab.scene != null) {
                if (prefab.scene.path != "" && prefab.scene.path != null) {
                    directories.Add(prefab.scene.path, "");
                    root = AssetDatabase.LoadAssetAtPath<Object>(prefab.scene.path);
                }
                else {
                    directories.Add(prefab);
                    root = prefab;
                }
            }

            //Get Materials and Textures and meshes
            foreach (Renderer _renderer in prefab.GetComponentsInChildren<Renderer>(true)) {
                foreach (Material _material in _renderer.sharedMaterials) {
                    directories.Add(_material, "Materials");
                    getTexturesFromMaterial(directories, _material);
                }
                if (_renderer is SkinnedMeshRenderer) {
                    SkinnedMeshRenderer smr = _renderer as SkinnedMeshRenderer;
                    directories.Add(smr.sharedMesh, "Models");
                }
                else if(_renderer is MeshRenderer) {
                    MeshFilter filter = _renderer.gameObject.GetComponent<MeshFilter>();
                    if(filter != null) {
                        directories.Add(filter.sharedMesh,"Models");
                        
                    }
                }
                else if(_renderer is ParticleSystemRenderer) {

                    ParticleSystemRenderer psr = _renderer as ParticleSystemRenderer;
                    Mesh[] meshes = new Mesh[0];

                    if (psr.mesh != null) {
                        directories.Add(psr.mesh, "Models");
                    }
                    else if (psr.GetMeshes(meshes) > 0) {
                        foreach(var mesh in meshes) {
                            directories.Add(mesh, "Models");
                        }
                    }
                }

            }

            //Get Sounds
            foreach (var a in prefab.GetComponentsInChildren<AudioSource>(true)) {
                directories.Add(a.clip, "Sound");
            }

            //Get Main Prefabs
            int maxDepth = 100;
            GameObject parentPrefab = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
            while (parentPrefab != null && maxDepth > 0) {
                if (PrefabUtility.GetPrefabAssetType(parentPrefab) == PrefabAssetType.Model) {
                    directories.Add(parentPrefab, "Models");
                }
                else {
                    directories.Add(parentPrefab, "Prefabs");
                }

                parentPrefab = PrefabUtility.GetCorrespondingObjectFromSource(parentPrefab);
                maxDepth--;
            }

            //Hanlde Animator Controller
            Animator animator = prefab.GetComponent<Animator>();
            if (animator) {
                AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
                if (controller) {
                    ProcessAnimatorController(directories, controller);
                }
            }
            //Find potential world constraints
            
            foreach(IConstraint constraint in prefab.GetComponentsInChildren<IConstraint>()) {
                List<ConstraintSource> sources = new List<ConstraintSource>();
                constraint.GetSources(sources);

                foreach(var source in sources) {
                    if(source.sourceTransform.gameObject.scene.path == null) {
                        directories.Add(source.sourceTransform.gameObject, "Extras");
                    }
                }
            }

            //VRCDescriptor Files
            VRCAvatarDescriptor descriptor = prefab.GetComponent<VRCAvatarDescriptor>();
            if (descriptor) {
                foreach (var layer in descriptor.baseAnimationLayers) {
                    ProcessAnimatorController(directories, layer.animatorController as AnimatorController);
                }
                foreach (var layer in descriptor.specialAnimationLayers) {
                    ProcessAnimatorController(directories, layer.animatorController as AnimatorController);
                }

                ProcessExpressionsMenu(directories, descriptor.expressionsMenu);

                if (descriptor.expressionParameters != null) {
                    directories.Add(descriptor.expressionParameters, "VRC/Expressions");
                }
            }




            ///////////Process files//////////
            float i = 1;
            List<string> _directoryList = new List<string>();
            foreach (string value in directories.Paths.Values) {
                cancel = EditorUtility.DisplayCancelableProgressBar("Compiling folder structure", value, i / directories.Paths.Count);

                if (!_directoryList.Contains(value)) {
                    _directoryList.Add(value);
                }
                i++;
            }

            i = 1;
            foreach (string dir in _directoryList) {
                cancel = EditorUtility.DisplayCancelableProgressBar("Creating Directories", dir, i / _directoryList.Count);
                Directory.CreateDirectory($"{dir}");
                i++;
            }
            AssetDatabase.Refresh();

            List<string> lines = new List<string>();

            i = 1;
            foreach (var path in directories.Paths.Keys) {
                EditorUtility.DisplayCancelableProgressBar("Moving Avatar Files", path, i / directories.Paths.Keys.Count);
                string fileName = Path.GetFileName(path);
                string destPath = $"{directories.Paths[path]}/{fileName}";
                AssetDatabase.MoveAsset(path, destPath);
                if(path != destPath)lines.Add($"{lines.Count}: {path} >>> {destPath}");
                i++;
            }
            if(lines.Count > 0)File.WriteAllLines(Path.GetDirectoryName(AssetDatabase.GetAssetPath(root)) + "/MoveManifest.txt",lines);
            Selection.activeObject = root;
            EditorUtility.ClearProgressBar();
            System.TimeSpan Time = System.DateTime.Now.Subtract(startTime);
            Debug.Log($"Processed avatar: {prefab.name} Took {Time.TotalMinutes:0}:{Time.Seconds:00}Minutes");
            AssetDatabase.Refresh();
        }


        //////////////////// METHODS //////////////////////////

        private static void ProcessExpressionsMenu(DirectoryTable directories, VRCExpressionsMenu menu) {
            directories.Add(menu, "VRC/Menus");
            if (menu != null) {
                foreach (VRCExpressionsMenu.Control control in menu.controls) {
                    if(control.icon != null) {
                        directories.Add(control.icon, $"Textures/MenuIcons/{menu.name}");
                    }
                    if(control.subMenu != null) {
                        //Debug.Log(path);
                        directories.Add(control.subMenu, "VRC/Menus");
                        //if (path != "") path = path + "/" + control.subMenu.name;
                        //else path = control.subMenu.name;
                        ProcessExpressionsMenu(directories, control.subMenu);

                    }
                }
            }
        }

        private static void getTexturesFromMaterial(DirectoryTable directories, Material _material) {
            //TODO MOVE THIS
            directories.Add(_material.shader, "Shaders");
            foreach (string name in _material.GetTexturePropertyNames()) {
                Texture _texture = _material.GetTexture(name);
                if (_texture != null) {
                    directories.Add(_texture, "Textures");
                }
            }
        }

        private static void ProcessAnimatorController(DirectoryTable directories, AnimatorController controller) {
            if (controller == null) return;
            directories.Add(controller, "Controllers");
            if (controller.animationClips.Length > 0) {
                foreach (var layer in controller.layers) {
                    if(layer.stateMachine.name == "")layer.stateMachine.name = layer.name;
                    ProcessStateMachine(directories, layer.stateMachine);
                }
            }
        }

        private static void ProcessStateMachine(DirectoryTable directories, AnimatorStateMachine stateMachine, string path = "") {
            
            path = path == "" ? stateMachine.name : $"{path}/{stateMachine.name}";
            foreach (var childState in stateMachine.states) {
                Motion anim = childState.state.motion;
                if (anim == null) continue;
                processMotion(directories, anim, path);
            }

            foreach (var childStateMachine in stateMachine.stateMachines) {
                ProcessStateMachine(directories, childStateMachine.stateMachine, path);
            }
        }

        private static void processMotion(DirectoryTable directories, Motion motion, string path = "") {
            if (motion is AnimationClip) {
                processAnimationClip(directories, motion as AnimationClip, path);
            }
            else if (motion is BlendTree) {
                processBlendTree(directories, motion as BlendTree, path);
            }
        }

        private static void processBlendTree(DirectoryTable directories, BlendTree blendTree, string path = "") {
            if (AssetDatabase.IsMainAsset(blendTree)) {
                directories.Add(blendTree, $"Animations/{path}/{blendTree.name}");
            }
            foreach (var childMotion in blendTree.children) {
                processMotion(directories, childMotion.motion, path + "/" + blendTree.name);
            }
        }

        private static void processAnimationClip(DirectoryTable directories, AnimationClip anim, string path = "") {
            if (path != "" && path != null) directories.Add(anim, $"Animations/{path}");
            else directories.Add(anim, "Animations");
            EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(anim);
            foreach (var binding in bindings) {
                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(anim, binding);

                foreach (var keyframe in keyframes) {
                    directories.Add(keyframe.value, $"Materials/{path}");
                    getTexturesFromMaterial(directories, keyframe.value as Material);
                }

            }
        }
    }
}