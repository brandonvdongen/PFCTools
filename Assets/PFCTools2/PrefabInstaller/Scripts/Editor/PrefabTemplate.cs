using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using PFCTools2.Utils;

namespace PFCTools2.Installer.Core {
    public abstract class PrefabTemplate : ScriptableObject {

        public bool debug = false;
        public Action onConfigChange;
        public GameObject Prefab;
        public TextAsset[] BaseLayers;
        public TextAsset[] AdditiveLayers;
        public TextAsset[] GestureLayers;
        public TextAsset[] ActionLayers;
        public TextAsset[] FXLayers;
        public abstract string PrefabTag { get; }

        public void Init() {
            return;
        }

        public virtual VisualElement PrefabConfigUI() {
            return new Label("No Settings available for this prefab. " + GetType());
        }
        internal VisualElement CustomizerUI() {
            return new Label("No Customizer available for this prefab " + GetType());
        }

        public virtual bool IsInstalledOn(AvatarDefinition avatar) {
            return GetInstalledPrefab(avatar) != null;
        }

        public virtual GameObject GetInstalledPrefab(AvatarDefinition avatar) {
            Transform[] children = avatar.transform.GetComponentsInChildren<Transform>();
            foreach (Transform child in children) {
                if (child.CompareTag(PrefabTag)) return child.gameObject;
            }
            return null;
        }

        public virtual void Validate(List<ValidatorResponse> log, InstallerMode mode) { }
        private void OnValidate() {
            if (onConfigChange != null) {
                onConfigChange();
            }
        }

        public virtual List<string> getConstraintMetaTags() {
            return new List<string>();
        }
        public virtual void BeforePrefabRemove() { }
        public virtual void AfterPrefabRemove() { }
    }

}