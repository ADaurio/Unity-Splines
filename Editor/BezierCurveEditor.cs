#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Bezier
{
    [CustomEditor(typeof(BezierSpline))]
    public class BezierCurveEditor : Editor
    {
        private static bool addPointMode;
        private static bool insertPointMode;
        private static bool deletePointMode;

        private int selectedIndex = -1;
        private BezierSpline spline;
        private Transform handleTransform;
        private Quaternion handleRotation;
        private Tool lastTool = Tool.None;
        private ControlPoint lastPoint;
        
        void OnDisable()
        {
            ResetTools();
            addPointMode = false;
            deletePointMode = false;
        }

        private void HideTools()
        {
            if (Tools.current == Tool.None) return;

            lastTool = Tools.current;
            Tools.current = Tool.None;
        }

        private void ResetTools()
        {
            selectedIndex = -1;
            Tools.current = lastTool;
        }

        private void GetCurveAndTransform()
        {
            spline = (BezierSpline)target;
            handleTransform = spline.transform;

            handleRotation = Tools.pivotRotation == PivotRotation.Local ?
                handleTransform.rotation : Quaternion.identity;
        }

        public override void OnInspectorGUI()
        {
            GetCurveAndTransform();

            spline.tangentMode = (TangentMode)EditorGUILayout.EnumPopup("Tangent Mode", spline.tangentMode);

            Color oColour = GUI.backgroundColor;
            Color selectedColour = hexToColor("4e9ac3");

            EditorGUILayout.BeginHorizontal();

            #region Add Button
            if (addPointMode && !spline.IsLoop())
                GUI.backgroundColor = selectedColour;

            if (spline.IsLoop())
                GUI.enabled = false;

            if (GUILayout.Button(new GUIContent("Add Control Point", "Click to begin adding control points to the end of your spline")))
            {
                addPointMode = !addPointMode;
                insertPointMode = false;
                deletePointMode = false;

                if (addPointMode)
                    HideTools();
                else
                    ResetTools();

                SceneView.RepaintAll();
            }

            if (spline.IsLoop())
                GUI.enabled = true;
            #endregion

            #region Insert Button
            GUI.backgroundColor = oColour;

            if (insertPointMode)
                GUI.backgroundColor = selectedColour;

            if (GUILayout.Button(new GUIContent("Insert Control Point", "Click to begin adding control points to your existing spline")))
            {
                addPointMode = false;
                insertPointMode = !insertPointMode;
                deletePointMode = false;

                if (insertPointMode)
                    HideTools();
                else
                    ResetTools();

                SceneView.RepaintAll();
            }
            #endregion

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            #region Delete Button
            GUI.backgroundColor = oColour;

            if (deletePointMode)
                GUI.backgroundColor = selectedColour;

            if (GUILayout.Button("Delete Control Points"))
            {
                addPointMode = false;
                insertPointMode = false;
                deletePointMode = !deletePointMode;

                if (deletePointMode)
                    HideTools();
                else
                    ResetTools();

                SceneView.RepaintAll();
            }
            #endregion

            #region Clear Button
            GUI.backgroundColor = oColour;

            if (GUILayout.Button(new GUIContent("Clear Control Points", "Clears all spline data")))
            {
                addPointMode = false;
                insertPointMode = false;
                deletePointMode = false;
                spline.Reset();
                ResetTools();
                SceneView.RepaintAll();
            }
            #endregion

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            string helpString = "Control Points: " + spline.GetControlPointCount().ToString() + " | Looping: " + ((spline.IsLoop()) ? "Yes" : "No");
            EditorGUILayout.HelpBox(helpString, MessageType.Info);
        }

        private void OnSceneGUI()
        {
            GetCurveAndTransform();
            Handles.BeginGUI();
            GUIStyle sceneLabelStyle = new GUIStyle();
            sceneLabelStyle.normal.textColor = Color.white;
            sceneLabelStyle.alignment = TextAnchor.UpperCenter;

            #region Scene View Labels
            if (addPointMode)
                GUI.Label(new Rect(Screen.width * 0.5f - 125, 10, 250, 30), "Left click to add a point", sceneLabelStyle);
            if (insertPointMode)
                GUI.Label(new Rect(Screen.width * 0.5f - 125, 10, 250, 30), "Left click to place a point", sceneLabelStyle);
            if (selectedIndex > -1)
                GUI.Label(new Rect(Screen.width * 0.5f - 125, 10, 250, 30), "Left click a point to move it", sceneLabelStyle);
            if (deletePointMode)
                GUI.Label(new Rect(Screen.width * 0.5f - 125, 10, 250, 30), "Left click a point to delete it", sceneLabelStyle);
            Handles.EndGUI();
            #endregion

            #region Intercept Clicks
            if ((addPointMode || insertPointMode || deletePointMode) && Event.current.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(0);
            }
            #endregion

            #region Add/Insert Control Points
            if ((addPointMode || insertPointMode) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                Plane plane = new Plane(-Camera.current.transform.forward, handleTransform.position);
                float enter = 0.0f;

                if (addPointMode && plane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = handleTransform.InverseTransformPoint(ray.GetPoint(enter));

                    Undo.RecordObject(spline, "Add Control Point");
                    EditorUtility.SetDirty(spline);
                    lastPoint = new ControlPoint(hitPoint);
                    
                    if (spline.GetControlPointCount() > 0 && ((Vector3)spline.GetControlPoint(0) - hitPoint).sqrMagnitude < 0.5f)
                        lastPoint = spline.GetControlPoint(0);

                    spline.AddControlPoint(lastPoint);
                }
                else if (insertPointMode)
                {
                    float t;
                    float dist = -1;

                    if (spline.GetClosestDistanceToRay(ray, out dist))
                    {
                        Undo.RecordObject(spline, "Insert Control Point");
                        EditorUtility.SetDirty(spline);
                        lastPoint = new ControlPoint(spline.GetPointOnSplineByDistance(dist));
                        BezierCurve curve = spline.GetCurveFromDistance(dist, out t);
                        spline.InsertControlPoint(lastPoint, curve);
                    }
                }

                Event.current.Use();
            }

            if ((addPointMode || insertPointMode) && lastPoint != null && Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                Plane plane = new Plane(-Camera.current.transform.forward, lastPoint);
                float enter = 0.0f;

                if (plane.Raycast(ray, out enter))
                {
                    Vector3 hitPoint = handleTransform.InverseTransformPoint(ray.GetPoint(enter));
                    lastPoint.tangentForward = new CurveHandle(hitPoint - lastPoint);
                    lastPoint.tangentBackward = new CurveHandle(lastPoint - hitPoint);
                }

                Event.current.Use();
            }

            if ((addPointMode || insertPointMode) && Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                Undo.RecordObject(spline, "Move Tangent");
                EditorUtility.SetDirty(spline);
                lastPoint = null;
                Event.current.Use();
            }
            #endregion

            spline.RenderCurvesInSceneView(!deletePointMode);
            RenderHandles();
        }

        /// <summary>
        /// Renders all handles and tangents when applicable.
        /// </summary>
        private void RenderHandles()
        {
            int pointCount = spline.GetControlPointCount();
            int index = 0;
            Handles.color = Color.white;

            for (int i = pointCount - 1; i >= 0; i--)
            {
                ControlPoint p = spline.GetControlPoint(i);
                ShowPoint(p, index);
                index++;
            }

            int segCount = spline.GetCurveCount();

            if (segCount <= 0) return;

            for (int i = segCount - 1; i >= 0; i--)
            {
                BezierCurve seg = spline.GetCurve(i);
                Handles.color = Color.green;
                
                //Display Tangents
                if (!deletePointMode)
                {
                    if (seg.GetStartPoint().tangentForward)
                    {
                        ShowTangentPoint(seg.GetStartPoint(), seg.GetStartPoint().tangentForward, index);
                        index++;
                    }

                    if (seg.GetEndPoint().tangentBackward)
                    {
                        ShowTangentPoint(seg.GetEndPoint(), seg.GetEndPoint().tangentBackward, index);
                        index++;
                    }
                }
            }
        }

        /// <summary>
        /// Displays a ControlPoint in the Scene View.  Also controls ControlPoint's movement.
        /// </summary>
        /// <param name="p">The affected ControlPoint.</param>
        /// <param name="controlIndex">Control index of this handle.</param>
        private void ShowPoint(ControlPoint p, int controlIndex)
        {
            Vector3 point = handleTransform.TransformPoint(p.point);
            float size = HandleUtility.GetHandleSize(point) * 0.05f;

            if (Handles.Button(point, handleRotation, size, (addPointMode) ? 0 : (size * 1.5f), Handles.DotHandleCap))
            {
                if (deletePointMode)
                {
                    Undo.RecordObject(spline, "Add Control Point");
                    EditorUtility.SetDirty(spline);
                    spline.RemoveControlPoint(p);
                }
                else if (!addPointMode)
                {
                    selectedIndex = controlIndex;
                }
            }

            if (selectedIndex == controlIndex)
            {
                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point, handleRotation);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spline, "Move Control Point");
                    EditorUtility.SetDirty(spline);
                    p.point = handleTransform.InverseTransformPoint(point);
                }
            }
        }

        /// <summary>
        /// Displays a tangent point in the Scene View.  Also controls tangent point movement.
        /// </summary>
        /// <param name="s">ControlPoint the tangent is tied to.</param>
        /// <param name="p">CurvePoint representing the tangent.</param>
        /// <param name="controlIndex">Control index of this handle.</param>
        private void ShowTangentPoint(ControlPoint s, CurveHandle p, int controlIndex)
        {
            Vector3 point = handleTransform.TransformPoint(p.point + s.point);
            float size = HandleUtility.GetHandleSize(point) * 0.03f;

            if (Handles.Button(point, handleRotation, size, size, Handles.DotHandleCap))
            {
                selectedIndex = controlIndex;
            }

            if (selectedIndex == controlIndex)
            {
                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point, handleRotation);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(spline, "Move Tangent");
                    EditorUtility.SetDirty(spline);
                    p.point = handleTransform.InverseTransformPoint(point - s.point);

                    if (spline.tangentMode == TangentMode.Link)
                    {
                        if (p == s.tangentBackward && s.tangentForward != null)
                        {
                            s.tangentForward.point = -p.point;
                            Debug.Log("Aligning");
                        }
                        else if (p == s.tangentForward && s.tangentBackward != null)
                        {
                            s.tangentBackward.point = -p.point;
                            Debug.Log("Aligning");
                        }
                    }
                }
            }
        }

        private static Color hexToColor(string hex)
        {
            //Source: https://answers.unity.com/questions/812240/convert-hex-int-to-colorcolor32.html
            hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
            byte a = 255;//assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color32(r, g, b, a);
        }
    }
}
#endif