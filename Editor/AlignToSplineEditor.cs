#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Bezier
{
    [CustomEditor(typeof(AlignToSpline))]
    public class AlignToSplineEditor : Editor
    {
        private AlignToSpline aTS;

        public void OnSceneGUI()
        {
            aTS = (AlignToSpline)target;

            if (aTS.spline != null)
                aTS.spline.RenderCurvesInSceneView();
        }

        public override void OnInspectorGUI()
        {
            aTS = (AlignToSpline)target;

            EditorGUI.BeginChangeCheck();

            aTS.spline = (BezierSpline)EditorGUILayout.ObjectField(new GUIContent("Bezier Spline", "Spline to snap to"), aTS.spline, typeof(BezierSpline), true);
            aTS.interpStyle = (InterpStyle)EditorGUILayout.EnumPopup(new GUIContent("Style", "Is the specified distance a normalized value?"), aTS.interpStyle);

            if (aTS.spline)
                aTS.distance = EditorGUILayout.Slider(new GUIContent("Distance", "Distance along the spline"), aTS.distance, 0, (aTS.interpStyle == InterpStyle.Distance) ? aTS.spline.GetTotalLength() : 1);

            aTS.speed = EditorGUILayout.FloatField(new GUIContent("Speed", "Determines the speed at which this object moves along the spline in play mode"), aTS.speed);
            aTS.lookForward = EditorGUILayout.Toggle(new GUIContent("Look Forward", "If true, object will look towards its current velocity"), aTS.lookForward);

            if (EditorGUI.EndChangeCheck() && aTS.spline != null)
            {
                aTS.SetSplinePosition();
            }
        }
    }
}
#endif
