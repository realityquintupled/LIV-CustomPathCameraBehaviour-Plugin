using System.Collections.Generic;
using UnityEngine;

class PathRenderer : MonoBehaviour {
    private LineRenderer lr;
    private List<Vector3> points;

    private void Awake() {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.widthCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, .01f) });
    }

    public void AddPoints(List<Vector3> points) {
        ClearPoints();
        this.points = points;
        foreach(Vector3 point in points) {
            GameObject pointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObj.transform.position = point;
            pointObj.transform.parent = transform;
            pointObj.transform.localScale = new Vector3(.05f, .05f, .05f);
        }
    }

    public void ClearPoints() {
        foreach (Transform point in transform)
            Destroy(point.gameObject);
    }

    public void RenderSpline(Spline spline, float c) {
        if (points.Count < 3) {
            lr.positionCount = 0;
            return;
        }
        Vector3[] dp = new Vector3[points.Count * 30 + 1];
        for (int i = 0; i < dp.Length - 1; i++) {
            dp[i] = spline.PointAtTime(i * (float)points.Count / dp.Length, c);
        }
        dp[dp.Length - 1] = dp[0];
        lr.positionCount = dp.Length;
        lr.SetPositions(dp);
    }
}
