using PFCTools2.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFCTools2.Installer.Core
{

    public abstract class PrefabTemplate : ScriptableObject
    {

        public static bool canDrawGizmos = false;
        public static Action OnConfigChange;
        public static Action OnGizmo;
        public GameObject Prefab;
        public TextAsset[] BaseLayers;
        public TextAsset[] AdditiveLayers;
        public TextAsset[] GestureLayers;
        public TextAsset[] ActionLayers;
        public TextAsset[] FXLayers;
        public string[] RequiredResourcePaths = new string[] { "Animations" };
        public virtual Dictionary<string, MetaData> MetaData { get; }
        public abstract string PrefabName { get; }
        public abstract string PrefabTag { get; }



        [OnOpenAsset()]
        public static bool OpenAssetEditor(Int32 instanceID, Int32 line)
        {
            string assetPath = AssetDatabase.GetAssetPath(instanceID);
            PrefabTemplate template = AssetDatabase.LoadAssetAtPath<PrefabTemplate>(assetPath);
            return OpenAssetEditor(template);
        }
        public static bool OpenAssetEditor(PrefabTemplate template)
        {
            if (template != null)
            {
                PrefabInstallerWindow.OpenWindow(template);
                return true;
            }
            return false;
        }

        public virtual VisualElement PrefabConfigUI()
        {
            return new Label("No Settings available for this prefab.");
        }
        public virtual VisualElement CustomizerUI()
        {
            return new Label("No Customizer available for this prefab.");
        }

        public virtual bool IsInstalledOn(AvatarDefinition avatar)
        {
            return GetInstalledPrefab(avatar) != null;
        }

        public virtual GameObject GetInstalledPrefab(AvatarDefinition avatar)
        {
            Transform[] children = avatar.transform.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child.CompareTag(PrefabTag))
                {
                    return child.gameObject;
                }
            }
            return null;
        }

        public virtual void Validate(List<ValidatorResponse> log, InstallerMode mode) { }
        private void OnValidate()
        {
            OnConfigChange?.Invoke();
        }

        public virtual List<string> GetMetaTags()
        {
            return new List<string>();
        }
        public virtual Dictionary<string, MetaData> GetMetaData()
        {
            return new Dictionary<string, MetaData>();
        }

        public virtual void BeforePrefabRemove() { }
        public virtual void AfterPrefabRemove() { }

        private void OnEnable()
        {
            SceneView.duringSceneGui += DrawGui;
            OnGizmo += DrawGizmos;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DrawGui;
            OnGizmo -= DrawGizmos;
        }

        public virtual void DrawGui(SceneView obj) { }

        [DrawGizmo(GizmoType.Selected | GizmoType.NotInSelectionHierarchy)]
        public static void OnDrawGizmo(Transform scr, GizmoType gizmoType)
        {
            if (canDrawGizmos) { OnGizmo?.Invoke(); }
        }
        public virtual void DrawGizmos() { }
    }

}