using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace PFCTools2.Installer.Core {
    public abstract class PrefabTemplate : ScriptableObject {

        public Action onConfigChange;
        public GameObject Prefab;

        public void Init() {
            return;
        }

        public virtual VisualElement PrefabConfigUI() {
            return null;
        }
        public virtual void ValidateConfig(List<validatorResponse> log) {}
        private void OnValidate() {
            if (onConfigChange != null) {
                onConfigChange();
            }
        }

        public virtual List<string> getConstraintMetaTags() {
            return new List<string>();
        }
    }

}