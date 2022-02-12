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

        static void ExportAvatar() {



            GameObject avatar = Selection.activeObject as GameObject;



            Object root = avatar;
            List<Object> prefabs = new List<Object>();
            List<Material> materials = new List<Material>();
            List<Texture> textures = new List<Texture>();
            List<Object> meshes = new List<Object>();
            List<AudioClip> sounds = new List<AudioClip>();

            //Add the selected object to the selection if it's a asset.
            if (AssetDatabase.GetAssetPath(avatar) != "") {
                root = avatar;
            }

            //Find parent prefabs and add them to the selection 
            int maxDepth = 100;
            GameObject parentPrefab = PrefabUtility.GetCorrespondingObjectFromSource(avatar);
            while (parentPrefab != null && maxDepth > 0) {
                if (PrefabUtility.GetPrefabAssetType(parentPrefab) == PrefabAssetType.Model) {
                    meshes.Add(parentPrefab);
                }
                else {
                    prefabs.Add(parentPrefab);
                }

                parentPrefab = PrefabUtility.GetCorrespondingObjectFromSource(parentPrefab);
                maxDepth--;
            }


            //Find all AudioSources
            List<AudioSource> audioSources = new List<AudioSource>(avatar.GetComponentsInChildren<AudioSource>(true));


            //find all renderers
            List<Renderer> renderers = new List<Renderer>(avatar.GetComponentsInChildren<Renderer>(true));
            //go through al renderers (mesh, particle, trail, etc)
            foreach (Renderer r in renderers) {
                //fetch all materials from the renderers
                foreach (Material m in r.sharedMaterials) {
                    materials.Add(m);
                    //fetch all textures from the material
                    foreach (string name in m.GetTexturePropertyNames()) {
                        Texture tex = m.GetTexture(name);
                        if (tex != null) {
                            textures.Add(tex);
                        }
                    }

                }
                //if the renderer is a skinned mesh renderer fetch the mesh
                if (r is SkinnedMeshRenderer) {
                    SkinnedMeshRenderer smr = r as SkinnedMeshRenderer;
                    Object prefab = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(smr.sharedMesh), typeof(Object));
                    meshes.Add(prefab);
                    Debug.Log($"Fetch Mesh: {prefab}", prefab);
                }


            }
            //See if the Selected object is part of a scene asset, if so make it the root.
            if (avatar.scene != null) {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath(avatar.scene.path, typeof(SceneAsset)) as SceneAsset;
                if (sceneAsset != null) {
                    root = sceneAsset;
                }
            }

            //Restructure all files in new folders.
            if (materials.Count > 0) Directory.CreateDirectory($"Assets/{avatar.name}/Materials");
            if (textures.Count > 0) Directory.CreateDirectory($"Assets/{avatar.name}/Textures");
            if (meshes.Count > 0) Directory.CreateDirectory($"Assets/{avatar.name}/Models");
            if (prefabs.Count > 0) Directory.CreateDirectory($"Assets/{avatar.name}/Prefabs");
            if(audioSources.Count >0) Directory.CreateDirectory($"Assets/{avatar.name}/Sounds");
            //Collect all Audio



            AssetDatabase.Refresh();

            //go through all sound sources
            foreach (var a in audioSources) {
                string path = AssetDatabase.GetAssetPath(a.clip);
                AssetDatabase.MoveAsset(path, $"Assets/{avatar.name}/Sounds/{Path.GetFileName(path)}");
            }

            
            foreach (var mat in materials) {
                string path = AssetDatabase.GetAssetPath(mat);
                string fileName = Path.GetFileName(path);
                AssetDatabase.MoveAsset(path, $"Assets/{avatar.name}/Materials/{fileName}");
            }

            foreach (var tex in textures) {
                string path = AssetDatabase.GetAssetPath(tex);
                string fileName = Path.GetFileName(path);
                AssetDatabase.MoveAsset(path, $"Assets/{avatar.name}/Textures/{fileName}");
            }

            foreach (var model in meshes) {
                string path = AssetDatabase.GetAssetPath(model);
                string fileName = Path.GetFileName(path);
                AssetDatabase.MoveAsset(path, $"Assets/{avatar.name}/Models/{fileName}");
            }

            foreach (var prefab in prefabs) {
                string path = AssetDatabase.GetAssetPath(prefab);
                string fileName = Path.GetFileName(path);
                AssetDatabase.MoveAsset(path, $"Assets/{avatar.name}/Prefabs/{fileName}");
            }

            {
                string path = AssetDatabase.GetAssetPath(root);
                string fileName = Path.GetFileName(path);
                AssetDatabase.MoveAsset(path, $"Assets/{avatar.name}/{fileName}");
            }

            //Handle Animator and animations

            Dictionary<string, string> animations = new Dictionary<string, string>();

            Animator animator = avatar.GetComponent<Animator>();
            if (animator.runtimeAnimatorController != null) {
                AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
                Directory.CreateDirectory($"Assets/{avatar.name}/Layers");
                AssetDatabase.Refresh();

                string path = AssetDatabase.GetAssetPath(controller);
                string fileName = Path.GetFileName(path);
                AssetDatabase.MoveAsset(path, $"Assets/{avatar.name}/Layers/{fileName}");

                if (controller.animationClips.Length > 0) {
                    foreach (var layer in controller.layers) {
                        string dir = $"Assets/{avatar.name}/Animations/{layer.name}";
                        foreach (var childState in layer.stateMachine.states) {
                            Motion anim = childState.state.motion;
                            if (anim == null) continue;
                            string aPath = AssetDatabase.GetAssetPath(anim);
                            string aFileName = Path.GetFileName(aPath);


                            if (animations.ContainsKey(aPath)) {
                                if (animations[aPath] != dir) {
                                    animations[aPath] = $"Assets/{avatar.name}/Animations/shared";
                                }
                            }
                            else {
                                animations.Add(aPath, dir);
                            }
                        }
                    }
                    List<string> directories = new List<string>();
                    foreach (string dir in animations.Values) {
                        if (!directories.Contains(dir)) {
                            Directory.CreateDirectory(dir);
                            Debug.Log($"Create: {dir}");
                            directories.Add(dir);
                        }
                    }
                    AssetDatabase.Refresh();
                    foreach (string animPath in animations.Keys) {

                        if (animPath != "" && animPath != null) {
                            AssetDatabase.MoveAsset(animPath, $"{animations[animPath]}/{Path.GetFileName(animPath)}");
                            Debug.Log($"move: {animPath} to {animations[animPath]}/{Path.GetFileName(animPath)}");
                        }

                    }
                }
                AssetDatabase.Refresh();
            }
            //Selection.objects = objects.ToArray();
        }


    }
}