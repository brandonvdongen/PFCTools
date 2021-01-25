using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;
public class AvatarCache {

    GameObject Target = null;

    //Avatar Component storage
    public readonly List<Component> AllComponents = new List<Component>();
    public readonly List<IConstraint> Constraints = new List<IConstraint>();
    public readonly List<SkinnedMeshRenderer> SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
    public readonly List<MeshRenderer> MeshRenderers = new List<MeshRenderer>();
    public readonly List<Component> Other = new List<Component>();

    void clearCache() {
        AllComponents.Clear();
        Constraints.Clear();
        SkinnedMeshRenderers.Clear();
        MeshRenderers.Clear();
        Other.Clear();
    }

    public void CacheAvatar(GameObject _target) {
        Target = _target;
        clearCache();
        if (Target == null) return;
        var Components = Target.GetComponentsInChildren<Component>();
        addToCache(Components);
    }

    public void updateCache() {
        if (Target == null) return;
        clearCache();
        var Components = Target.GetComponentsInChildren<Component>();
        addToCache(Components);
    }

    private void addToCache(Component[] Components) {
        float i = 0;
        float progress = 0;
        int discarded = 0;
        foreach (var component in Components) {

            if (Components.Length > 1000) {
                progress = i / Components.Length;
                EditorUtility.DisplayProgressBar("Searching Components", string.Format("({0}/{1})", i, Components.Length), progress);
                i++;
            }
            if (component is Transform) { discarded++; continue; }
            else if (component is IConstraint) Constraints.Add(component as IConstraint);
            else if (component is SkinnedMeshRenderer) SkinnedMeshRenderers.Add(component as SkinnedMeshRenderer);
            else if (component is MeshRenderer) MeshRenderers.Add(component as MeshRenderer);
            else Other.Add(component);

            AllComponents.Add(component);

        }
        EditorUtility.ClearProgressBar();
        Debug.Log(string.Format("Processed {0} components on {1}", Components.Length - discarded, Target.name));
    }
}