using System;
using UnityEngine;

namespace PFCTools2.Utils
{
    [ExecuteInEditMode]
    public class CentroidFinder : MonoBehaviour
    {
        [SerializeField]
        private SourceData[] sources;
        private Transform lastSource;
        [SerializeField]
        private Vector3 offset;

        private void Awake()
        {
            lastSource = transform;
        }

        private void OnDrawGizmos()
        {
            if (lastSource == null)
            {
                lastSource = transform;
            }

            Vector3 centroid = Vector3.zero;
            Vector3 weighedCentroid = Vector3.zero;
            Gizmos.color = Color.blue;
            float totalWeight = 0;
            float maxWeight = 1;
            foreach (SourceData source in sources)
            {

                Gizmos.DrawWireSphere(source.target.position, 0.05f);
                Gizmos.DrawLine(lastSource.position, source.target.position);

                lastSource = source.target;
                centroid += source.target.position;
                weighedCentroid += source.target.position * source.weight;
                totalWeight += source.weight;
                maxWeight = Mathf.Max(maxWeight, source.weight);
            }
            Vector3 Center = centroid / sources.Length;
            Gizmos.DrawSphere(Center, 0.1f);
            Gizmos.color = Color.cyan;
            foreach (SourceData source in sources)
            {
                Gizmos.DrawLine(source.target.position, Center);
            }

            Vector3 WeighedCenter = weighedCentroid / totalWeight;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(WeighedCenter, 0.1f);

            Color newCol = Color.red;

            foreach (SourceData source in sources)
            {
                newCol.a = source.weight / maxWeight;
                newCol.r = source.weight / maxWeight;
                Gizmos.color = newCol;
                Gizmos.DrawLine(source.target.position, WeighedCenter);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(WeighedCenter, WeighedCenter + offset);
            Gizmos.DrawWireSphere(WeighedCenter + offset, 0.11f);


        }
    }

    [Serializable]
    internal struct SourceData
    {
        public Transform target;
        [Range(0f, 1f)]
        public float weight;
    }

}