using UnityEngine;

namespace UGameCore.Utilities
{
    public static class MathUtils
    {
        public static float DistanceFromPointToLineSegment(Vector3 p, Vector3 v, Vector3 w)
        {
            // Return minimum distance between line segment vw and point p
            float l2 = Vector3.SqrMagnitude(v - w);  // i.e. |w-v|^2 -  avoid a sqrt
            if (l2 == 0.0f) return Vector3.Distance(p, v);   // v == w case
                                                    // Consider the line extending the segment, parameterized as v + t (w - v).
                                                    // We find projection of point p onto the line. 
                                                    // It falls where t = [(p-v) . (w-v)] / |w-v|^2
                                                    // We clamp t from [0,1] to handle points outside the segment vw.
            float t = Mathf.Max(0, Mathf.Min(1, Vector3.Dot(p - v, w - v) / l2));
            Vector3 projection = v + t * (w - v);  // Projection falls on the segment
            return Vector3.Distance(p, projection);
        }

        public static Vector2 MinComponents(Vector2 a, Vector2 b)
        {
            return new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
        }

        public static Vector3 MinComponents(Vector3 a, Vector3 b)
        {
            return a.Min(b);
        }

        public static float MinComponent(this Vector3 v)
        {
            return Mathf.Min(Mathf.Min(v.x, v.y), v.z);
        }

        public static Vector3 Min(this Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));
        }

        public static Vector3 Min(this Vector3 a, float b)
        {
            return a.Min(new Vector3(b, b, b));
        }

        public static Vector2 MaxComponents(Vector2 a, Vector2 b)
        {
            return new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }

        public static float MaxComponent(this Vector3 v)
        {
            return Mathf.Max(Mathf.Max(v.x, v.y), v.z);
        }

        public static Vector3 MaxComponents(Vector3 a, Vector3 b)
        {
            return a.Max(b);
        }

        public static Vector3 Max(this Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
        }

        public static Vector3 Max(this Vector3 a, float b)
        {
            return a.Max(new Vector3(b, b, b));
        }

        public static Vector3 Sign(this Vector3 v)
        {
            return new Vector3(v.x.Sign(), v.y.Sign(), v.z.Sign());
        }

        public static Vector3 Clamp(this Vector3 v, Vector3 a, Vector3 b)
        {
            return new Vector3(Mathf.Clamp(v.x, a.x, b.x), Mathf.Clamp(v.y, a.y, b.y), Mathf.Clamp(v.z, a.z, b.z));
        }

        public static Vector3 Clamp(this Vector3 v, float a, float b)
        {
            return new Vector3(Mathf.Clamp(v.x, a, b), Mathf.Clamp(v.y, a, b), Mathf.Clamp(v.z, a, b));
        }

        public static Vector3 NormalizedOrZero(this Vector3 vec)
        {
            if (vec == Vector3.zero)
                return Vector3.zero;

            return vec.normalized;
        }

        public static Vector3 Absolute(this Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        public static Vector2 AsAbsolute(this Vector2 vec)
        {
            return new Vector2(Mathf.Abs(vec.x), Mathf.Abs(vec.y));
        }

        public static Vector3 Scaled(this Vector3 v, Vector3 other)
        {
            Vector3 result = v;
            result.Scale(other);
            return result;
        }

        public static Vector3 Pow(this Vector3 v, float p)
        {
            return new Vector3(Mathf.Pow(v.x, p), Mathf.Pow(v.y, p), Mathf.Pow(v.z, p));
        }

        public static Vector4 Pow(this Vector4 v, float p)
        {
            return new Vector4(Mathf.Pow(v.x, p), Mathf.Pow(v.y, p), Mathf.Pow(v.z, p), Mathf.Pow(v.w, p));
        }

        /// <summary>
        /// Returns vector with negated component at given index.
        /// </summary>
        public static Vector3 NegatedAt(this Vector3 v, int index)
        {
            v[index] = -v[index];
            return v;
        }

        public static Vector3 WithMagnitude(this Vector3 v, float newMagnitude)
        {
            return v.normalized * newMagnitude;
        }

        public static Vector3 Inverted(this Vector3 vec3)
        {
            return new Vector3(1.0f / vec3.x, 1.0f / vec3.y, 1.0f / vec3.z);
        }

        public static Vector2 ToVec2WithXAndZ(this Vector3 vec3)
        {
            return new Vector2(vec3.x, vec3.z);
        }

        public static Vector3 ToVec3XZ(this Vector2 vec)
        {
            return new Vector3(vec.x, 0.0f, vec.y);
        }

        public static Vector3 WithValueAtIndex(this Vector3 vec3, int index, float value)
        {
            vec3[index] = value;
            return vec3;
        }

        public static Vector3 WithXAndZ(this Vector3 vec3)
        {
            return new Vector3(vec3.x, 0f, vec3.z);
        }

        public static Vector3 WithY(this Vector3 vec3, float yValue)
        {
            return new Vector3(vec3.x, yValue, vec3.z);
        }

        public static Vector3 WithAddedY(this Vector3 vec3, float addedYValue)
        {
            return new Vector3(vec3.x, vec3.y + addedYValue, vec3.z);
        }

        public static Vector3Int IsLess(this Vector3 v, Vector3 other)
        {
            return new Vector3Int(v.x < other.x ? 1 : 0, v.y < other.y ? 1 : 0, v.z < other.z ? 1 : 0);
        }

        public static bool IsGreaterOrEqual(this Vector2 local, Vector2 other)
        {
            if (local.x >= other.x && local.y >= other.y)
                return true;
            else
                return false;
        }

        public static bool IsLesserOrEqual(this Vector2 local, Vector2 other)
        {
            if (local.x <= other.x && local.y <= other.y)
                return true;
            else
                return false;
        }

        public static bool IsGreater(this Vector2 local, Vector2 other, bool orOperator = false)
        {
            if (orOperator)
            {
                if (local.x > other.x || local.y > other.y)
                    return true;
                else
                    return false;
            }
            else
            {
                if (local.x > other.x && local.y > other.y)
                    return true;
                else
                    return false;
            }
        }

        public static bool IsLesser(this Vector2 local, Vector2 other, bool orOperator = false)
        {
            if (orOperator)
            {
                if (local.x < other.x || local.y < other.y)
                    return true;
                else
                    return false;
            }
            else
            {
                if (local.x < other.x && local.y < other.y)
                    return true;
                else
                    return false;
            }
        }

        public static Vector3 TransformDirection(this Quaternion rot, Vector3 dir)
        {
            return rot * dir;
        }

        public static Vector3 ClampDirection(Vector3 dir, Vector3 referenceVec, float maxAngle)
        {
            float angle = Vector3.Angle(dir, referenceVec);
            if (angle > maxAngle)
            {
                // needs to be clamped

                return Vector3.RotateTowards(dir, referenceVec, (angle - maxAngle) * Mathf.Deg2Rad, 0f);
                //	Vector3.Lerp( dir, referenceVec, );
            }

            return dir;
        }

        public static float Min(this float a, float b)
        {
            return a < b ? a : b;
        }

        public static float Max(this float a, float b)
        {
            return a > b ? a : b;
        }

        public static float Clamp(this float x, float a, float b)
        {
            return Mathf.Clamp(x, a, b);
        }

        public static float Sign(this float f)
        {
            // note: don't use Unity's Mathf.Sign() - it doesn't return 0
            return System.Math.Sign(f);
        }

        public static bool BetweenInclusive(this float v, float min, float max)
        {
            return v >= min && v <= max;
        }

        public static bool BetweenExclusive(this float v, float min, float max)
        {
            return v > min && v < max;
        }

        public static int RoundToInt(this float f)
        {
            return Mathf.RoundToInt(f);
        }

        public static float SqrtOrZero(this float f)
        {
            if (float.IsNaN(f))
                return 0f;
            if (f <= 0f)
                return 0f;
            return Mathf.Sqrt(f);
        }

        /// <summary>
        /// Returns 0 if this number is NaN (Not-a-number), otherwise returns original value.
        /// </summary>
        public static float ZeroIfNan(this float f)
        {
            return float.IsNaN(f) ? 0f : f;
        }

        /// <summary>
        /// Returns 0 if this number is NaN (Not-a-number), otherwise returns original value.
        /// </summary>
        public static double ZeroIfNan(this double d)
        {
            return double.IsNaN(d) ? 0.0 : d;
        }

        /// <summary>
		/// Clamps all coordinates between 0 and 1.
		/// </summary>
		public static Rect Clamp01(this Rect rect)
        {

            float xMin = rect.xMin;
            float xMax = rect.xMax;
            float yMin = rect.yMin;
            float yMax = rect.yMax;

            xMin = Mathf.Clamp01(xMin);
            xMax = Mathf.Clamp01(xMax);
            yMin = Mathf.Clamp01(yMin);
            yMax = Mathf.Clamp01(yMax);

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public static bool Contains(this Rect rect, Rect other)
        {

            return rect.xMax >= other.xMax && rect.xMin <= other.xMin && rect.yMax >= other.yMax && rect.yMin <= other.yMin;

        }

        public static Rect Intersection(this Rect rect, Rect other)
        {

            float xMax = Mathf.Min(rect.xMax, other.xMax);
            float yMax = Mathf.Min(rect.yMax, other.yMax);

            float xMin = Mathf.Max(rect.xMin, other.xMin);
            float yMin = Mathf.Max(rect.yMin, other.yMin);

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public static Rect Normalized(this Rect rect, Rect outter)
        {

            float xMin = (rect.xMin - outter.xMin) / outter.width;
            float xMax = (rect.xMax - outter.xMin) / outter.width;

            float yMin = (rect.yMin - outter.yMin) / outter.height;
            float yMax = (rect.yMax - outter.yMin) / outter.height;

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public static Rect CreateRect(Vector2 center, Vector2 size)
        {
            return new Rect(center - size / 2.0f, size);
        }
    }
}
