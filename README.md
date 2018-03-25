# Unity Splines

To get started, attach a BezierSpline component to a GameObject.  You can find this under the Splines section of the component menu.

You will see four buttons in the Inspector:

<b>Add control point</b> - Adds a control point to the end of your spline each time you left click the scene view.
<b>Insert control point</b> - Inserts a control point along an existing curve.  Requires two or more points.
<b>Delete control point</b> - Deletes whatever control point you click on in the scene view.
<b>Clear control points</b> - Deletes your entire spline, but leaves a bare component.

To edit control points or tangents, left click a handle in the scene view to access a move gizmo.  You can link or break tangents in the Inspector.


<b>Additional Features</b>

<b>Align to Spline</b>

Adding this component to an object snaps it to a spline.

<b>Bezier Spline</b> - Spline to align your object to.
<b>Style</b> - Normalized clamps the distance slider from 0-1.  Distance shows actual distance values.
<b>Distance</b> - Specifies the distance along the spline the object should be placed at.
<b>Speed</b> - Optional.  Specifies the speed at which this object moves along the spline in play mode.  0 holds the object in place.
<b>Look Forward</b> - If true, rotates the object as it moves along the spline.  False preserves the object's initial rotation.

<b>Spline Prefab Generator</b>

Adding this component to an object generates prefabs along a spline.

<b>Bezier Spline</b> - Spline to spawn prefabs along.
<b>Prefab</b> - Prefab to spawn along the spline.
<b>Look Forward</b> - If true, rotates the spawned prefabs as they move along the spline.