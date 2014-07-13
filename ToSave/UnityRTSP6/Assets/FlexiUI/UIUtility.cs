using System;
using System.Collections.Generic;
using UnityEngine;
using UIExtensions;

namespace UIExtensions {
    public static class VectorExt {
        public static Vector2 XY(this Vector3 vec) { return new Vector2(vec.x, vec.y); }
        public static Vector2 XZ(this Vector3 vec) { return new Vector2(vec.x, vec.z); }

        public static float Cross(this Vector2 v1, Vector2 v2) { return v1.x * v2.y - v1.y * v2.x; }

        public static float LengthSquared(this Vector2 vec) { return vec.sqrMagnitude; }
        public static float Length(this Vector2 vec) { return vec.magnitude; }
        public static float LengthSquared(this Vector3 vec) { return vec.sqrMagnitude; }
        public static float Length(this Vector3 vec) { return vec.magnitude; }

        public static Vector2 Normalized(this Vector2 vec) { return vec.normalized; }
        public static Vector3 Normalized(this Vector3 vec) { return vec.normalized; }
    }

    public static class RectEx {

        public static float LerpX(this Rect rect, float lerp) { return rect.x + rect.width * lerp; }
        public static float LerpY(this Rect rect, float lerp) { return rect.y + rect.height * lerp; }
        public static Vector2 Lerp(this Rect rect, float x, float y) { return new Vector2(rect.LerpX(x), rect.LerpY(y)); }

        public static Vector2 Offset(this Rect rect) { return new Vector2(rect.x, rect.y); }
        public static Rect Offset(this Rect rect, Vector2 vec) { return Offset(rect, vec.x, vec.y); }
        public static Rect Offset(this Rect rect, float x, float y) {
            return new Rect(rect.x + x, rect.y + y, rect.width, rect.height);
        }
        public static Rect OffsetN(this Rect rect, Vector2 vec) { return OffsetN(rect, vec.x, vec.y); }
        public static Rect OffsetN(this Rect rect, float x, float y) {
            return new Rect(rect.x + x * rect.width, rect.y + y * rect.height, rect.width, rect.height);
        }

        public static Rect Inset(this Rect rect, float amount) { return rect.Inset(amount, amount, amount, amount); }
        public static Rect Inset(this Rect rect, float left, float top, float right, float bottom) {
            return new Rect(rect.x + top, rect.y + left, rect.width - right - left, rect.height - bottom - top);
        }

        public static Rect ExpandToInclude(this Rect rect, Vector2 pnt) {
            return Rect.MinMaxRect(
                Mathf.Min(rect.xMin, pnt.x),
                Mathf.Min(rect.yMin, pnt.y),
                Mathf.Max(rect.xMax, pnt.x),
                Mathf.Max(rect.yMax, pnt.y)
            );
        }

        public static Rect MinMaxNormalised(this Rect rect, float minX, float minY, float maxX, float maxY) {
            return Rect.MinMaxRect(rect.LerpX(minX), rect.LerpY(minY), rect.LerpX(maxX), rect.LerpY(maxY));
        }

        public static Rect Lerp(this Rect r1, Rect r2, float lerp) {
            return new Rect(
                Mathf.Lerp(r1.x, r2.x, lerp), Mathf.Lerp(r1.y, r2.y, lerp),
                Mathf.Lerp(r1.width, r2.width, lerp), Mathf.Lerp(r1.height, r2.height, lerp)
            );
        }
        public static Vector2 Size(this Rect rect) { return new Vector2(rect.width, rect.height); }

        public static Rect ScaleAroundPivot(this Rect rect, Vector2 scale, Vector2 pivot) {
            rect.x = (rect.x - pivot.x) * scale.x + pivot.x;
            rect.y = (rect.y - pivot.y) * scale.y + pivot.y;
            rect.width *= scale.x;
            rect.height *= scale.y;
            return rect;
        }
        public static Rect ScaleAroundAnchor(this Rect rect, Vector2 scale, Vector2 anchor) {
            return ScaleAroundPivot(rect, scale, new Vector2(rect.LerpX(anchor.x), rect.LerpY(anchor.y)));
        }
    }
}

public static class UIScale {
    public delegate Vector2 Function(Vector2 container, Vector2 size);
    public static Vector2 None(Vector2 container, Vector2 size) { return size; }
    public static Vector2 Fill(Vector2 container, Vector2 size) { return container; }
    public static Vector2 Uniform(Vector2 container, Vector2 size) { return size * Mathf.Min(container.x / size.x, container.y / size.y); }
    public static Vector2 UniformFill(Vector2 container, Vector2 size) { return size * Mathf.Max(container.x / size.x, container.y / size.y); }

    public static Vector2 UniformOrNone(Vector2 container, Vector2 size) { return size * Mathf.Min(Mathf.Min(container.x / size.x, container.y / size.y), 1); }
    public static Vector2 UniformFillOrNone(Vector2 container, Vector2 size) { return size * Mathf.Max(Mathf.Max(container.x / size.x, container.y / size.y), 1); }
}
public static class UIUtility {

    public static Rect Anchor(Rect bounds, Vector2 size, UIScale.Function scaleFn, Vector2 anchor) {
        return Anchor(bounds, size, scaleFn, anchor, anchor);
    }
    public static Rect Anchor(Rect bounds, Vector2 size, UIScale.Function scaleFn, Vector2 anchor, Vector2 itemAnchor) {
        // Apply any scaling required
        if (scaleFn != null) size = scaleFn(new Vector2(bounds.width, bounds.height), size);
        // Anchor scale to the bounding rectangle
        var rect = new Rect(bounds.x, bounds.y, size.x, size.y);
        rect.x += (bounds.width * anchor.x - rect.width * itemAnchor.x);
        rect.y += (bounds.height * anchor.y - rect.height * itemAnchor.y);
        return rect;
    }

    public static int Arrange(Rect bounds, Vector2 size, int count) {
        int rowCount = Mathf.Min(Mathf.FloorToInt(bounds.width / size.x), count);
        for (int t = 0; t < 100; ++t) {
            float scale = Mathf.Min(bounds.width / (size.x * rowCount), 1);
            int colCount = Mathf.Max(1, Mathf.FloorToInt(bounds.height / (size.y * scale)));
            if (rowCount * colCount >= count) break;
            rowCount++;
        }
        return rowCount;
    }
    public static Rect ArrangeH(Rect bounds, Vector2 size, int id, int hcount, int count) {
        int vcount = (count + hcount - 1) / hcount;
        var scale = Mathf.Min(bounds.width / (size.x * hcount), bounds.height / (size.y * vcount));
        if (scale < 1) size *= scale;
        int x = id % hcount;
        int y = id / hcount;
        return new Rect(bounds.x + size.x * x, bounds.y + size.y * y, size.x, size.y);
    }

    public struct GUIAlpha : IDisposable {
        Color oldCol;
        public GUIAlpha(float alpha) {
            oldCol = GUI.color;
            var newCol = oldCol;
            newCol.a = alpha * oldCol.a;
            GUI.color = newCol;
        }
        public void Dispose() { GUI.color = oldCol; }
    }
    public struct GUIColor : IDisposable {
        Color oldCol;
        public GUIColor(Color color) {
            oldCol = GUI.color;
            GUI.color = oldCol * color;
        }
        public void Dispose() { GUI.color = oldCol; }
    }
    public struct GUITransform : IDisposable {
        Matrix4x4 oldMat;
        public GUITransform(Matrix4x4 mat) {
            oldMat = GUI.matrix;
            GUI.matrix *= mat;
        }
        public GUITransform(Rect from, Rect to) {
            var scale = new Vector3(to.width / from.width, to.height / from.height, 1);
            oldMat = GUI.matrix;
            if (to.x != 0 || to.y != 0) GUI.matrix *= MatrixExt.Translate(new Vector2(to.x, to.y));
            GUIUtility.ScaleAroundPivot(scale, Vector2.zero);
            if (from.x != 0 || from.y != 0) GUI.matrix *= MatrixExt.Translate(new Vector2(-from.x, -from.y));
        }
        public void Dispose() { GUI.matrix = oldMat; }
    }
    public struct GUIScale : IDisposable {
        Matrix4x4 oldMat;
        public GUIScale(Vector2 pivot, float scale) {
            oldMat = GUI.matrix;
            GUIUtility.ScaleAroundPivot(Vector2.one * scale, pivot);
        }
        public void Dispose() { GUI.matrix = oldMat; }
    }
    public struct GUIOffset : IDisposable {
        Matrix4x4 oldMat;
        public GUIOffset(Vector2 offset) {
            oldMat = GUI.matrix;
            GUI.matrix = oldMat * MatrixExt.Translate(offset.x, offset.y, 0);
        }
        public void Dispose() { GUI.matrix = oldMat; }
    }
    public struct GUIRotate : IDisposable {
        Matrix4x4 oldMat;
        public GUIRotate(Vector2 pivot, float amount) {
            oldMat = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(pivot, Quaternion.AngleAxis(amount, Vector3.forward), Vector3.one) * MatrixExt.Translate(-pivot) * oldMat;
        }
        public void Dispose() { GUI.matrix = oldMat; }
    }
    public struct GUIClip : IDisposable {
        Type guiClip;
        Rect topRect;
        public GUIClip(Rect rect) {
            var guiType = typeof(GUI);
            var guiAssembly = guiType.Assembly;
            guiClip = guiAssembly.GetType("UnityEngine.GUIClip");
            var topRectMethod = guiClip.GetMethod("GetTopRect", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            topRect = (Rect)topRectMethod.Invoke(null, null);
            GUI.EndGroup();
            rect.x += topRect.x;
            rect.y += topRect.y;
            GUI.BeginGroup(rect);
        }
        public void Dispose() {
            GUI.EndGroup();
            GUI.BeginGroup(topRect);
        }
    }

    public enum Directions { Min, Max, };
    public struct Group : IDisposable {
        private static Rect _wbounds;
        private readonly Rect oldBounds;

        public readonly Rect Bounds;
        public float X { get { return Bounds.x; } }
        public float Y { get { return Bounds.y; } }
        public float Width { get { return Bounds.width; } }
        public float Height { get { return Bounds.height; } }
        public float x { get { return Bounds.x; } }
        public float y { get { return Bounds.y; } }
        public float width { get { return Bounds.width; } }
        public float height { get { return Bounds.height; } }

        public Group(Rect bounds) {
            oldBounds = _wbounds;
            _wbounds = bounds;
            Bounds = bounds;
        }
        public void Dispose() { _wbounds = oldBounds; }
        public Group HeightSlice(float height, bool reverse = false) {
            if (height > 0 && height < 1) height *= Bounds.height;
            var oldBounds = _wbounds;
            float div = !reverse ?
                Mathf.Min(oldBounds.yMin + height, oldBounds.yMax) :
                Mathf.Max(oldBounds.yMin, oldBounds.yMax - height);
            var minRect = Rect.MinMaxRect(oldBounds.xMin, oldBounds.yMin, oldBounds.xMax, div);
            var maxRect = Rect.MinMaxRect(oldBounds.xMin, div, oldBounds.xMax, oldBounds.yMax);
            _wbounds = !reverse ? maxRect : minRect;
            return new Group(!reverse ? minRect : maxRect);
        }
        public Group WidthSlice(float width, bool reverse = false) {
            if (width > 0 && width < 1) width *= Bounds.width;
            var oldBounds = _wbounds;
            float div = !reverse ?
                Mathf.Min(oldBounds.xMin + width, oldBounds.xMax) :
                Mathf.Max(oldBounds.xMin, oldBounds.xMax - width);
            var minRect = Rect.MinMaxRect(oldBounds.xMin, oldBounds.yMin, div, oldBounds.yMax);
            var maxRect = Rect.MinMaxRect(div, oldBounds.yMin, oldBounds.xMax, oldBounds.yMax);
            _wbounds = !reverse ? maxRect : minRect;
            return new Group(!reverse ? minRect : maxRect);
        }
        public static implicit operator Rect (Group group) {
            return group.Bounds;
        }
        internal static Rect Inset(float amount) { return new Rect(_wbounds.Inset(amount)); }
        internal static Rect Inset(float left, float top, float right, float bottom) {
            return new Rect(_wbounds.Inset(left, top, right, bottom));
        }
        internal static Group Clone() {
            return new Group(_wbounds);
        }
    }
    public static Group Inset(float amount) {
        return new Group(Group.Inset(amount));
    }
    public static Group Inset(float left, float top, float right, float bottom) {
        return new Group(Group.Inset(left, top, right, bottom));
    }

    public struct Grid : IDisposable {
        private Group container;
        private int oldXCount, oldYCount;
        public static int XCount, YCount;
        public Grid(int xcount, int ycount) {
            oldXCount = XCount;
            oldYCount = YCount;
            XCount = xcount;
            YCount = ycount;
            lastX = -1;
            lastY = 0;
            container = Group.Clone();
        }
        public void Dispose() {
            XCount = oldXCount;
            YCount = oldYCount;
        }
        private int lastX, lastY;
        public Group Cell() {
            if (lastX < XCount - 1) return Cell(lastX + 1, lastY);
            else return Cell(0, lastY + 1);
        }
        public Group Cell(int x, int y) {
            lastX = x;
            lastY = y;
            var bounds = container.Bounds;
            return new Group(Rect.MinMaxRect(
                bounds.LerpX((float)x / XCount), bounds.LerpY((float)y / YCount),
                bounds.LerpX((float)(x + 1) / XCount), bounds.LerpY((float)(y + 1) / YCount)
            ));
        }
    }

    private static class MatrixExt {

        public static Matrix4x4 Translate(float x, float y, float z) { return Translate(new Vector3(x, y, z)); }
        public static Matrix4x4 Translate(Vector3 vec) {
            var mat = Matrix4x4.identity;
            mat.SetColumn(3, new Vector4(vec.x, vec.y, vec.z, 1));
            return mat;
        }

    }

}
