using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UGameCore.Utilities
{
    public static class TransformExtensions
    {
        public static void MakeChild(this Transform parent, GameObject[] children)
        {
            MakeChild(parent, children, null);
        }

        //Make the game objects children of the parent.
        public static void MakeChild(this Transform parent, GameObject[] children, Action<Transform, GameObject> actionPerLoop)
        {
            foreach (GameObject child in children)
            {
                child.transform.parent = parent;
                if (actionPerLoop != null) actionPerLoop(parent, child);
            }
        }

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

        /// <summary>
		/// Transforms the rotation from local space to world space.
		/// </summary>
		public static Quaternion TransformRotation(this Transform tr, Quaternion rot)
        {
            Vector3 localForward = rot * Vector3.forward;
            Vector3 localUp = rot * Vector3.up;

            return Quaternion.LookRotation(tr.TransformDirection(localForward), tr.TransformDirection(localUp));
        }

        public static void SetGlobalScale(this Transform tr, Vector3 globalScale)
        {
            Vector3 parentGlobalScale = tr.parent != null ? tr.parent.lossyScale : Vector3.one;
            tr.localScale = Vector3.Scale(globalScale, parentGlobalScale.Inverted());
        }
    }
}
