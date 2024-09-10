using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UGameCore.Utilities
{
    public static class TransformExtensions
    {
        public static IEnumerable<Transform> GetAllParents(this Transform tr)
        {
            Transform currentParent = tr.parent;
            while (currentParent != null)
            {
                yield return currentParent;
                currentParent = currentParent.parent;
            }
        }

        public static bool IsParentOf(this Transform tr, Transform child)
        {
            return child.GetAllParents().Any(p => p == tr);
        }

        public static void ExchangeSiblings(Transform trA, Transform trB)
        {
            if (trA.parent != trB.parent)
                throw new System.InvalidOperationException($"Transforms must have the same parent");

            int indexA = trA.GetSiblingIndex();
            int indexB = trB.GetSiblingIndex();

            trA.SetSiblingIndex(indexB);
            trB.SetSiblingIndex(indexA);
        }

        /// <summary>
        /// Get depth level (ie. number of parents) in the hierarchy.
        /// </summary>
        public static int GetDepthLevel(this Transform tr)
        {
            return tr.GetAllParents().Count();
        }

        public static T GetTopmostParentComponent<T>(this Transform tr)
            where T : Component
        {
            T result = null;

            while (tr.parent != null)
            {
                T component = tr.parent.GetComponent<T>();
                if (component != null)
                    result = component;

                tr = tr.parent;
            }

            return result;
        }

        public static IEnumerable<Transform> GetFirstLevelChildren(this Transform tr)
        {
            for (int i = 0; i < tr.childCount; i++)
            {
                yield return tr.GetChild(i);
            }
        }

        public static Transform[] GetFirstLevelChildrenPreallocated(this Transform tr)
        {
            var array = new Transform[tr.childCount];
            for (int i = 0; i < tr.childCount; i++)
            {
                array[i] = tr.GetChild(i);
            }
            return array;
        }

        public static List<Transform> GetAllChildrenRecursively(this Transform tr)
            => tr.GetComponentsInChildrenExceptThis<Transform>();

        public static List<T> GetComponentsInChildrenExceptThis<T>(this Transform tr)
        {
            int childCount = tr.childCount;
            var resultList = new List<T>(childCount);
            var tempList = new List<T>();
            for (int i = 0; i < childCount; i++)
            {
                Transform child = tr.GetChild(i);
                child.GetComponentsInChildren(tempList);
                resultList.AddRange(tempList);
            }
            return resultList;
        }

        /// <summary>
        /// Find first-level (direct) children of specified <see cref="Transform"/> that have specified name.
        /// </summary>
        public static List<Transform> FindFirstLevelChildrenWithName(this Transform transform, string strName)
        {
            var resultList = new List<Transform>();
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name.Equals(strName, System.StringComparison.Ordinal))
                    resultList.Add(child);
            }
            return resultList;
        }

        /// <summary>
        /// Recursively searches through transform hierarchy to find a child with specified name.
        /// </summary>
        public static Transform FindChildRecursive(this Transform transform, string strName)
        {
            if (transform.name.Equals(strName, System.StringComparison.InvariantCultureIgnoreCase))
                return transform;

            for (int i = 0; i < transform.childCount; i++)
            {
                var tran = transform.GetChild(i).FindChildRecursive(strName);
                if (tran) return tran;
            }

            return null;
        }

        public static Transform FindChildWithPath(this Transform transform, string path, string pathSeparator)
        {
            string[] parts = path.Split(pathSeparator, System.StringSplitOptions.RemoveEmptyEntries);

            Transform currentTr = transform;
            for (int i = 0; i < parts.Length; i++)
            {
                Transform child = currentTr.Find(parts[i]);
                if (null == child)
                    return null;
                currentTr = child;
            }

            return currentTr;
        }

        public static void DestroyChildren(this Transform transform)
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
                F.DestroyEvenInEditMode(transform.GetChild(i).gameObject);
        }

        public static void SetChildrenActive(this Transform transform, bool isActive)
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
                transform.GetChild(i).gameObject.SetActive(isActive);
        }

        public static void SetY(this Transform t, float yPos)
        {
            Vector3 pos = t.position;
            pos.y = yPos;
            t.position = pos;
        }

        public static void SetLocalY(this Transform t, float yPos)
        {
            Vector3 pos = t.localPosition;
            pos.y = yPos;
            t.localPosition = pos;
        }

        public static float Distance(this Transform t, Vector3 pos)
        {
            return Vector3.Distance(t.position, pos);
        }

        public static float DistanceSquared(this Transform t, Vector3 pos)
        {
            return (t.position - pos).sqrMagnitude;
        }

        public static float LocalDistanceSquared(this Transform t, Vector3 pos)
        {
            return (t.localPosition - pos).sqrMagnitude;
        }

        /// <summary>
		/// Transforms the rotation from local space to world space.
		/// </summary>
		public static Quaternion TransformRotation(this Transform tr, Quaternion rot)
        {
            Vector3 localForward = rot * Vector3.forward;
            Vector3 localUp = rot * Vector3.up;

            return Quaternion.LookRotation(tr.TransformDirection(localForward), tr.TransformDirection(localUp));
        }

        /// <summary>
		/// Transforms the rotation from world space to local space.
		/// </summary>
		public static Quaternion InverseTransformRotation(this Transform tr, Quaternion rot)
        {
            Vector3 localForward = rot * Vector3.forward;
            Vector3 localUp = rot * Vector3.up;

            return Quaternion.LookRotation(tr.InverseTransformDirection(localForward), tr.InverseTransformDirection(localUp));
        }

        /// <summary>
        /// Transforms the bounds from local space to world space.
        /// </summary>
        public static Bounds TransformBounds(this Transform tr, in Bounds bounds)
        {
            // note: Transform scale is not applied
            Vector3 center = tr.TransformPoint(bounds.center);
            return new Bounds(center, bounds.size);
        }

        public static void CopyPositionAndRotation(this Transform tr, Transform target)
        {
            tr.GetPositionAndRotation(out Vector3 pos, out Quaternion rotation);
            target.SetPositionAndRotation(pos, rotation);
        }

        public static void CopyPositionAndRotation(this Transform tr, Transform target, out Vector3 pos, out Quaternion rotation)
        {
            tr.GetPositionAndRotation(out pos, out rotation);
            target.SetPositionAndRotation(pos, rotation);
        }

        public static void CopyLocalPositionAndRotation(this Transform tr, Transform target)
        {
            tr.GetLocalPositionAndRotation(out Vector3 pos, out Quaternion rotation);
            target.SetLocalPositionAndRotation(pos, rotation);
        }

        public static void GetPositionAndRotation(this Transform tr, out PositionAndRotation positionAndRotation)
        {
            tr.GetPositionAndRotation(out positionAndRotation.Position, out positionAndRotation.Rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PositionAndRotation GetPositionAndRotation(this Transform tr)
        {
            PositionAndRotation matrix;
            tr.GetPositionAndRotation(out matrix.Position, out matrix.Rotation);
            return matrix;
        }

        public static void SetPositionAndRotation(this Transform tr, PositionAndRotation positionAndRotation)
        {
            tr.SetPositionAndRotation(positionAndRotation.Position, positionAndRotation.Rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetLocalPositionAndRotation(this Transform tr, out PositionAndRotation positionAndRotation)
        {
            tr.GetLocalPositionAndRotation(out positionAndRotation.Position, out positionAndRotation.Rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PositionAndRotation GetLocalPositionAndRotation(this Transform tr)
        {
            PositionAndRotation matrix;
            tr.GetLocalPositionAndRotation(out matrix.Position, out matrix.Rotation);
            return matrix;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLocalPositionAndRotation(this Transform tr, PositionAndRotation positionAndRotation)
        {
            tr.SetLocalPositionAndRotation(positionAndRotation.Position, positionAndRotation.Rotation);
        }

        public static void SetGlobalScale(this Transform tr, Vector3 globalScale)
        {
            Vector3 parentGlobalScale = tr.parent != null ? tr.parent.lossyScale : Vector3.one;
            tr.localScale = Vector3.Scale(globalScale, parentGlobalScale.Inverted());
        }
    }
}
