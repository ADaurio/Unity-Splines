using System.Collections.Generic;
using UnityEngine;

namespace Bezier
{    
    public enum TangentMode
    {
        Link,
        Break
    }

    [AddComponentMenu("Splines/Bezier Spline")]
    public class BezierSpline : MonoBehaviour
    {
        /// <summary>
        /// Number of 'steps' to break curves into for mathematic claculations and visuals.
        /// </summary>
        public const int lineSteps = 10;

        /// <summary>
        /// Determines whether tangents should be locked or broken when editing.
        /// </summary>
        [SerializeField]
        public TangentMode tangentMode = TangentMode.Link;

        /// <summary>
        /// Becomes true when the spline becomes cyclical.  False otherwise.
        /// </summary>
        [SerializeField]
        private bool looped;

        /// <summary>
        /// List of Curves contained within the spline.
        /// </summary>
        [SerializeField]
        private List<BezierCurve> curves = new List<BezierCurve>();

        /// <summary>
        /// List of ControlPoints that make up the spline.
        /// </summary>
        [SerializeField]
        private List<ControlPoint> controlPoints = new List<ControlPoint>();
        
        /// <summary>
        /// Resets the spline.  Deletes all ControlPoints and Curves.
        /// </summary>
        public void Reset()
        {
            tangentMode = TangentMode.Link;
            looped = false;
            curves = new List<BezierCurve>();
            controlPoints = new List<ControlPoint>();
        }

        /// <summary>
        /// Add a ControlPoint to the end of the spline.  Spline becomes cyclical if you pass its first ControlPoint back in.
        /// </summary>
        /// <param name="point">Point to add.</param>
        public void AddControlPoint(ControlPoint point)
        {
            if (controlPoints.Count > 1)
            {
                Vector3 dir1 = (Vector3)controlPoints[controlPoints.Count - 1] - point;
                Vector3 dir2 = ((Vector3)controlPoints[controlPoints.Count - 2] - point).normalized * dir1.magnitude * 0.5f;

                ControlPoint last = controlPoints[controlPoints.Count - 1];

                if (last.tangentBackward)
                {
                    last.tangentForward = new CurveHandle(-last.tangentBackward.point);
                }
                else
                {
                    last.tangentBackward = new CurveHandle(dir2);
                    last.tangentForward = new CurveHandle(-dir2);
                }

                if (controlPoints[0] == point)
                {
                    looped = true;
                    ControlPoint next = controlPoints[1];
                    Vector3 dir3 = ((Vector3)last - next) * 0.25f;
                    point.tangentBackward = new CurveHandle(dir3);
                    point.tangentForward = new CurveHandle(-dir3);
                }

                if (controlPoints.Count == 0 || controlPoints[0] != point)
                    controlPoints.Add(point);

                BezierCurve newSegment = new BezierCurve(this, last, point);
                curves.Add(newSegment);
            }
            else
            {
                if (controlPoints.Count == 0 || controlPoints[0] != point)
                    controlPoints.Add(point);

                if (controlPoints.Count > 1)
                {
                    BezierCurve newSegment = new BezierCurve(this, 0, 1);
                    curves.Add(newSegment);
                }
            }
        }

        /// <summary>
        /// Removes a ControlPoint from the spline.
        /// </summary>
        /// <param name="point">Point to remove.</param>
        public void RemoveControlPoint(ControlPoint point)
        {
            if (!controlPoints.Contains(point)) return;

            int index = controlPoints.IndexOf(point);

            for (int i = 0; i < curves.Count; i++)
            {
                if (curves[i].controlPointIndices[0] >= index)
                    curves[i].controlPointIndices[0]--;
                if (curves[i].controlPointIndices[1] >= index)
                    curves[i].controlPointIndices[1]--;
            }

            if (index == 0 && !looped)
            {
                curves.RemoveAt(0);
                controlPoints.RemoveAt(0);
            }
            else if (index + 1 == controlPoints.Count && !looped)
            {
                controlPoints.RemoveAt(controlPoints.Count - 1);
                curves.RemoveAt(curves.Count - 1);
            }
            else
            {
                int prevControlIndex = (index - 1 + controlPoints.Count) % controlPoints.Count;
                ControlPoint last = controlPoints[prevControlIndex];
                ControlPoint next = controlPoints[(index + 1) % controlPoints.Count];
                controlPoints.Remove(point);

                int prevCurveIndex = (index - 1 + curves.Count) % curves.Count;
                BezierCurve newSegment = new BezierCurve(this, last, next);
                curves[prevCurveIndex] = newSegment;
                curves.RemoveAt(index);
            }
        }

        /// <summary>
        /// Splits a Curve and adds a new ControlPoint.
        /// </summary>
        /// <param name="point">Point to insert.</param>
        /// <param name="curve">Curve to be split.</param>
        public void InsertControlPoint(ControlPoint point, BezierCurve curve)
        {
            int index = curves.IndexOf(curve);
            ControlPoint last = controlPoints[index];
            ControlPoint next = controlPoints[index + 1];

            controlPoints.Insert(index + 1, point);
            curves[index] = new BezierCurve(this, point, next);
            BezierCurve newCurve = new BezierCurve(this, last, point);
            curves.Insert(index, newCurve);

            if (index + 2 >= curves.Count) return;

            for (int i = index + 2; i < curves.Count; i++)
            {
                curves[i].controlPointIndices[0]++;
                curves[i].controlPointIndices[1]++;
            }
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Renders this spline's Curves into the Scene View.  Should only be called from OnSceneGUI.
        /// </summary>
        /// <param name="displayTangents">If true, also renders tangent lines.</param>
        public void RenderCurvesInSceneView(bool displayTangents = false)
        {
            int segCount = GetCurveCount();

            if (segCount <= 0) return;

            for (int i = segCount - 1; i >= 0; i--)
            {
                BezierCurve seg = GetCurve(i);
                DisplayCurveInSceneView(seg);

                UnityEditor.Handles.color = Color.green;

                //Display Tangents
                if (displayTangents)
                {
                    if (seg.GetStartPoint().tangentForward)
                    {
                        Vector3 p0 = transform.TransformPoint(seg.GetStartPoint().point);
                        Vector3 p1 = p0 + transform.TransformVector(seg.GetStartPoint().tangentForward.point);
                        UnityEditor.Handles.DrawLine(p0, p1);
                    }
                    if (seg.GetEndPoint().tangentBackward)
                    {
                        Vector3 p0 = transform.TransformPoint(seg.GetEndPoint().point);
                        Vector3 p1 = p0 + transform.TransformVector(seg.GetEndPoint().tangentBackward.point);
                        UnityEditor.Handles.DrawLine(p0, p1);
                    }
                }
            }
        }

        /// <summary>
        /// Renders a Curve to the Scene View.  Should only be called from OnSceneGUI.
        /// </summary>
        /// <param name="curve">Curve to render.</param>
        private void DisplayCurveInSceneView(BezierCurve curve)
        {
            Vector3 lineStart = transform.TransformPoint(curve.FindPointAlongCurve(0));
            UnityEditor.Handles.color = Color.white;

            for (int i = 1; i <= lineSteps; i++)
            {
                Vector3 lineEnd = transform.TransformPoint(curve.FindPointAlongCurve((float)i / lineSteps));
                UnityEditor.Handles.DrawLine(lineStart, lineEnd);
                lineStart = lineEnd;
            }
        }
        #endif

        #region Getters
        /// <summary>
        /// Gets a ControlPoint by index.
        /// </summary>
        /// <param name="index">Index of the ControlPoint.</param>
        /// <returns>Returns the ControlPoint at [index].</returns>
        public ControlPoint GetControlPoint(int index)
        {
            return controlPoints[index];
        }

        /// <summary>
        /// Gets the index of a ControlPoint.
        /// </summary>
        /// <param name="cP">ControlPoint to find the index of.</param>
        /// <returns>Returns the index of [cP].</returns>
        public int GetControlPointIndex(ControlPoint cP)
        {
            return controlPoints.IndexOf(cP);
        }

        /// <summary>
        /// Gets the number of ControlPoints in the spline.
        /// </summary>
        /// <returns>Returns the number of ControlPoints in the spline.</returns>
        public int GetControlPointCount()
        {
            return controlPoints.Count;
        }

        /// <summary>
        /// Gets a Curve from the spline by index.
        /// </summary>
        /// <param name="index">Index of the Curve.</param>
        /// <returns>Returns the Curve at [index].</returns>
        public BezierCurve GetCurve(int index)
        {
            return curves[index];
        }

        /// <summary>
        /// Gets the number of Curves in the spline.
        /// </summary>
        /// <returns>Returns the number of Curves in the spline.</returns>
        public int GetCurveCount()
        {
            return curves.Count;
        }

        /// <summary>
        /// Gets the Curve affecting the spline at a specified distance.
        /// </summary>
        /// <param name="dist">Distance to check.</param>
        /// <param name="t">A normalized float representing the start of the returned Curve.</param>
        /// <returns>Returns the Curve at dist, which begins at t.</returns>
        public BezierCurve GetCurveFromDistance(float dist, out float t)
        {
            float d = dist;

            for(int i = 0; i < curves.Count; i++)
            {
                float length = curves[i].GetLength();

                if (length < d)
                {
                    d -= length;
                }
                else
                {
                    t = d / length;
                    return curves[i];
                }
            }

            t = 1;
            return curves[curves.Count - 1];
        }

        /// <summary>
        /// Gets the point on the spline at a specified distance from its start point.
        /// </summary>
        /// <param name="dist">Distance from spline's start point.</param>
        /// <returns>Returns the point along spline at dist.</returns>
        public Vector3 GetPointOnSplineByDistance(float dist)
        {
            float t = 0;
            dist = dist % GetTotalLength();
            BezierCurve seg = GetCurveFromDistance(dist, out t);
            return transform.TransformPoint(seg.FindPointAlongCurve(t));
        }

        /// <summary>
        /// Gets the point along a Curve with a normalized distance. 
        /// </summary>
        /// <param name="t">Normalized distance from 0-1.</param>
        /// <param name="curveIndex">Index of the Curve.</param>
        /// <returns>Returns point along Curve at normalized distance t</returns>
        public Vector3 GetPointOnCurve(float t, int curveIndex)
        {
            return transform.TransformPoint(curves[curveIndex].FindPointAlongCurve(t));
        }

        /// <summary>
        /// Gets the length of a specific Curve.
        /// </summary>
        /// <param name="index">Index of the Curve to check.</param>
        /// <returns>Returns the length of the Curve at [index].</returns>
        public float GetLengthOfCurve(int index)
        {
            return curves[index].GetLength();
        }

        /// <summary>
        /// Gets the total length of the spline.
        /// </summary>
        /// <returns>Returns the total length of the spline.</returns>
        public float GetTotalLength()
        {
            float length = 0;

            foreach (BezierCurve s in curves)
            {
                length += s.GetLength();
            }

            return length;
        }

        /// <summary>
        /// Gets the derivative/velocity of the spline at a specified distance.
        /// </summary>
        /// <param name="dist">Distance along the spline.</param>
        /// <returns>Returns the derivative/velocity of the spline at a specified distance.</returns>
        public Vector2 GetDerivativeByDistance(float dist)
        {
            float t = 0;
            BezierCurve seg = GetCurveFromDistance(dist, out t);
            return seg.GetDerivative(t);
        }

        /// <summary>
        /// Gets the derivative/velocity of a Curve at a specified distance.
        /// </summary>
        /// <param name="t">Normalized distance along Curve.</param>
        /// <param name="curveIndex">Index of the Curve to check.</param>
        /// <returns>Returns the derivative/velocity of a Curve at a specified distance.</returns>
        public Vector3 GetDerivativeFromCurve(float t, int curveIndex)
        {
            return curves[curveIndex].GetDerivative(t);
        }

        /// <summary>
        /// Estimates the closest distance between the spline and a point. Returns true if the closest distance is within a specified threshold. 
        /// </summary>
        /// <param name="point">Point to check.</param>
        /// <param name="distanceAlongSpline">Returned closest distance between the point and the spline.</param>
        /// <param name="threshold">Determines a pass/fail distance.</param>
        /// <returns>Returns true if distance is within the threshold.</returns>
        public bool GetClosestDistanceToPoint(Vector3 point, out float distanceAlongSpline, float threshold = 0.2f)
        {
            distanceAlongSpline = -1;

            if (controlPoints.Count == 1) return false;

            if (controlPoints.Count <= 0)
            {
                Debug.LogError("Spline contains no points or curves");
                return false;
            }

            float increment = 0.1f;
            float dist = 0;
            float lowest = Mathf.Infinity;

            while (dist < GetTotalLength() + increment)
            {
                Vector3 p = GetPointOnSplineByDistance(dist);
                float diff = (point - p).sqrMagnitude;

                if (diff < lowest)
                {
                    distanceAlongSpline = dist;
                    lowest = diff;
                }

                dist += increment;
            }

            return lowest <= threshold;
        }

        /// <summary>
        /// Estimates the closest distance between the spline and a ray. Returns true if the closest distance is within a specified threshold. 
        /// </summary>
        /// <param name="ray">Ray to check.</param>
        /// <param name="distanceAlongSpline">Returned closest distance between the point and the spline.</param>
        /// <param name="threshold">Determines a pass/fail distance.</param>
        /// <returns>Returns true if distance is within the threshold.</returns>
        public bool GetClosestDistanceToRay(Ray ray, out float distanceAlongSpline, float threshold = 0.2f)
        {
            distanceAlongSpline = -1;

            if (controlPoints.Count == 1) return false;

            if (controlPoints.Count <= 0)
            {
                Debug.LogError("Spline contains no points or curves");
                return false;
            }

            float increment = 0.1f;
            float dist = 0;
            float lowest = Mathf.Infinity;

            while (dist < GetTotalLength() + increment)
            {
                Vector3 p = GetPointOnSplineByDistance(dist);
                Vector3 rayToP = (p - ray.origin);
                float angle = Mathf.Deg2Rad * Vector3.Angle(rayToP.normalized, ray.direction);
                float diff = Mathf.Sin(angle) * rayToP.magnitude;

                if (diff < lowest)
                {
                    distanceAlongSpline = dist;
                    lowest = diff;
                }

                dist += increment;
            }

            return lowest <= threshold;
        }

        /// <summary>
        /// Checks whether the spline is cyclical or not.
        /// </summary>
        /// <returns>Returns true is spline is cycical.  False otherwise.</returns>
        public bool IsLoop()
        {
            return looped;
        }
        #endregion
    }
}