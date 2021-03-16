using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class VRCModelCombiner : EditorWindow {
    [MenuItem("PFCTools/ModelCombiner")]
    public static void OpenWindow() {
        var window = GetWindow<VRCModelCombiner>();
        window.titleContent = new GUIContent("Model Combiner");
    }

    private SkinnedMeshRenderer MeshRenderer;
    private Transform root;



    void TransferMeshToTarget(SkinnedMeshRenderer skinnedMesh, Transform newRoot) {
        Transform[] newBones = new Transform[skinnedMesh.bones.Length];
        for (int i = 0; i < skinnedMesh.bones.Length; i++) {
            foreach (Transform newBone in newRoot.GetComponentsInChildren<Transform>()) {
                if (newBone.name == skinnedMesh.bones[i].name) {
                    newBones[i] = newBone;
                    continue;
                }
            }
        }
        skinnedMesh.bones = newBones;
    }


    void ShiftMissingBones(SkinnedMeshRenderer skinnedMesh, Transform targetArmature) {        
        Dictionary<Transform,Transform> boneMap = new Dictionary<Transform, Transform>();
        foreach (Transform bone in skinnedMesh.bones) {
            foreach (Transform targetChild in targetArmature.GetComponentsInChildren<Transform>()) {
                if (bone.name == targetChild.name && !boneMap.ContainsKey(bone)) {
                    boneMap.Add(bone, targetChild);
                    continue;
                }
            }
            if (!boneMap.ContainsKey(bone)) {
                Debug.DrawLine(bone.transform.position, boneMap[bone.parent].position, Color.blue);
                bone.transform.SetParent(boneMap[bone.parent], false);
            }
        }
        skinnedMesh.transform.parent = targetArmature;
    }

    private void OnGUI() {

        MeshRenderer = EditorGUILayout.ObjectField("Target", MeshRenderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
        root = EditorGUILayout.ObjectField("RootBone", root, typeof(Transform), true) as Transform;
        GUI.enabled = (MeshRenderer != null && root != null);

        if (GUILayout.Button("Shift bones to new armature")) {
            ShiftMissingBones(MeshRenderer, root);
        }

        if (GUILayout.Button("Update Skinned Mesh Renderer")) {
            TransferMeshToTarget(MeshRenderer, root);
        }
        if (GUILayout.Button("Transfer Mesh to new Model")) {
            MeshRenderer.transform.SetParent(root,false);
        }
    }
}
