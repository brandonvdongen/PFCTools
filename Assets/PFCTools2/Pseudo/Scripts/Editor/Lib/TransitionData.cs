using System.Collections.Generic;
using UnityEditor.Animations;

namespace PFCTools2.Installer.PseudoParser
{
    public struct TransitionData
    {
        public bool isDefault;
        public string start;
        public AnimatorState end;
        public bool hasExitTime;
        public float exitTime;
        public bool hasFixedDuration;
        public float duration;
        public float offset;
        public TransitionInterruptionSource interruptionSource;
        public bool orderedInterruption;
        public bool canTransitionToSelf;
        public List<AnimatorStateTransition> transitions;
    }
}