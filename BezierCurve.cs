using System.Collections.Generic;
using UnityEngine;

namespace Bezier
{
    [System.Serializable]
    public class BezierCurve
    {
        /// <summary>
        /// The spline this curve belongs to.
        /// </summary>
        public BezierSpline spline;

        /// <summary>
        /// Indices of the control points this curve travels from and to.  Indices are used over ControlPoints to prevent C# serialization from brekaing reference connections.
        /// </summary>
        public int[] controlPointIndices;
        
        /// <summary>
        /// Constructs a curve that belongs to a specified spline.
        /// </summary>
        /// <param name="spline_">Spline that this curve belongs to.</param>
        /// <param name="cP1">Start point.</param>
        /// <param name="cP2">End point.</param>
        public BezierCurve(BezierSpline spline_, ControlPoint cP1, ControlPoint cP2)
        {
            spline = spline_;
            controlPointIndices = new int[2];
            controlPointIndices[0] = spline.GetControlPointIndex(cP1);
            controlPointIndices[1] = spline.GetControlPointIndex(cP2);
        }

        /// <summary>
        /// Constructs a curve that belongs to a specified spline.
        /// </summary>
        /// <param name="spline_">Spline that this curve belongs to.</param>
        /// <param name="cP1">Index of the start point.</param>
        /// <param name="cP2">Index of the end point.</param>
        public BezierCurve(BezierSpline spline_, int index1_, int index2_)
        {
            spline = spline_;
            controlPointIndices = new int[2];
            controlPointIndices[0] = index1_;
            controlPointIndices[1] = index2_;
        }

        private List<Vector3> GetAffectedPoints()
        {
            List<Vector3> retList = new List<Vector3>();
            retList.Add(GetStartPoint());

            CurveHandle t0 = GetStartPoint().tangentForward;
            CurveHandle t1 = GetEndPoint().tangentBackward;

            if (t0)
                retList.Add((Vector3)GetStartPoint() + t0);

            if (t1)
                retList.Add((Vector3)GetEndPoint() + t1);

            retList.Add(GetEndPoint());
            return retList;
        }

        /// <summary>
        /// Estimates the point at dist along the curve.
        /// </summary>
        /// <param name="dist">Distance along the curve.</param>
        /// <returns>Returns the point at dist along the curve.</returns>
        public Vector3 FindPointAlongCurveByDistance(float dist)
        {
            float t = GetTFromDistance(dist);
            return FindPointAlongCurve(t);
        }

        /// <summary>
        /// Estimates the point at normalized dist along the curve.
        /// </summary>
        /// <param name="t">Normalized distance along the curve.</param>
        /// <returns>Returns the point at dist along the curve.</returns>
        public Vector3 FindPointAlongCurve(float t)
        {
            t = Mathf.Clamp(t, 0, 1);
            float tSquared = t * t;
            float tCubed = tSquared * t;
            float inverseT = 1 - t;
            float inverseTSquared = inverseT * inverseT;
            float inverseTCubed = inverseTSquared * inverseT;
            Vector3 retPoint = Vector3.zero;
            List<Vector3> points = GetAffectedPoints();

            switch (points.Count)
            {
                case 2:
                    retPoint = inverseT * points[0] + t * points[1];
                    break;
                case 3:
                    retPoint = (inverseTSquared * points[0]) + (2 * inverseT * t * points[1]) + (tSquared * points[2]);
                    break;
                case 4:
                    retPoint = (inverseTCubed * points[0]) + (3 * inverseTSquared * t * points[1]) + (3 * inverseT * tSquared * points[2]) + (tCubed * points[3]);
                    break;
            }

            return retPoint;
        }

        /// <summary>
        /// Estimates the length of this curve.
        /// </summary>
        /// <returns>Returns the length of this curve.</returns>
        public float GetLength()
        {
            List<Vector3> points = GetAffectedPoints();

            if (points.Count == 2)
            {
                return Vector3.Distance(points[0], points[1]);
            }

            float increment = (float)1 / BezierSpline.lineSteps;
            Vector3 point = points[0];
            float t = 0;
            float length = 0;

            for (int i = 0; i < BezierSpline.lineSteps; i++)
            {
                t += increment;
                Vector3 newPoint = FindPointAlongCurve(t);
                length += Vector2.Distance(point, newPoint);
                point = newPoint;
            }

            return length;
        }

        /// <summary>
        /// Gets the start ControlPoint of this curve.
        /// </summary>
        /// <returns>Returns the start ControlPoint of this curve.</returns>
        public ControlPoint GetStartPoint()
        {
            return spline.GetControlPoint(controlPointIndices[0]);
        }

        /// <summary>
        /// Gets the end ControlPoint of this curve.
        /// </summary>
        /// <returns>Returns the end ControlPoint of this curve.</returns>
        public ControlPoint GetEndPoint()
        {
            return spline.GetControlPoint(controlPointIndices[1]);
        }

        /// <summary>
        /// Normalizes a distance along this curve.
        /// </summary>
        /// <param name="dist">Initial distance.</param>
        /// <returns>Returns normalized distance (0-1).</returns>
        private float GetTFromDistance(float dist)
        {
            return Mathf.Clamp(dist / GetLength(), 0, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dist">Distance along the curve.</param>
        /// <returns>Returns derivative at dist along the curve.</returns>
        public Vector3 GetDerivativeFromDistance(float dist)
        {
            float t = GetTFromDistance(dist);
            return GetDerivative(t);
        }
        
        /// <summary>
        /// Estimates the velocity on the curve at a specified distance.  Used when neither the start or end point have tangents.
        /// </summary>
        /// <param name="t">Normalized value from 0-1.</param>
        /// <returns>Returns the velocity on the curve.</returns>
        public Vector3 GetDerivative(float t)
        {
            List<Vector3> points = GetAffectedPoints();

            switch (points.Count)
            {
                case 2:
                    return points[1] - points[0];
                case 3:
                    return GetQuadraticDerivative(t);
                case 4:
                    return GetCubicDerivative(t);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Estimates the velocity on the curve at a specified distance.  Used either the beginning or end point have a tangent, but not both.
        /// </summary>
        /// <param name="t">Normalized value from 0-1.</param>
        /// <returns>Returns the velocity on the curve.</returns>
        private Vector3 GetQuadraticDerivative(float t)
        {
            List<Vector3> points = GetAffectedPoints();
            float inverseT = 1 - t;
            float part1 = 2 * inverseT;
            float part2 = 2 * t;
            Vector3 p1p0 = points[1] - points[0];
            Vector3 p2p1 = points[2] - points[1];
            return (part1 * p1p0) + (part2 * p2p1);
        }

        /// <summary>
        /// Estimates the velocity on the curve at a specified distance.  Used when beginning and end points both have tangents.
        /// </summary>
        /// <param name="t">Normalized value from 0-1.</param>
        /// <returns>Returns the velocity on the curve.</returns>
        private Vector3 GetCubicDerivative(float t)
        {
            List<Vector3> points = GetAffectedPoints();
            float inverseT = 1 - t;
            float part1 = 3 * (inverseT * inverseT);
            float part2 = 6 * inverseT * t;
            float part3 = 3 * (t * t);
            Vector3 p1p0 = points[1] - points[0];
            Vector3 p2p1 = points[2] - points[1];
            Vector3 p3p2 = points[3] - points[2];
            return (part1 * p1p0) + (part2 * p2p1) + (part3 * p3p2);
        }

        public static implicit operator bool(BezierCurve me)
        {
            return me != null;
        }
    }
}