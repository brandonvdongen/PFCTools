#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PFCTools.Outfitter
{

    [ExecuteInEditMode]
    public class PFCEZOutfitter : MonoBehaviour
    {

        public bool showDebug = false;
        private Dictionary<Transform, Transform> _boneMap = new Dictionary<Transform, Transform>();

        public Dictionary<Transform, Transform> boneMap { get { return _boneMap; } }

        public void WearItem()
        {

            GameObject[] avatars = VRCSDK.GetAvatars();
            if (avatars.Length < 1)
            {
                EditorUtility.DisplayDialog("No avatars", "No avatar was found in the scene, make sure there's an avatar in your scene before using the merger!", "OK");
            }

            if (avatars.Length > 1)
            {
                EditorUtility.DisplayDialog("Too many avatars", "More then 1 avatar was found in the scene, make sure there's only 1 avatar in your scene before using the merger!", "OK");
            }

            if (avatars.Length != 1)
            {
                return;
            }

            Transform target = avatars[0].transform;

            int UndoID = Undo.GetCurrentGroup();
            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();


            if (PrefabUtility.GetPrefabInstanceHandle(this.gameObject) != null)
            {
                Debug.Log("Unpacking Prefab");
                PrefabUtility.UnpackPrefabInstance(this.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
            }


            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                Undo.RecordObject(gameObject, "Wear Item");

                foreach (Transform bone in renderer.bones)
                {
                    foreach (Transform targetChild in target.GetComponentsInChildren<Transform>())
                    {
                        if (bone.name == targetChild.name && !_boneMap.ContainsKey(bone))
                        {
                            _boneMap.Add(bone, targetChild);
                            continue;
                        }
                    }
                    if (!_boneMap.ContainsKey(bone))
                    {
                        if (showDebug)
                        {
                            Debug.DrawLine(bone.position, _boneMap[bone.parent].position, Color.blue, 5);
                        }

                        Vector3 localpos = bone.localPosition;
                        Vector3 localscale = bone.localScale;
                        Quaternion localrot = bone.localRotation;

                        Transform newTransform = new GameObject().transform;
                        Undo.RegisterCreatedObjectUndo(newTransform.gameObject, "Create new bone");
                        newTransform.parent = _boneMap[bone.parent];
                        newTransform.localPosition = localpos;
                        newTransform.localRotation = localrot;
                        newTransform.localScale = localscale;
                        newTransform.gameObject.name = bone.name;
                        _boneMap.Add(bone, newTransform);
                        if (showDebug)
                        {
                            Debug.Log(string.Format("Created {0} and parented to {1}", newTransform.name, _boneMap[bone.parent]));
                        }

                        //setLocalParentUndo(bone.transform,boneMap[bone.parent]);
                    }
                }

                Undo.RegisterFullObjectHierarchyUndo(renderer.gameObject, "Transfer Mesh");
                Transform[] newBones = new Transform[renderer.bones.Length];
                for (int i = 0; i < renderer.bones.Length; i++)
                {
                    foreach (Transform newBone in target.GetComponentsInChildren<Transform>())
                    {
                        if (newBone.name == renderer.bones[i].name)
                        {
                            newBones[i] = newBone;
                            renderer.rootBone = newBone;
                            continue;
                        }
                    }
                }
                renderer.rootBone = _boneMap[renderer.bones[0]];
                renderer.bones = newBones;
                setLocalParentUndo(renderer.transform, target);
            }

            foreach (KeyValuePair<Transform, Transform> bone in boneMap)
            {
                Component[] components = bone.Key.GetComponents<Component>();

                foreach (Component component in components)
                {
                    if (!(component.GetType() == transform.GetType()))
                    {
                        if (showDebug)
                        {
                            Debug.Log(string.Format("found: {0} on {1}", component.GetType().ToString(), bone.Key.name));
                        }

                        if (component.GetType().ToString() == "DynamicBone")
                        {
                            Type DynamicBoneType = component.GetType();
                            FieldInfo FI = DynamicBoneType.GetField("m_Root");
                            FI.SetValue(component, bone.Value);

                            //Type DynamicBoneColliderType = 

                            FieldInfo CF = DynamicBoneType.GetField("m_Colliders");
                            Debug.Log(CF.GetValue(component));
                            IList list = CF.GetValue(component) as IList;

                            List<Component> newColliders = new List<Component>();

                            foreach (object collider in list)
                            {
                                Type ColliderType = collider.GetType();
                                Component colliderBone = collider as Component;
                                Component newCollider = Undo.AddComponent(_boneMap[colliderBone.transform].gameObject, ColliderType);
                                foreach (FieldInfo field in collider.GetType().GetFields())
                                {
                                    Debug.Log(string.Format("set {0} to {1} on {2}", field.GetValue(collider), field.GetValue(collider), newCollider));
                                    field.SetValue(newCollider, field.GetValue(collider));
                                }
                                newColliders.Add(newCollider);
                            }
                            list.Clear();
                            foreach (Component newCollider in newColliders)
                            {
                                list.Add(newCollider);
                            }
                            CF.SetValue(component, list);

                            UnityEditorInternal.ComponentUtility.CopyComponent(component);
                            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(bone.Value.gameObject);
                        }

                    }
                }

            }

            if (showDebug)
            {
                Debug.Log(string.Format("Destoying {0}", gameObject));
            }

            Undo.DestroyObjectImmediate(gameObject);
            Undo.SetCurrentGroupName("Wear Item");
            Undo.CollapseUndoOperations(UndoID);
            Selection.activeObject = target;
        }

        private void setLocalParentUndo(Transform transform, Transform target)
        {
            Vector3 localpos = transform.localPosition;
            Quaternion localrot = transform.localRotation;
            Undo.SetTransformParent(transform, target, "change parent");
            Undo.RegisterCompleteObjectUndo(transform, "change transform position");
            transform.localPosition = localpos;
            transform.localRotation = localrot;
        }

        public void updateBoneMap(Transform target)
        {
            _boneMap = new Dictionary<Transform, Transform>();
            foreach (Transform bone in gameObject.GetComponentsInChildren<Transform>())
            {
                foreach (Transform targetBone in target.GetComponentsInChildren<Transform>())
                {
                    if (bone.gameObject.name == targetBone.gameObject.name)
                    {
                        _boneMap.Add(bone, targetBone);
                    }
                }
            }
        }
        private void OnDrawGizmos()
        {

            if (showDebug)
            {
                foreach (KeyValuePair<Transform, Transform> kvp in _boneMap)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(kvp.Key.position, kvp.Value.position);
                }
            }
        }
    }
}

#endif