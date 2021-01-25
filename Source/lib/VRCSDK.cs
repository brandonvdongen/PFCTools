#if UNITY_EDITOR
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace PFCTools {
    
    [ExecuteInEditMode]
    [InitializeOnLoad]
    public static class VRCSDK {

        
        static VRCSDK() {
            if (Directory.GetFiles(Application.dataPath, "VRCSDKBase.dll", SearchOption.AllDirectories).Length >= 1) {
                _installed = true;
                Debug.Log("VRCSDK Found!");
            }
            else {
                _installed = false;
                Debug.Log("VRCSDK not found!");
            }
        }

        static bool _installed = false;
        public static bool installed { get { return _installed; } }

        static List<GameObject> cachedAvatars;

        public static GameObject[] GetAvatars() {
            return GetAvatars(false);
        }

        public static GameObject[] GetAvatars(bool cachedOnly) {
            if (!_installed) return null;
            if (cachedOnly) return cachedAvatars.ToArray();
            Component[] components = Object.FindObjectsOfType<Component>();

            cachedAvatars = new List<GameObject>();
            float i = 1;
            foreach (var component in components) {
                
                if(components.Length > 1000) {
                    float progress = i / components.Length;
                    i++;
                    EditorUtility.DisplayProgressBar("Finding Avatars",string.Format("Processing... {0}/{1}",i,components.Length),progress);
                }
                if (component.GetType().ToString().Contains("VRCAvatarDescriptor")) {
                    if (component.gameObject.activeSelf) {
                        cachedAvatars.Add(component.gameObject);
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            Debug.Log(string.Format("Found {0} avatars.", cachedAvatars.Count));
            if (cachedAvatars.Count > 0) return cachedAvatars.ToArray();
            else return new GameObject[0];
        }

    }
}
#endif