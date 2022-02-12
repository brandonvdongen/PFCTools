using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.Components;

namespace PFCTools2.AvatarTools {
    public class AvatarAssetCollector {
        [MenuItem("GameObject/PFCTools/Sort Avatar Files", false, 0)]
        static void hierarchyExport() {
            ExportAvatar();
        }
        [MenuItem("GameObject/PFCTools/Sort Avatar Files", true, 0)]
        static bool hierarchyValidation() {
            return ValidateIfAvatar();
        }

        [MenuItem("Assets/PFCTools/Sort Avatar Files", false, 0)]
        static void projectExport() {
            ExportAvatar();
        }

        [MenuItem("Assets/PFCTools/Sort Avatar Files", true, 0)]
        static bool projectValidate() {
            return ValidateIfAvatar();
        }

        static bool ValidateIfAvatar() {

            GameObject go = Selection.activeGameObject;
            if (go != null) {
                VRCAvatarDescriptor descriptor = go.GetComponent<VRCAvatarDescriptor>();
                return descriptor != null;
            }
            return false;
        }

        public static void moveFiles(List<Object> files, string assetPath) {
            foreach (Object file in files) {
                string path = AssetDatabase.GetAssetPath(file);
                string fileName = Path.GetFileName(path);
                if (assetPath == "") {
                    AssetDatabase.MoveAsset(path, $"Assets/{fileName}");
                }
                else {
                    AssetDatabase.MoveAsset(path, $"Assets/{assetPath}/{fileName}");
                }

            }
        }
        public static void moveFiles(Object file, string assetPath) {
            List<Object> files = new List<Object>();
            files.Add(file);
            moveFiles(files, assetPath);
        }

        private class DirectoryTable {

            public string rootPath;

            public DirectoryTable(string rootPath) {
                this.rootPath = rootPath;
            }

            private Dictionary<string, string> _paths = new Dictionary<string, string>();
            public Dictionary<string, string> Paths { get => _paths; }
            public void Add(string orgPath, string destPath) {
                if (_paths.ContainsKey(orgPath)) {
                    if (_paths[orgPath] == destPath) {
                        string newPath = Path.GetDirectoryName(destPath);
                        _paths[orgPath] = $"Assets/{rootPath}/{newPath}/Shared";
                    }
                }
                else {
                    _paths.Add(orgPath, $"Assets/{rootPath}/{destPath}");
                }
            }
            public void Add(Object _object, string destPath) {
                string orgPath = AssetDatabase.GetAssetPath(_object);
                Add(orgPath, destPath);
            }

        }

        static void ExportAvatar() {

            GameObject prefab = Selection.activeGameObject as GameObject;
            DirectoryTable directories = new DirectoryTable(prefab.name);

            EditorUtility.DisplayProgressBar("Fetching Avatar Files", "Processing Avatar.", 0);


            //See if the Selected object is part of a scene asset, if so make it the root.
            if (prefab.scene != null) {
                if (prefab.scene.path != "") {
                    directories.Add(prefab.scene.path, "");
                }
            }

            //Get Materials and Textures
            foreach (Renderer _renderer in prefab.GetComponentsInChildren<Renderer>(true)) {
                foreach (Material _material in _renderer.sharedMaterials) {
                    directories.Add(_material, "Materials");

                    foreach (string name in _material.GetTexturePropertyNames()) {
                        Texture _texture = _material.GetTexture(name);
                        if (_texture != null) {
                            directories.Add(_texture, "Textures");
                        }
                    }
                }
                if (_renderer is SkinnedMeshRenderer) {
                    SkinnedMeshRenderer smr = _renderer as SkinnedMeshRenderer;
                    directories.Add(smr.sharedMesh, "Models");
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

            Animator animator = prefab.GetComponent<Animator>();
            if (animator.runtimeAnimatorController != null) {
                AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
                directories.Add(controller, "Layers");
                if (controller.animationClips.Length > 0) {
                    foreach (var layer in controller.layers) {
                        foreach (var childState in layer.stateMachine.states) {
                            Motion anim = childState.state.motion;
                            if (anim == null) continue;
                            directories.Add(anim, $"Animations/{layer.name}");
                        }
                    }
                }
            }

            //Process files//////////////////////////////////////////////////////////////////////////////////////////////
            int i = 1;
            List<string> _directoryList = new List<string>();
            foreach (string value in directories.Paths.Values) {
                EditorUtility.DisplayProgressBar("Fetching Avatar Files", "Processing Folders.", i / directories.Paths.Count);

                if (!_directoryList.Contains(value)) {
                    _directoryList.Add(value);
                }
                i++;
            }

            i = 1;
            foreach (string dir in _directoryList) {
                EditorUtility.DisplayProgressBar("Fetching Avatar Files", "Creating Folders.", i / _directoryList.Count);
                Directory.CreateDirectory($"{dir}");
                //Debug.Log(val);
                i++;
            }
            AssetDatabase.Refresh();

            i = 1;
            foreach (var path in directories.Paths.Keys) {
                EditorUtility.DisplayProgressBar("Fetching Avatar Files", "Moving Files.", i / directories.Paths.Count);
                string fileName = Path.GetFileName(path);
                Debug.Log($"{path} > {directories.Paths[path]}/{fileName}");
                AssetDatabase.MoveAsset(path, $"{directories.Paths[path]}/{fileName}");
                i++;
            }


            EditorUtility.ClearProgressBar();


        }
    }
}