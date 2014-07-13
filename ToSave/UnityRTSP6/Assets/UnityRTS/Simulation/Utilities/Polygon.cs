using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation {
    public class Polygon {

        public readonly Vector2[] Points;

        public Polygon(params Vector2[] points) { Points = points; }

        public Vector2 GetNearestPoint(Vector2 pos) {
            float nearDist2 = float.MaxValue;
            Vector2 near = Vector2.zero;
            for (int p = 0; p < Points.Length; ++p) {
                var pnt = Points[p];
                var dist2 = (pnt - pos).sqrMagnitude;
                if (dist2 < nearDist2) { nearDist2 = dist2; near = pnt; }
            }
            for (int p = 0; p < Points.Length; ++p) {
                var p0 = Points[p];
                var p1 = Points[(p + 1) % Points.Length];
                var delta = p1 - p0;
                float deltaDp = Vector2.Dot(pos - p0, delta) / delta.sqrMagnitude;
                if (deltaDp < 0 || deltaDp > 1) continue;
                var pnt = Vector2.Lerp(p0, p1, deltaDp);
                var dist2 = (pnt - pos).sqrMagnitude;
                if (dist2 < nearDist2) { nearDist2 = dist2; near = pnt; }
            }
            return near;
        }
        public float GetDistanceTo(Vector2 pnt) {
            return (GetNearestPoint(pnt) - pnt).magnitude;
        }


        public Rect GetExtents() {
            float xMin, xMax, yMin, yMax;
            GetExtents(out xMin, out yMin, out xMax, out yMax);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }
        public void GetExtents(out float xMin, out float yMin, out float xMax, out float yMax) {
            xMin = float.MaxValue; yMin = float.MaxValue;
            xMax = float.MinValue; yMax = float.MinValue;
            for (int p = 0; p < Points.Length; ++p) {
                xMin = Mathf.Min(xMin, Points[p].x);
                yMin = Mathf.Min(yMin, Points[p].y);
                xMax = Mathf.Max(xMax, Points[p].x);
                yMax = Mathf.Max(yMax, Points[p].y);
            }
        }

        public override string ToString() {
            return Points.Select(p => p.ToString()).Aggregate((p1, p2) => p1 + ", " + p2);
        }
    }
}
