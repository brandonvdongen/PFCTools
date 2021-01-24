#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PFCTools.Outfitter {

    [CustomEditor(typeof(PFCEZOutfitter))]
    public class PFCEZOutfitterEditor : Editor {

        PFCEZOutfitter outfit;
        Vector2 bonelistscroll;

        private void OnEnable() {
            outfit = (PFCEZOutfitter)target;
        }

        public override void OnInspectorGUI() {

            if (VRCSDK.installed) { 
            if (GUILayout.Button("Equip item")) {
                outfit.WearItem();
            }
            }
            else {
                GUILayout.TextArea("Sorry, this Component requires the VRCSDK to be present in your project.");
            }
        }
    }
}
#endif