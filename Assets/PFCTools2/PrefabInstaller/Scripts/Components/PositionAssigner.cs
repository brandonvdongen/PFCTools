using System;
using UnityEngine;

namespace PFCTools2.Installer.Core {


    public class PositionAssigner : MonoBehaviour {
        public PositionOffsetEntry[] Offsets;
    }

    [Serializable]
    public struct PositionOffsetEntry {
        public string Meta;
        public Vector3 offset;
    }
}