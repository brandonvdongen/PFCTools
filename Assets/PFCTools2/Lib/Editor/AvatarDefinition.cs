using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace PFCTools2.Utils {
    public class AvatarDefinition {
        public VRCAvatarDescriptor descriptor;
        public Animator _animator;
        public Animator Animator { get { if (_animator == null)_animator = descriptor.GetComponent<Animator>(); return _animator; } }
        public bool HasAnimator { get { return Animator; } }
        public VRCExpressionParameters ExpressionParameters { get { return descriptor.expressionParameters; } }
        public bool HasParameters { get { return ExpressionParameters != null; } }
        public VRCExpressionsMenu ExpressionsMenu { get { return descriptor.expressionsMenu; } }
        public bool HasMenu { get { return ExpressionParameters != null; } }

        public Transform transform { get { return descriptor.transform; } }
        public GameObject gameObject {  get { return descriptor.gameObject; } }

        public AvatarDefinition(VRCAvatarDescriptor descriptor) {
            this.descriptor = descriptor;
        }

        public RuntimeAnimatorController GetLayer(VRCAvatarDescriptor.AnimLayerType type) {
            foreach (VRCAvatarDescriptor.CustomAnimLayer layer in descriptor.baseAnimationLayers) {
                if (layer.type == type) {
                    return layer.animatorController;
                }
            }
            return null;
        }
        public bool HasLayer(VRCAvatarDescriptor.AnimLayerType type) {
            foreach (VRCAvatarDescriptor.CustomAnimLayer layer in descriptor.baseAnimationLayers) {
                if (layer.type == type) {
                    return layer.animatorController != null;
                }
            }
            return false;
        }


    }
}