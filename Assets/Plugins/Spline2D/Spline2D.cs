using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// Utility class for calculating a Cubic multi-segment (Hermite) spline in 2D
// Hermite splines are convenient because they only need 2 positions and 2
// tangents per segment, which can be automatically calculated from the surrounding
// points if desired.
// The spline can be extended dynamically over time and must always consist of
// 3 or more points. If the spline is closed, the spline will loop back to the
// first point.
// It can provide positions, derivatives (slope of curve) either at a parametric
// 't' value over the whole curve, or as a function of distance along the curve
// for constant-speed traversal. The distance is calculated approximately via
// sampling (cheap integration), its accuracy is determined by LengthSamplesPerSegment
// which defaults to 5 (a decent trade-off for most cases).
// This object is not a MonoBehaviour to keep it flexible. If you want to
// save/display one in a scene, use the wrapper Spline2DComponent class.
public class Spline2D
{
    private bool _tangentsDirty = true;
    private bool _lengthSampleDirty = true;
    
    // Points which the curve passes through.
    private readonly List<Vector2> _points = new();
    
    // Tangents at each point; automatically calculated
    private readonly List<Vector2> _tangents = new();

    // Points, calculated with a relatively even interval
    // does not necessarily contain the user-created points
    private readonly List<Vector2> _subdividedPoints = new();
    public List<Vector2> SubdividedPoints => _subdividedPoints;


    /// Whether the spline is closed; if so, the first point is also the last
    private bool _closed;
    public bool isClosed
    {
        get => _closed;
        set
        {
            _closed = value;
            _tangentsDirty = true;
            
            _lengthSampleDirty = true;
        }
    }
    
    /// The amount of curvature in the spline; 0.5 is Catmull-Rom
    private float _curvature = 0.5f;
    public float curvature {
        get => _curvature;
        set
        {
            _curvature = value;
            _tangentsDirty = true;
            _lengthSampleDirty = true;
        }
    }
    
    /// Accuracy of sampling curve to traverse by distance
    private int _lengthSamplesPerSegment = 5;
    public int lengthSamplesPerSegment
    {
        get => _lengthSamplesPerSegment;
        set {
            _lengthSamplesPerSegment = value;
            _lengthSampleDirty = true;
        }
    }

    private struct DistanceToT
    {
        public readonly float distance;
        public readonly float t;
        public DistanceToT(float dist, float tm)
        {
            distance = dist;
            t = tm;
        }
    }
    private readonly List<DistanceToT> _distanceToTList = new();

    /// Get point count
    public int count => _points.Count;

    /// Return the approximate length of the curve, as derived by sampling the
    /// curve at a resolution of LengthSamplesPerSegment
    public float length
    {
        get
        {
            Recalculate(true);
            return _distanceToTList.Count == 0 ? 0.0f : _distanceToTList[^1].distance;
        }
    }



    public Spline2D()
    {
    }

    public Spline2D(List<Vector2> intersectionPoints, bool isClosed = false, float curve = 0.5f,
        int samplesPerSegment = 5)
    {
        _points = intersectionPoints;
        _closed = isClosed;
        _curvature = curve;
        _lengthSamplesPerSegment = samplesPerSegment;
        _tangentsDirty = true;
        _lengthSampleDirty = true;
    }

    /// Add a point to the curve
    public void AddPoint(Vector2 p) 
    {
        _points.Add(p);
        _tangentsDirty = true;
        _lengthSampleDirty = true;
    }

    /// Add a point to the curve by dropping the earliest point and scrolling
    /// all other points backwards.
    /// This allows you to maintain a fixed-size spline which you extend to new
    /// points at the expense of dropping earliest points. This is efficient for
    /// unbounded paths you need to keep adding to but don't need the old history.
    /// Note that when you do this the distances change to being measured from
    /// the new start point so you have to adjust your next interpolation request
    /// to take this into account. Subtract DistanceAtPoint(1) from distances
    /// before calling this method, for example (or for plain `t` interpolation,
    /// reduce `t` by 1f/Count)
    /// This method cannot be used on closed splines
    public void AddPointScroll(Vector2 point)
    {
        Assert.IsFalse(_closed, "Cannot use AddPointScroll on closed splines!");

        if (_points.Count == 0)
        {
            AddPoint(point);
        }
        else
        {
            for (var i = 0; i < _points.Count - 1; ++i)
            {
                _points[i] = _points[i+1];
            }
            _points[^1] = point;
        }
        _tangentsDirty = true;
        _lengthSampleDirty = true;
    }

    /// Add a list of points to the end of the spline, in order
    public void AddPoints(IEnumerable<Vector2> pointList)
    {
        _points.AddRange(pointList);
        _tangentsDirty = true;
        _lengthSampleDirty = true;
    }

    /// Replace all the points in the spline from fromIndex onwards with a new set
    public void ReplacePoints(IEnumerable<Vector2> pointList, int fromIndex = 0)
    {
        Assert.IsTrue(fromIndex < _points.Count, "Spline2D: point index out of range");

        _points.RemoveRange(fromIndex, _points.Count-fromIndex);
        _points.AddRange(pointList);
        _tangentsDirty = true;
        _lengthSampleDirty = true;
    }

    /// Change a point on the curve
    public void SetPoint(int index, Vector2 point)
    {
        Assert.IsTrue(index < _points.Count, "Spline2D: point index out of range");

        _points[index] = point;
        _tangentsDirty = true;
        _lengthSampleDirty = true;
    }
    
    /// Remove a point on the curve
    public void RemovePoint(int index)
    {
        Assert.IsTrue(index < _points.Count, "Spline2D: point index out of range");

        _points.RemoveAt(index);
        _tangentsDirty = true;
        _lengthSampleDirty = true;
    }

    /// Insert a point on the curve before the given index
    public void InsertPoint(int index, Vector2 point)
    {
        Assert.IsTrue(index <= _points.Count && index >= 0, "Spline2D: point index out of range");
        _points.Insert(index, point);
        _tangentsDirty = true;
        _lengthSampleDirty = true;
    }

    // TODO add more efficient 'scrolling' curve of N length where we add one &
    // drop the earliest for efficient non-closed curves that continuously extend

    /// Reset and start again
    public void Clear()
    {
        _points.Clear();
        _tangentsDirty = true;
        _lengthSampleDirty = true;
    }
    
    /// Get a single point
    public Vector2 GetPoint(int index)
    {
        Assert.IsTrue(index < _points.Count, "Spline2D: point index out of range");
        return _points[index];
    }



    /// Interpolate a position on the entire curve. Note that if the control
    /// points are not evenly spaced, this may result in varying speeds.
    public Vector2 Interpolate(float alpha)
    {
        Recalculate(false);
        ToSegment(alpha, out var segmentIndex, out var segmentAlpha);
        return Interpolate(segmentIndex, segmentAlpha);
    }

    private void ToSegment(float t, out int iSeg, out float tSeg)
    {
        // Work out which segment this is in
        // Closed loops have 1 extra node at t = 1.0 i.e., the first node
        float pointCount = _closed ? _points.Count : _points.Count - 1;
        var fSeg = t * pointCount;
        iSeg = (int)fSeg;
        // Remainder t
        tSeg = fSeg - iSeg;
    }

    /// Interpolate a position between one point on the curve and the next
    /// Rather than interpolating over the entire curve, this simply interpolates
    /// between the point with fromIndex and the next point
    public Vector2 Interpolate(int fromIndex, float t)
    {
        Recalculate(false);

        var toIndex = fromIndex + 1;
        // At or beyond last index?
        if (toIndex >= _points.Count)
        {
            if (_closed)
            {
                // Wrap
                toIndex = toIndex % _points.Count;
                fromIndex = fromIndex % _points.Count;
            }
            else
            {
                // Clamp to end
                return _points[^1];
            }
        }

        // Fast special cases
        if (Mathf.Approximately(t, 0.0f)) {
            return _points[fromIndex];
        } else if (Mathf.Approximately(t, 1.0f)) {
            return _points[toIndex];
        }

        // Now general case
        // Pre-calculate powers
        var t2 = t * t;
        var t3 = t2 * t;
        // Calculate hermite basis parts
        var h1 =  2f*t3 - 3f*t2 + 1f;
        var h2 = -2f*t3 + 3f*t2;
        var h3 =     t3 - 2f*t2 + t;
        var h4 =     t3 -    t2;

        return h1 * _points[fromIndex] +
               h2 * _points[toIndex] +
               h3 * _tangents[fromIndex] +
               h4 * _tangents[toIndex];
    }

    /// Get derivative of the curve at a point. Note that if the control
    /// points are not evenly spaced, this may result in varying speeds.
    /// This is not normalized by default in case you don't need that
    public Vector2 Derivative(float t)
    {
        Recalculate(false);
        ToSegment(t, out var segmentIndex, out var segmentAlpha);
        return Derivative(segmentIndex, segmentAlpha);
    }

    /// Get derivative of curve between one point on the curve and the next
    /// Rather than interpolating over the entire curve, this simply interpolates
    /// between the point with fromIndex and the next segment.
    /// This is not normalized by default in case you don't need the normalized value
    public Vector2 Derivative(int fromIndex, float alpha)
    {
        Recalculate(false);

        var toIndex = fromIndex + 1;
        // At or beyond last index?
        if (toIndex >= _points.Count)
        {
            if (_closed) 
            {
                // Wrap
                toIndex = toIndex % _points.Count;
                fromIndex = fromIndex % _points.Count;
            }
            else 
            {
                // Clamp to end
                toIndex = fromIndex;
            }
        }

        // Pre-calculate power
        var t2 = alpha*alpha;
        // Derivative of hermite basis parts
        var h1 =  6f*t2 - 6f*alpha;
        var h2 = -6f*t2 + 6f*alpha;
        var h3 =  3f*t2 - 4f*alpha + 1;
        var h4 =  3f*t2 - 2f*alpha;

        return h1 * _points[fromIndex] +
               h2 * _points[toIndex] +
               h3 * _tangents[fromIndex] +
               h4 * _tangents[toIndex];
    }

    /// Convert a physical distance to a t position on the curve. This is
    /// approximate, the accuracy of can be changed via LengthSamplesPerSegment
    public float DistanceToLinearT(float dist)
    {
        return DistanceToLinearT(dist, out _);
    }

    /// Convert a physical distance to a t position on the curve. This is
    /// approximate, the accuracy of can be changed via LengthSamplesPerSegment
    /// Also returns an out param of the last point index passed
    public float DistanceToLinearT(float dist, out int lastIndex) {
        Recalculate(true);

        if (_distanceToTList.Count == 0) {
            lastIndex = 0;
            return 0.0f;
        }

        // Check to see if distance > length
        var len = length;
        if (dist >= len) 
        {
            if (_closed) 
            {
                // wrap and continue as usual
                dist %= len;
            }
            else 
            {
                // clamp to end
                lastIndex = _points.Count - 1;
                return 1.0f;
            }
        }


        var prevDist = 0.0f;
        var prevT = 0.0f;
        for (var i = 0; i < _distanceToTList.Count; ++i)
        {
            var distToT = _distanceToTList[i];
            if (dist < distToT.distance) 
            {
                var distanceT = Mathf.InverseLerp(prevDist, distToT.distance, dist);
                lastIndex = i / _lengthSamplesPerSegment; // not i-1 because distanceToTList starts at point index 1
                return Mathf.Lerp(prevT, distToT.t, distanceT);
            }
            prevDist = distToT.distance;
            prevT = distToT.t;
        }

        // If we got here then we ran off the end
        lastIndex = _points.Count - 1;
        return 1.0f;
    }

    /// Interpolate a position on the entire curve based on distance. This is
    /// approximate, the accuracy of can be changed via LengthSamplesPerSegment
    public Vector2 InterpolateDistance(float dist) 
    {
        var t = DistanceToLinearT(dist);
        return Interpolate(t);
    }

    /// Get derivative of the curve at a point long the curve at a distance. This
    /// is approximate, the accuracy of this can be changed via
    /// LengthSamplesPerSegment
    public Vector2 DerivativeDistance(float distance) 
    {
        var alpha = DistanceToLinearT(distance);
        return Derivative(alpha);
    }

    /// Get the distance at a point index
    public float DistanceAtPoint(int index)
    {
        Assert.IsTrue(index < _points.Count, "Spline2D: point index out of range");

        // Length samples are from first actual distance, with points at
        // LengthSamplesPerSegment intervals
        if (index == 0) {
            return 0.0f;
        }
        Recalculate(true);
        return _distanceToTList[index*_lengthSamplesPerSegment - 1].distance;
    }
    
    /**
     * Gets the closest major point on the spline to the given position
     * <param name="position">Position to compare with spline</param>
     * <param name="index">Index of the point in the list</param>
     */
    public Vector2 GetClosestPoint(Vector2 position, out int index)
    {
        Recalculate(true);
        var subdividedPointsCount = SubdividedPoints.Count;
        switch (subdividedPointsCount)
        {
            case 0:
            {
                index = 0;
                return Vector2.zero;
            }
            
            case 1:
            {
                index = 0;
                return SubdividedPoints[0];
            }
        }

        var closestPointIndex = 0;
        var minDistanceSquared = Vector2.SqrMagnitude(position - SubdividedPoints[0]);
        
        for (var i = 1; i < subdividedPointsCount; ++i)
        {
            var currentPointDistanceSquared = Vector2.SqrMagnitude(position - SubdividedPoints[i]);
            if (!(currentPointDistanceSquared < minDistanceSquared))
            {
                continue;
            }
            minDistanceSquared = currentPointDistanceSquared;
            closestPointIndex = i;
        }

        index = closestPointIndex;
        return SubdividedPoints[closestPointIndex];
    }

    private void Recalculate(bool includingLength) 
    {
        if (_tangentsDirty) 
        {
            RecalculateTangents();
            _tangentsDirty = false;
        }
        
        // Need to check the length of distanceToTList because for some reason
        // when scripts are reloaded in the editor, tangents survives but
        // distanceToTList does not (and dirty flags remain false). Maybe because
        // it's a custom struct it can't be restored
        if (!includingLength || (!_lengthSampleDirty && _distanceToTList.Count != 0))
        {
            return;
        }
        
        RecalculateLength();
        _lengthSampleDirty = false;
        
        RecalculateSubdividedPoints();
    }

    private void RecalculateTangents()
    {
        var numPoints = _points.Count;
        if (numPoints < 2) 
        {
            // Nothing to do here
            return;
        }
        _tangents.Clear();
        _tangents.Capacity = numPoints;

        for (var i = 0; i < numPoints; ++i)
        {
            Vector2 tangent;
            if (i == 0)
            {
                // Special case start
                // Wrap around
                tangent = _closed ? 
                    MakeTangent(_points[numPoints-1], _points[1]) :
                    MakeTangent(_points[i], _points[i+1]);
            } 
            else if (i == numPoints - 1)
            {
                // Wrap around
                tangent = _closed ? 
                    MakeTangent(_points[i - 1], _points[0]) :
                    MakeTangent(_points[i - 1], _points[i]);
            } 
            else 
            {
                // Midpoint is average of previous point and next point
                tangent = MakeTangent(_points[i-1], _points[i+1]);
            }
            _tangents.Add(tangent);
        }
    }

    private Vector2 MakeTangent(Vector2 p1, Vector2 p2) 
    {
        return _curvature * (p2 - p1);
    }

    private void RecalculateLength() 
    {
        var numPoints = _points.Count;
        if (numPoints < 2) 
        {
            return;
        }
        
        // Sample along curve & build distance -> t lookup, can interpolate t
        // linearly between nearest points to approximate distance parametrization
        // count is segments * lengthSamplesPerSegment
        // We sample from for st t > 0 all the way to t = 1
        // For a closed loop, t = 1 is the first point again, for open it's the last point
        var samples = _lengthSamplesPerSegment * (_closed ? _points.Count : _points.Count-1);

        _distanceToTList.Clear();
        _distanceToTList.Capacity = samples;
        var distanceSoFar = 0.0f;
        var tinc = 1.0f / samples;
        var t = tinc; // we don't start at 0 since that's easy
        var lastPos = _points[0];
        for (var i = 1; i <= samples; ++i)
        {
            var pos = Interpolate(t);
            var distInc = Vector2.Distance(lastPos, pos);
            distanceSoFar += distInc;
            _distanceToTList.Add(new DistanceToT(distanceSoFar, t));
            lastPos = pos;
            t += tinc;
        }
    }

    private void RecalculateSubdividedPoints()
    {
        var numPoints = _points.Count;
        if (numPoints < 2)
        {
            return;
        }
        
        _subdividedPoints.Clear();
        _subdividedPoints.Add(_points[0]);
        for (var i = 1; i < length; ++i)
        {
            _subdividedPoints.Add(InterpolateDistance(i));
        }
    }
}