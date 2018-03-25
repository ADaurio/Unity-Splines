# Unity Splines

To get started, attach a BezierSpline component to a GameObject.  You can find this under the Splines section of the component menu.

You will see four buttons in the Inspector:

* <b>Add control point</b> - Adds a control point to the end of your spline each time you left-click the scene view.  Left clicking your original point will turn your spline into a loop.
* <b>Insert control point</b> - Inserts a control point along an existing curve.  Requires two or more points.
* <b>Delete control point</b> - Deletes whatever control point you left-click on in the scene view.
* <b>Clear control points</b> - Deletes your entire spline, but leaves a bare component.

To edit control points or tangents, left click a handle in the scene view to access a move gizmo.  You can link or break tangents in the Inspector.  Dragging after adding or inserting a control point will alter tangents as well.


# Additional Features

## Align to Spline

Adding this component to an object snaps it to a spline.

* <b>Bezier Spline</b> - Spline to align your object to.
* <b>Style</b> - Normalized clamps the distance slider from 0-1.  Distance shows actual distance values.
* <b>Distance</b> - Specifies the distance along the spline the object should be placed at.
* <b>Speed</b> - Optional.  Specifies the speed at which this object moves along the spline in play mode.  0 holds the object in place.
* <b>Look Forward</b> - If true, rotates the object as it moves along the spline.  False preserves the object's initial rotation.

## Spline Prefab Generator

Adding this component to an object generates prefabs along a spline.

* <b>Bezier Spline</b> - Spline to spawn prefabs along.
* <b>Prefab</b> - Prefab to spawn along the spline.
* <b>Placement Style</b> - Distance generates a prefab every X distance along the spline.  Total places Y number of prefab instances spaced evenly along the spline.
* <b>Spacing</b> - Only works with the Distance placement style.  Determines the distance between generated prefab instances.
* <b>Instance Count</b> - Only works with the Total placement style.  Determines the number of prefab instances generated along the spline.
* <b>Look Forward</b> - If true, rotates the spawned prefabs as they move along the spline.
