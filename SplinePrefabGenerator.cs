using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bezier
{
    public enum PlacementStyle
    {
        Distance,
        Total
    }

    [AddComponentMenu("Splines/Spline Prefab Generator")]
    public class SplinePrefabGenerator : MonoBehaviour
    {
        /// <summary>
        /// Spline to generate prefabs along
        /// </summary>
        public BezierSpline spline;

        /// <summary>
        /// Prefab to generate along the spline
        /// </summary>
        public GameObject prefab;

        /// <summary>
        /// Distance places a prefab every X distance along the spline.  Total spawns the exact number of prefabs sporead equidistantly along the spline
        /// </summary>
        public PlacementStyle placementStyle;

        /// <summary>
        /// Determines the distance between objects
        /// </summary>
        public float spacing = 1;

        /// <summary>
        /// Determines the number of objects generated on the spline
        /// </summary>
        public int instanceCount = 1;

        /// <summary>
        /// If true, object will look towards its current velocity
        /// </summary>
        public bool lookForward;

        [SerializeField]
        private List<GameObject> spawnedObjects = new List<GameObject>();

        /// <summary>
        /// Spawns prefabs along the spline.
        /// </summary>
        public void GeneratePrefabsAlongSpline()
        {
            if (!prefab || !spline) return;

            float length = spline.GetTotalLength();
            int count = (placementStyle == PlacementStyle.Distance) ? Mathf.FloorToInt(length / spacing) : instanceCount;
            float currSpacing = (placementStyle == PlacementStyle.Distance) ? spacing : ((spline.GetTotalLength() - 0.05f) / instanceCount);
            
            for (int i = 0; i < count; i++)
            {
                if (spawnedObjects.Count <= i || spawnedObjects == null)
                    spawnedObjects.Add(Instantiate(prefab, transform));

                float dist = (i + 1) * currSpacing;
                Transform t = spawnedObjects[i].transform;
                t.position = spline.GetPointOnSplineByDistance(dist);

                if (lookForward)
                {
                    t.rotation = Quaternion.LookRotation(spline.GetDerivativeByDistance(dist), Vector3.up);
                }
            }

            for (int i = spawnedObjects.Count - 1; i >= count; i--)
            {
                GameObject g = spawnedObjects[i];
                spawnedObjects.RemoveAt(i);
                DestroyImmediate(g);
            }
        }
    }
}
