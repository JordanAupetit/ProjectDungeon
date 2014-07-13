using UnityEngine;
using System.Collections;

namespace UnityRTS {

    public struct Point {
        public int X, Y;
        public Point(int x, int y) { X = x; Y = y; }
        public Point(Vector2 vec) : this(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y)) { }
        public static Point operator +(Point p1, Point p2) { return new Point(p1.X + p2.X, p1.Y + p2.Y); }
        public static Point operator -(Point p1, Point p2) { return new Point(p1.X - p2.X, p1.Y - p2.Y); }
        public static Point operator *(Point p1, Point p2) { return new Point(p1.X * p2.X, p1.Y * p2.Y); }
        public static Point operator *(Point p1, int amount) { return new Point(p1.X * amount, p1.Y * amount); }
        public static bool operator ==(Point p1, Point p2) { return p1.X == p2.X && p1.Y == p2.Y; }
        public static bool operator !=(Point p1, Point p2) { return p1.X != p2.X && p1.Y != p2.Y; }
        public int LengthSquared { get { return X * X + Y * Y; } }
        public static implicit operator Vector2(Point p) { return new Vector2(p.X, p.Y); }
        public override string ToString() { return X + "," + Y; }
        public override bool Equals(object obj) { return obj is Point && (Point)obj == this; }
        public override int GetHashCode() { return X ^ Y; }

        public static Point Min(Point p1, Point p2) {
            return new Point(Mathf.Min(p1.X, p2.X), Mathf.Min(p1.Y, p2.Y));
        }
        public static Point Max(Point p1, Point p2) {
            return new Point(Mathf.Max(p1.X, p2.X), Mathf.Max(p1.Y, p2.Y));
        }

        public static int DistanceSquared(Point p1, Point p2) {
            return (p2 - p1).LengthSquared;
        }

        public static readonly Point Zero = new Point(0, 0);
        public static readonly Point One = new Point(1, 1);

    }

}
