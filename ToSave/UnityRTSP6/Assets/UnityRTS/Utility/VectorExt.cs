using UnityEngine;
using System.Collections;

namespace UnityRTS {

    public static class Vector3Ext {

        public static Vector2 XY(this Vector3 vec) { return new Vector2(vec.x, vec.y); }
        public static Vector2 XZ(this Vector3 vec) { return new Vector2(vec.x, vec.z); }

        public static Vector3 Y(this Vector2 vec, float y) { return new Vector3(vec.x, y, vec.y); }
        public static Vector3 Z(this Vector2 vec, float z) { return new Vector3(vec.x, vec.y, z); }

    }

}