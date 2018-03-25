using UnityEngine;

namespace Bezier
{
    public enum InterpStyle
    {
        Normalized,
        Distance
    }

    [AddComponentMenu("Splines/Align To Spline")]
    public class AlignToSpline : MonoBehaviour
    {
        /// <summary>
        /// Spline to snap to
        /// </summary>
        public BezierSpline spline;
        
        /// <summary>
        /// Is the specified distance a normalized value?
        /// </summary>
        public InterpStyle interpStyle;

        /// <summary>
        /// Distance along the spline
        /// </summary>
        public float distance;

        /// <summary>
        /// Determines the speed at which this object moves along the spline in play mode
        /// </summary>
        public float speed;

        /// <summary>
        /// If true, object will look towards its current velocity
        /// </summary>
        public bool lookForward;

        [SerializeField]
        private float currDist;

        /// <summary>
        /// Sets the distance variable to an appropriate value if slider is normalized.  Called from the custom editor.
        /// </summary>
        public void SetSplinePosition()
        {
            currDist = distance;

            if (interpStyle == InterpStyle.Normalized)
                currDist = distance * spline.GetTotalLength();

            MoveAlongSpline(currDist);
        }

        /// <summary>
        /// Snaps GameObject to spline at specified distance.  Rotates the object is LookForward is true.
        /// </summary>
        /// <param name="d">Distance along the spline.</param>
        private void MoveAlongSpline(float d)
        {
            transform.position = spline.GetPointOnSplineByDistance(d);

            if (lookForward)
            {
                transform.rotation = Quaternion.LookRotation(spline.GetDerivativeByDistance(d), Vector3.up);
            }
        }

        private void Update()
        {
            currDist = (currDist + speed * Time.deltaTime) % spline.GetTotalLength();
            MoveAlongSpline(currDist);
        }
    }
}
