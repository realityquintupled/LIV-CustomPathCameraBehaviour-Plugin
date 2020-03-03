using System.Collections.Generic;
using UnityEngine;

public class Spline {
    public List<Vector3> points;

    public Spline(List<Vector3> points) {
        this.points = points;
    }

    public Vector3 PointAtTime(float t, float c) {
        t = t % points.Count;
        if (t < 0)
            t = points.Count - -t;
        int index = (int)t;
        Vector3 a = index == 0 ? points[points.Count - 1] : points[index - 1];
        Vector3 k1 = points[index];
        Vector3 k2 = index + 1 == points.Count ? points[0] : points[index + 1];
        Vector3 b = index + 2 >= points.Count ? points[(index + 2) % points.Count] : points[index + 2];
        Vector3 m1 = ComputeCardinalTangent(a, k1, k2, c);
        Vector3 m2 = ComputeCardinalTangent(k1, k2, b, c);
        return CubicSpline(k1, m1, k2, m2, t - index);
    }

    private Vector3 CubicSpline(Vector3 p1, Vector3 m1, Vector3 p2, Vector3 m2, float t) {
        return (2 * t * t * t - 3 * t * t + 1) * p1 + (t * t * t - 2 * t * t + t) * m1 + (-2 * t * t * t + 3 * t * t) * p2 + (t * t * t - t * t) * m2;
    }

    private Vector3 ComputeCardinalTangent(Vector3 a, Vector3 k, Vector3 b, float c) {
        return (1 - c) * (b - a) / Vector3.Distance(a, b);
    }
}
