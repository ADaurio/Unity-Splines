using UnityEngine;

namespace Bezier
{
    /// <summary>
    /// Serializable class wrapper for Vector3s.  Allows for null values.
    /// </summary>
    [System.Serializable]
    public class CurveHandle
    {
        public Vector3 point;

        public CurveHandle(Vector3 point_)
        {
            point = point_;
        }

        public static implicit operator bool(CurveHandle me)
        {
            return me != null;
        }

        public static implicit operator Vector3(CurveHandle me)
        {
            return me.point;
        }

        public override string ToString()
        {
            return point.ToString();
        }
    }
    
    /// <summary>
    /// Serializable class for Vector3s that allows for null values.  Contains reference for forward and backward tangents.
    /// </summary>
    [System.Serializable]
    public class ControlPoint : CurveHandle
    {
        public CurveHandle tangentBackward;
        public CurveHandle tangentForward;

        public ControlPoint(Vector3 point_) : base (point_) { }

        public static implicit operator bool(ControlPoint me)
        {
            return me != null;
        }

        public static implicit operator Vector3(ControlPoint me)
        {
            return me.point;
        }
    }
}
