#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Bezier
{
    [CustomEditor(typeof(SplinePrefabGenerator))]
    public class SplinePrefabGeneratorEditor : Editor
    {
        private SplinePrefabGenerator sPG;

        public void OnSceneGUI()
        {
            sPG = (SplinePrefabGenerator)target;

            if (sPG.spline != null)
                sPG.spline.RenderCurvesInSceneView();
        }

        public override void OnInspectorGUI()
        {
            sPG = (SplinePrefabGenerator)target;
            EditorGUI.BeginChangeCheck();

            sPG.spline = (BezierSpline)EditorGUILayout.ObjectField(new GUIContent("Bezier Spline", "Spline to generate prefabs along"), sPG.spline, typeof(BezierSpline), true);
            sPG.prefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Prefab", "Prefab to generate along the spline"), sPG.prefab, typeof(GameObject), false);

            if (sPG.spline)
            {
                sPG.placementStyle = (PlacementStyle)EditorGUILayout.EnumPopup(new GUIContent("Placement Style", "Distance places a prefab every X distance along the spline.  Total spawns the exact number of prefabs sporead equidistantly along the spline"), sPG.placementStyle);

                if (sPG.placementStyle == PlacementStyle.Distance)
                    sPG.spacing = EditorGUILayout.Slider(new GUIContent("Spacing", "Determines the distance between objects"), sPG.spacing, 0.1f, sPG.spline.GetTotalLength());
                else
                    sPG.instanceCount = EditorGUILayout.IntField(new GUIContent("Instance Count", "Determines the number of objects generated on the spline"), sPG.instanceCount);
            }

            sPG.lookForward = EditorGUILayout.Toggle(new GUIContent("Look Forward", "If true, object will look towards its current velocity"), sPG.lookForward);

            if (EditorGUI.EndChangeCheck() && sPG.spline != null)
            {
                sPG.GeneratePrefabsAlongSpline();
            }
        }
    }
}
#endif
