using System;
using UnityEngine;

namespace PFCTools2.Installer.Core {

    
    public class ConstraintAssigner : MonoBehaviour {
        public Behaviour TargetConstraint;
        public ConstraintAssignerMode Mode; 
        public HumanBoneEntry[] Sources;
    }

    [Serializable]
    public struct HumanBoneEntry {
        public string Meta;
        public HumanBodyBones targetBone;
        public float weight;
    }

    public enum ConstraintAssignerMode {
        All,
        Meta
    }
}