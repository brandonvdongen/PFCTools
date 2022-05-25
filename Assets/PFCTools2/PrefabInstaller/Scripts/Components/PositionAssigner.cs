using System;
using UnityEngine;

namespace PFCTools2.Installer.Core
{

    public class PositionAssigner : MonoBehaviour
    {
        public PositionAssignerMode Mode;
        public string MetaDataKey;
        public PositionOffsetEntry[] Offsets;

    }

    [Serializable]
    public struct PositionOffsetEntry
    {

        public string Meta;
        public Vector3 offset;
        public sourceData[] targets;
    }

    [Serializable]
    public struct sourceData
    {
        public HumanBodyBones bone;
        public float weight;
    }

    [Serializable]
    public enum PositionAssignerMode
    {
        All,
        MetaTags,
        MetaData
    }

}