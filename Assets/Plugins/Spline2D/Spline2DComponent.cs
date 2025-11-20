using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Utility class for displaying a Spline2D in a scene as a component
// All the real work is done by Spline2D, this just makes is serialize and render
// editable gizmos (also see Spline2DInspector)
public class Spline2DComponent : MonoBehaviour
{
    private Spline2D _spline;

    [Tooltip("Display in the XZ plane in the editor instead of the default XY plane (spline is still in XY)")]
    public bool displayXZ;

    private void InitSpline()
    {
        _spline ??= new Spline2D(points, closed, curvature, lengthSamplesPerSegment);
    }

    // All state is duplicated so it can be correctly serialized, Unity never
    // calls getters/setters on load/save so they're useless, we have to store
    // actual fields here. In the editor, a custom inspector is used to ensure
    // the property setters are called to sync the underlying Spline2D

    // Points which the curve passes through.
    [SerializeField]
    private List<Vector2> points = new();
    
    [SerializeField]
    private bool closed;
    /// Whether the spline is closed; if so, the first point is also the last
    public bool isClosed
    {
        get => closed;
        set {
            closed = value;
            InitSpline();
            _spline.isClosed = closed;
        }
    }
    
    [SerializeField]
    private float curvature = 0.5f;
    /// The amount of curvature in the spline; 0.5 is Catmull-Rom
    public float Curvature
    {
        get => curvature;
        set {
            curvature = value;
            InitSpline();
            _spline.curvature = curvature;
        }
    }
    
    [SerializeField]
    private int lengthSamplesPerSegment = 5;
    /// Accuracy of sampling curve to traverse by distance
    public int LengthSamplesPerSegment 
    {
        get => lengthSamplesPerSegment;
        set {
            lengthSamplesPerSegment = value;
            InitSpline();
            _spline.lengthSamplesPerSegment = lengthSamplesPerSegment;
        }
    }

    // For gizmo drawing
	private const int StepsPerSegment = 20;
    public bool showNormals;
    public float normalDisplayLength = 5.0f;
	public bool showDistance;
	public float distanceMarker = 1.0f;


    /// Get point count
    public int count => points.Count;

    /// Return the approximate length of the curve, as derived by sampling the
    /// curve at a resolution of LengthSamplesPerSegment
    public float length
    {
        get 
        {
            InitSpline();
            return _spline.length;
        }
    }

    /// Add a point to the curve (local 2D space)
    public void AddPoint(Vector2 point)
    {
        // We share the same list so adding there adds here
        InitSpline();
        _spline.AddPoint(point);
    }

    /// Add a point to the curve based on a world space position
    /// If point is off the plane of the spline it will be projected back on to it
    public void AddPointWorldSpace(Vector3 point) 
    {
        var localPoint = transform.InverseTransformPoint(point);
        if (displayXZ)
        {
            localPoint = FlipXZtoXY(localPoint);
        }
        
        AddPoint(localPoint);
    }

    /// Change a point on the curve (local 2D space)
    public void SetPoint(int index, Vector2 point)
    {
        // We share the same list so changing there adds here
        InitSpline();
        _spline.SetPoint(index, point);
    }
    
    /// Change a point on the curve based on a world space position
    /// If point is off the plane of the spline it will be projected back on to it
    public void SetPointWorldSpace(int index, Vector3 position) 
    {
        var localPoint = transform.InverseTransformPoint(position);
        if (displayXZ)
        {
            localPoint = FlipXZtoXY(localPoint);
        }
        
        SetPoint(index, localPoint);
    }

    /// Insert a point before the given index, in local 2D space
    public void InsertPoint(int index, Vector2 position)
    {
        // We share the same list so adding there adds here
        InitSpline();
        _spline.InsertPoint(index, position);
    }

    /// Insert a point before the given index, in world space
    /// If point is off the plane of the spline it will be projected back on to it
    public void InsertPointWorldSpace(int index, Vector3 position)
    {
        var localP = transform.InverseTransformPoint(position);
        if (displayXZ)
        {
            localP = FlipXZtoXY(localP);
        }
        InsertPoint(index, localP);
    }

    // Remove a point on the curve
    public void RemovePoint(int index) 
    {
        // We share the same list so changing there adds here
        InitSpline();
        _spline.RemovePoint(index);
    }

    // TODO add more efficient 'scrolling' curve of N length where we add one &
    // drop the earliest for efficient non-closed curves that continuously extend
    /// Reset and start again
    public void Clear() 
    {
        // We share the same list so changing there adds here
        InitSpline();
        _spline.Clear();
    }
    
    /// Get a single point in local 2D space
    public Vector2 GetPoint(int index)
    {
        InitSpline();
        return _spline.GetPoint(index);
    }
    
    /// Get a single point in world space
    public Vector3 GetPointWorldSpace(int index)
    {
        Vector3 p = GetPoint(index);
        if (displayXZ)
        {
            p = FlipXYtoXZ(p);
        }
        return transform.TransformPoint(p);
    }

    /// Interpolate a position on the entire curve in local 2D space. Note that if the control
    /// points are not evenly spaced, this may result in varying speeds.
    public Vector2 Interpolate(float t)
    {
        InitSpline();
        return _spline.Interpolate(t);
    }

    /// Interpolate a position on the entire curve in world space. Note that if the control
    /// points are not evenly spaced, this may result in varying speeds.
    public Vector3 InterpolateWorldSpace(float alpha)
    {
        Vector3 point = Interpolate(alpha);
        if (displayXZ)
        {
            point = FlipXYtoXZ(point);
        }
        return transform.TransformPoint(point);
    }

    /// Interpolate a position between one point on the curve and the next
    /// in local 2D space.
    /// Rather than interpolating over the entire curve, this simply interpolates
    /// between the point with fromIndex and the next point
    public Vector2 Interpolate(int fromIndex, float alpha)
    {
        InitSpline();
        return _spline.Interpolate(fromIndex, alpha);
    }

    /// Interpolate a position between one point on the curve and the next
    /// in world space.
    /// Rather than interpolating over the entire curve, this simply interpolates
    /// between the point with fromIndex and the next point
    public Vector3 InterpolateWorldSpace(int fromIndex, float alpha) 
    {
        Vector3 p = Interpolate(fromIndex, alpha);
        if (displayXZ)
            p = FlipXYtoXZ(p);
        return transform.TransformPoint(p);
    }

    /// Get derivative of the curve at a point in local 2D space. Note that if the control
    /// points are not evenly spaced, this may result in varying speeds.
    /// This is not normalised by default in case you don't need that
    public Vector2 Derivative(float alpha) 
    {
        InitSpline();
        return _spline.Derivative(alpha);
    }

    /// Get derivative of the curve at a point in world space. Note that if the control
    /// points are not evenly spaced, this may result in varying speeds.
    /// This is not normalized by default in case you don't need that
    public Vector3 DerivativeWorldSpace(float alpha) 
    {
        Vector3 derivative = Derivative(alpha);
        if (displayXZ)
        {
            derivative = FlipXYtoXZ(derivative);
        }
        return transform.TransformDirection(derivative);
    }

    /// Get derivative of curve between one point on the curve and the next in local 2D space
    /// Rather than interpolating over the entire curve, this simply interpolates
    /// between the point with fromIndex and the next segment.
    /// This is not normalized by default in case you don't need that
    public Vector2 Derivative(int fromIndex, float alpha)
    {
        InitSpline();
        return _spline.Derivative(fromIndex, alpha);
    }

    /// Get derivative of curve between one point on the curve and the next in world space
    /// Rather than interpolating over the entire curve, this simply interpolates
    /// between the point with fromIndex and the next segment
    /// This is not normalised by default in case you don't need that
    public Vector3 DerivativeWorldSpace(int fromIndex, float alpha)
    {
        Vector3 derivative = Derivative(fromIndex, alpha);
        if (displayXZ)
        {
            derivative = FlipXYtoXZ(derivative);
        }
        return transform.TransformDirection(derivative);
    }

    /// Convert a physical distance to a t position on the curve. This is
    /// approximate, the accuracy of can be changed via LengthSamplesPerSegment
    public float DistanceToLinearT(float distance)
    {
        InitSpline();
        return _spline.DistanceToLinearT(distance);
    }

    /// Interpolate a position on the entire curve based on distance in local 2D space. This is
    /// approximate, the accuracy of can be changed via LengthSamplesPerSegment
    public Vector2 InterpolateDistance(float dist) 
    {
        InitSpline();
        return _spline.InterpolateDistance(dist);
    }

    /// Interpolate a position on the entire curve based on distance in world space. This is
    /// approximate, the accuracy of can be changed via LengthSamplesPerSegment
    public Vector3 InterpolateDistanceWorldSpace(float dist)
    {
        Vector3 p = InterpolateDistance(dist);
        if (displayXZ)
        {
            p = FlipXYtoXZ(p);
        }
        
        return transform.TransformPoint(p);
    }

    /// Get derivative of the curve at a point long the curve at a distance, in local 2D space. This
    /// is approximate, the accuracy of this can be changed via
    /// LengthSamplesPerSegment
    public Vector2 DerivativeDistance(float dist)
    {
        InitSpline();
        return _spline.DerivativeDistance(dist);
    }

    /// Get derivative of the curve at a point long the curve at a distance in world space. This
    /// is approximate, the accuracy of this can be changed via
    /// LengthSamplesPerSegment
    public Vector2 DerivativeDistanceWorldSpace(float dist) 
    {
        Vector3 d = DerivativeDistance(dist);
        if (displayXZ)
        {
            d = FlipXYtoXZ(d);
        }
        
        return transform.TransformDirection(d);
    }

    /**
     * Gets the closest major point on the spline to the given position
     * <param name="position">Position to compare with spline</param>
     */
    public Vector2 GetClosestPoint(Vector2 position, out int index)
    {
        InitSpline();
        return _spline.GetClosestPoint(position + (Vector2)transform.position, out index) - (Vector2)transform.position;
    }

    public int GetNextSubdividedIndex(int index)
    {
        ++index;
        return index == _spline.SubdividedPoints.Count ? 0 : index;
    }

    public Vector2 GetSubdividedPoint(int index)
    {
        return _spline.SubdividedPoints[index];
    }

    // Editor functions
    private void OnDrawGizmos()
    {
        DrawCurveGizmo();
    }

    private void OnDrawGizmosSelected() 
    {
		DrawNormalsGizmo();
		DrawDistancesGizmo();
    }

    private void DrawCurveGizmo()
    {
		if (count == 0)
        {
			return;
		}
        
        var sampleWidth = 1.0f / ((float)count * StepsPerSegment);
        Gizmos.color = Color.blue;
        var pLast = GetPointWorldSpace(0);
        for (var t = sampleWidth; t <= 1.0f; t += sampleWidth) {
            var p = InterpolateWorldSpace(t);

            Gizmos.DrawLine(pLast, p);
            pLast = p;
        }
        
        foreach (var point in _spline.SubdividedPoints)
        {
            Handles.Button((Vector3)point + transform.position, Quaternion.identity, 0.05f, 0.0f, Handles.DotHandleCap);
        }
    }

	private void DrawNormalsGizmo()
    {
		if (count == 0 || !showNormals) 
        {
			return;
		}
        
        var sampleWidth = 1.0f / ((float)count * StepsPerSegment);
        Gizmos.color = Color.magenta;
        for (var t = 0.0f; t <= 1.0f; t += sampleWidth) {
            var p = InterpolateWorldSpace(t);
			var tangent = DerivativeWorldSpace(t);
			tangent.Normalize();
            tangent *= normalDisplayLength;
            p -= tangent * 0.5f;
            Gizmos.DrawLine(p, p + tangent);
        }
	}

	private void DrawDistancesGizmo() 
    {
		if (count == 0 || !showDistance || distanceMarker <= 0.0f) 
        {
			return;
		}
        
        var len = length;
        Gizmos.color = Color.green;
		var rot90 = Quaternion.AngleAxis(90, transform.TransformDirection(displayXZ ? Vector3.up : Vector3.forward));

        for (var dist = 0.0f; dist <= len; dist += distanceMarker)
        {
			// Just so we only have to perform the dist->t calculation once
			// for both position & tangent
			var t = DistanceToLinearT(dist);
            var p = InterpolateWorldSpace(t);
			var tangent = DerivativeWorldSpace(t);
			// Rotate tangent 90 degrees so we can render marker
			tangent.Normalize();
			tangent = rot90 * tangent;
			var t1 = p + tangent;
			var t2 = p - tangent;
            Gizmos.DrawLine(t1, t2);
        }

	}

    private static Vector3 FlipXYtoXZ(Vector3 inp) 
    {
        return new Vector3(inp.x, 0, inp.y);
    }
    
    private static Vector3 FlipXZtoXY(Vector3 inp) 
    {
        return new Vector3(inp.x, inp.z, 0);
    }

}