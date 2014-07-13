using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityRTS {

    [Serializable]
    public class Expandable2DGrid<T> where T : class {

        [SerializeField]
        T[] container = new T[0];

        [SerializeField]
        int minX;
        [SerializeField]
        int minY;

        [SerializeField]
        int sizeX;
        [SerializeField]
        int sizeY;

        public T[] Values { get { return container; } }

        public Point Min { get { return new Point(minX, minY); } }
        public Point Max { get { return new Point(minX, minY) + Size + new Point(-1, -1); } }
        public Point Size { get { return new Point(sizeX, sizeY); } }

        public void ToLocal(ref int x, ref int y) {
            x -= minX;
            y -= minY;
        }
        public void ToWorld(ref int x, ref int y) {
            x += minX;
            y += minY;
        }

        public Expandable2DGrid() { }

        public bool IsInBounds(int x, int y) {
            return x >= 0 && y >= 0 && x < Size.X && y < Size.Y;
        }

        private int ToIndex(int x, int y) { return y * Size.X + x; }
        private int ToIndex(int x, int y, Point min, Point size) { return ((y - min.Y)) * size.X + (x - min.X); }

        public void Set(Point pnt, T obj) { Set(pnt.X, pnt.Y, obj); }
        public void Set(int x, int y, T obj) {
            ToLocal(ref x, ref y);
            Set_Local(x, y, obj);
        }
        public void Set_Local(int x, int y, T obj) {
            container[ToIndex(x, y)] = obj;
        }

        public T Get(Point pnt) { return Get(pnt.X, pnt.Y); }
        public T Get(int x, int y) {
            ToLocal(ref x, ref y);
            return Get_Local(x, y);
        }
        public T Get_Local(int x, int y) {
            if (!IsInBounds(x, y)) return null;
            var index = ToIndex(x, y);
            try {
                return container[index];
            } catch {
                Debug.Log("Unable to get " + x + "," + y + ": " + index + " @ " + Size + " = " + container.Length);
                throw new Exception();
            }
        }

        public void Add(Point pnt, T t) {
            Add(pnt.X, pnt.Y, t);
        }
        public void Add(int x, int y, T t) {
            ToLocal(ref x, ref y);
            if (x < 0 || y < 0 || x >= Size.X || y >= Size.Y) {
                ToWorld(ref x, ref y);
                var cmin = Point.Min(Min, new Point(x, y));
                var cmax = Point.Max(Max, new Point(x, y));
                Rebound(cmin, cmax);
                ToLocal(ref x, ref y);
            }
            Set_Local(x, y, t);
        }

        public void Rebound(Point min, Point max) {
            var cmin = Point.Max(min, Min);
            var cmax = Point.Min(max, Max);
            var size = max - min + Point.One;
            var newContainer = new T[size.X * size.Y];
            for (int tx = cmin.X; tx <= cmax.X; ++tx) {
                for (int ty = cmin.Y; ty <= cmax.Y; ++ty) {
                    newContainer[ToIndex(tx, ty, min, size)] =
                        container[ToIndex(tx, ty, Min, Size)];
                }
            }
            container = newContainer;
            minX = min.X; minY = min.Y;
            sizeX = size.X; sizeY = size.Y;
        }

        public void Each(Action<T> with) {
            for (int tx = 0; tx < Size.X; ++tx) {
                for (int ty = 0; ty < Size.Y; ++ty) {
                    var value = Get_Local(tx, ty);
                    if (value == null) continue;
                    with(value);
                }
            }
        }

        public void Clear() {
            for (int tx = 0; tx < Size.X; ++tx) {
                for (int ty = 0; ty < Size.Y; ++ty) {
                    Set_Local(tx, ty, null);
                }
            }
        }
    }

}
