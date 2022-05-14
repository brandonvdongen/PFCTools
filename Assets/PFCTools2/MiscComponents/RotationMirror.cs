using UnityEngine;

namespace PFCTools2.AvatarTools
{
    [ExecuteAlways]
    public class RotationMirror : MonoBehaviour
    {
        public GameObject Source;
        public Vector3 MirrorPlane = Vector3.right;

        public bool rotation = true;
        public bool position = true;
        private void Update()
        {
            if (Source != null)
            {
                if (rotation)
                {
                    Quaternion sourceQuaternion = Source.transform.localRotation;
                    Quaternion mirrorNormalQuat = new Quaternion(MirrorPlane.x, MirrorPlane.y, MirrorPlane.z, 0);
                    Quaternion reflectedQuat = mirrorNormalQuat * sourceQuaternion * mirrorNormalQuat;
                    transform.localRotation = reflectedQuat;
                }
                if (position)
                {
                    transform.localPosition = Vector3.Reflect(Source.transform.localPosition, MirrorPlane);
                }
            }
        }
    }
}