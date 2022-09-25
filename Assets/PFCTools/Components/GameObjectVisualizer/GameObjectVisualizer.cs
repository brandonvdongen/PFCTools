using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace PFCTools.Components
{
    public class GameObjectVisualizer : MonoBehaviour
    {

        [Header("Structure")]
        [SerializeField]
        public bool _ParentStructure = true;
        public bool _ShowDisabledParents = false;
        [Range(0.001f, 0.1f)]
        public float _BoneSphereSize = 0.2f;
        [Header("Constraints")]
        public bool _ConstraintConnections = true;

        private void OnDrawGizmos()
        {
            // Draw a yellow sphere at the transform's position

            if (_ParentStructure)
            {
                Gizmos.color = Color.green;
                Transform[] childList = GetComponentsInChildren<Transform>(true);
                foreach (Transform child in childList)
                {
                    Gizmos.color = Color.green;
                    if (child.gameObject.activeInHierarchy || _ShowDisabledParents)
                    {
                        if (!child.gameObject.activeInHierarchy)
                        {
                            Gizmos.color = Color.red;
                        }
                        Gizmos.DrawSphere(child.position, _BoneSphereSize);
                        if (child.parent)
                        {
                            Gizmos.DrawLine(child.position, child.parent.position);
                        }
                    }
                }
            }
            if (_ConstraintConnections)
            {
                Gizmos.color = Color.cyan;
                AimConstraint[] constraints = GetComponentsInChildren<AimConstraint>(true);
                foreach (AimConstraint constraint in constraints)
                {
                    List<ConstraintSource> sources = new List<ConstraintSource>();
                    constraint.GetSources(sources);
                    foreach (ConstraintSource source in sources)
                    {
                        if (source.sourceTransform)
                        {
                            //Gizmos.DrawArrow(constraint.transform.position, source.sourceTransform.position);
                            DrawArrow.ForGizmo(constraint.transform.position, source.sourceTransform.position - constraint.transform.position, 0.01f);

                        }
                    }
                }
            }
        }
    }
}