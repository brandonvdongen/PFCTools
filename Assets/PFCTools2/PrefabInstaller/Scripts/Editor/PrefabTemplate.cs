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
        public virtual string[] ComponentTags { get; }

        //Installer functions
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
        //UI WINDOWS
        public virtual VisualElement PrefabConfigUI(PrefabInstallerWindow installer)
        {
            return new Label("No Settings available for this prefab.");
        }
        public virtual VisualElement CustomizerUI(PrefabInstallerWindow installer)
        {
            return new Label("No Customizer available for this prefab.");
        }

        //VALIDATION AND COMPONENT HANDLING
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

        public virtual HashSet<GameObject> GetPrefabComponents(AvatarDefinition avatar)
        {
            Transform[] children = avatar.transform.GetComponentsInChildren<Transform>();
            HashSet<GameObject> gameObjects = new HashSet<GameObject>();
            foreach (Transform child in children)
            {
                foreach (string tag in ComponentTags)
                {
                    if (child.CompareTag(tag))
                    {
                        gameObjects.Add(child.gameObject);
                    }
                }
            }
            return gameObjects;
        }

        public virtual void Validate(PrefabInstallerWindow installer) { }
        private void OnValidate()
        {
            OnConfigChange?.Invoke();
        }

        //META DATA
        public virtual List<string> GetMetaTags()
        {
            return new List<string>();
        }
        public virtual Dictionary<string, MetaData> GetMetaData()
        {
            return new Dictionary<string, MetaData>();
        }

        //Events
        public virtual void BeforePrefabRemove(PrefabInstallerWindow installer) { }
        public virtual void AfterPrefabRemove(PrefabInstallerWindow installer) { }

        //GUI HANDLING
        public virtual void DrawGUI(PrefabInstallerWindow installer) { }

        //[DrawGizmo(GizmoType.Selected | GizmoType.NotInSelectionHierarchy)]
        //internal static void OnDrawGizmo(Transform scr, GizmoType gizmoType)
        //{
        //if (canDrawGizmos) { OnGizmo?.Invoke(); }
        //}
        public virtual void DrawGizmos(PrefabInstallerWindow installer) { }
    }
}