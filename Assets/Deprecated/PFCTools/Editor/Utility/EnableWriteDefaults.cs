#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;

namespace Utils.EditorExtension
{
    public class EnableWriteDefaults : Editor
    {
        [MenuItem("Assets/PFCTools/Enable Write Defaults")]
        public static void UnfuckAnimator()
        {
            AnimatorController fuckedController = Selection.activeObject as AnimatorController;
            try
            {
                foreach (AnimatorControllerLayer layer in fuckedController.layers)
                {
                    int count = layer.stateMachine.states.Length;
                    int i = 0;
                    foreach (ChildAnimatorState childState in layer.stateMachine.states)
                    {
                        i++;
                        EditorUtility.DisplayProgressBar("Processing Layer (" + layer.name + ")", childState.state.name, (1 / (float)count) * i);
                        childState.state.writeDefaultValues = true;
                        AssetDatabase.SaveAssets();
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }



        }

        [MenuItem("Assets/PFCTools/Enable Write Defaults", true)]
        private static bool UnfuckAnimatorValidation()
        {
            return Selection.activeObject is AnimatorController;
        }
    }
}
#endif