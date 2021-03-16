using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Utils.EditorExtension {
    public class EnableWriteDefaults : Editor {
        [MenuItem("Assets/PFCTools/Enable Write Defaults")]
        private static void UnfuckAnimator() {
            AnimatorController fuckedController = Selection.activeObject as AnimatorController;

            foreach (AnimatorControllerLayer layer in fuckedController.layers){
                foreach(ChildAnimatorState childState in layer.stateMachine.states) {
                    childState.state.writeDefaultValues = true;
                    AssetDatabase.SaveAssets();
                }
            }

            

        }

        [MenuItem("Assets/PFCTools/Enable Write Defaults", true)]
        private static bool UnfuckAnimatorValidation() {
            return Selection.activeObject is AnimatorController;
        }
    }
}