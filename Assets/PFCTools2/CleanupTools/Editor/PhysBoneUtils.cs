using PFCTools2.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace PFCTools2.PhysboneUtils
{
    public static class PhysBoneUtils
    {
        #region Separate Physbones To Children
        [MenuItem("PFCTools2/Cleanup/PhysBones/Separate Bones Into Children")]
        public static void SeparatePhysonesToChildren()
        {
            VRCPhysBone[] bones = Selection.activeGameObject.GetComponentsInChildren<VRCPhysBone>();
            foreach (VRCPhysBone bone in bones)
            {
                GameObject ga = new GameObject(bone.GetRootTransform().name);
                VRCPhysBone newBone = ga.AddComponent<VRCPhysBone>();
                ga.transform.SetParent(bone.transform, false);

                ClassCopier.Copy<VRCPhysBone>(bone, newBone);
                Object.DestroyImmediate(bone);
            }
        }

        [MenuItem("PFCTools2/Cleanup/PhysBones/Separate Bones Into Children", true)]
        public static bool Validate()
        {
            if (Selection.activeGameObject.GetComponentInChildren<VRCPhysBone>() != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Collect All Physbones to Selected Parent
        [MenuItem("PFCTools2/Cleanup/PhysBones/Collect All Physbones")]
        public static void CollectPhysBones()
        {
            GameObject container = new GameObject("Container");
            VRCAvatarDescriptor descriptor = Selection.activeGameObject.GetComponentInParent<VRCAvatarDescriptor>();
            if (descriptor == null) { throw new System.Exception("No Avatar Root Found."); }

            VRCPhysBone[] bones = descriptor.gameObject.GetComponentsInChildren<VRCPhysBone>();
            Dictionary<Transform, Transform> gameObjects = new Dictionary<Transform, Transform>();
            foreach (VRCPhysBone bone in bones)
            {
                if (bone.rootTransform == null)
                {
                    bone.rootTransform = bone.transform;
                }

                Transform root = bone.rootTransform;
                HashSet<Transform> path = new HashSet<Transform>();

                Transform current = root;
                while (current != null && current != descriptor.transform)
                {

                    if (!gameObjects.ContainsKey(current))
                    {
                        GameObject go = new GameObject(current.name);
                        go.transform.parent = container.transform;
                        gameObjects.Add(current, go.transform);
                    }
                    current = current.parent;
                }
                if (gameObjects.ContainsKey(root))
                {
                    VRCPhysBone newBone = gameObjects[root].gameObject.AddComponent<VRCPhysBone>();
                    ClassCopier.Copy(bone, newBone);
                    newBone.rootTransform = root;
                    GameObject.DestroyImmediate(bone);
                }
            }
            foreach (Transform key in gameObjects.Keys)
            {
                if (gameObjects.ContainsKey(key.parent))
                {
                    gameObjects[key].parent = gameObjects[key.parent];
                }
                else
                {
                    gameObjects[key].name = "Physbones";
                    gameObjects[key].parent = descriptor.transform;
                }
            }
            GameObject.DestroyImmediate(container.gameObject);

        }

        [MenuItem("PFCTools2/Cleanup/PhysBones/Collect All Physbones", true)]
        public static bool CollectPhysBonesValidate()
        {
            VRCAvatarDescriptor desc = Selection.activeGameObject.GetComponentInParent<VRCAvatarDescriptor>();
            if (desc == null)
            {
                return false;
            }

            if (desc.gameObject.GetComponentInChildren<VRCPhysBone>() != null)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}