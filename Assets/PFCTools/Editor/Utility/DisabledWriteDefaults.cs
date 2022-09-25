#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Utils.EditorExtension
{
    public class DisabledWriteDefaults : Editor
    {
        [MenuItem("Assets/PFCTools/Disable Write Defaults")]
        public static void FuckUpAnimator()
        {
            AnimatorController unfuckedController = Selection.activeObject as AnimatorController;
            try
            {
                foreach (AnimatorControllerLayer layer in unfuckedController.layers)
                {
                    int count = layer.stateMachine.states.Length;
                    int i = 0;
                    foreach (ChildAnimatorState childState in layer.stateMachine.states)
                    {
                        i++;
                        Debug.Log(1 / count);
                        EditorUtility.DisplayProgressBar("Processing Layer (" + layer.name + ")", childState.state.name, (1 / (float)count) * i);
                        childState.state.writeDefaultValues = false;
                        AssetDatabase.SaveAssets();
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }


        }

        [MenuItem("Assets/PFCTools/Disable Write Defaults", true)]
        private static bool FuckUpAnimatorValidation()
        {
            return Selection.activeObject is AnimatorController;
        }
    }
}
#endif