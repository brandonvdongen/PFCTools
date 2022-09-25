#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

namespace PFCTools2.AvatarTools
{
    public class ArmatureBinder : EditorWindow
    {
        [MenuItem("PFCTools2/AvatarTools/Armature Binder")]
        public static void ShowWindow()
        {
            // Opens the window, otherwise focuses it if it’s already open.
            ArmatureBinder window = GetWindow<ArmatureBinder>();

            // Adds a title to the window.
            window.titleContent = new GUIContent("Armature Binder");

            // Sets a minimum size to the window.
            window.minSize = new Vector2(50, 50);
        }

        private void OnEnable()
        {
            Label bindLabel = new Label("Bind:");
            ObjectField sourceObject = new ObjectField() { objectType = typeof(GameObject) };
            Label toLabel = new Label("To:");
            ObjectField targetObject = new ObjectField() { objectType = typeof(GameObject) };
            Toggle useParent = new Toggle("Use Parent 4 all");
            Button bindArmatureBtn = new Button() { text = "Bind Armature" };
            bindArmatureBtn.clicked += () => { BindArmature(sourceObject.value as GameObject, targetObject.value as GameObject, useParent.value); sourceObject.value = null; targetObject.value = null; };
            rootVisualElement.Add(bindLabel);
            rootVisualElement.Add(sourceObject);
            rootVisualElement.Add(toLabel);
            rootVisualElement.Add(targetObject);
            rootVisualElement.Add(useParent);
            rootVisualElement.Add(bindArmatureBtn);
        }

        private void BindArmature(GameObject source, GameObject target, bool useParent)
        {

            Animator sourceAnimator = source.GetComponent<Animator>();
            Animator targetAnimator = target.GetComponent<Animator>();

            foreach (HumanBodyBones boneSlot in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (boneSlot == HumanBodyBones.LastBone)
                {
                    continue;
                }

                Transform sourceBone = sourceAnimator.GetBoneTransform(boneSlot);
                Transform targetBone = targetAnimator.GetBoneTransform(boneSlot);
                if (sourceBone == null || targetBone == null)
                {
                    continue;
                }

                if (boneSlot == HumanBodyBones.Hips || useParent)
                {
                    ParentConstraint constraint = sourceBone.gameObject.AddComponent<ParentConstraint>();
                    constraint.AddSource(new ConstraintSource() { sourceTransform = targetBone, weight = 1 });
                    constraint.locked = true;
                    constraint.constraintActive = true;
                }
                else
                {
                    RotationConstraint constraint = sourceBone.gameObject.AddComponent<RotationConstraint>();
                    constraint.AddSource(new ConstraintSource() { sourceTransform = targetBone, weight = 1 });
                    constraint.locked = true;
                    constraint.constraintActive = true;
                }


            }

            source.transform.parent = target.transform;
            DestroyImmediate(sourceAnimator);


        }
    }

}
#endif