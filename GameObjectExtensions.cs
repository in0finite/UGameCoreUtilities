using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UGameCore.Utilities
{
    public static class GameObjectExtensions
    {
		public static T GetOrAddComponent<T>(this GameObject go) where T : Component
		{
			T comp = go.GetComponent<T>();
			if (null == comp)
				comp = go.AddComponent<T>();
			return comp;
		}

		public static T GetComponentOrThrow<T>(this GameObject go)
		{
			T comp = go.GetComponent<T>();
			if (null == comp)
				throw new MissingComponentException(string.Format("Failed to get component of type: {0}, on game object: {1}", typeof(T), go.name));
			return comp;
		}

		public static T GetComponentOrLogError<T>(this GameObject go)
		{
			T comp = go.GetComponent<T>();
			if (null == comp)
				Debug.LogErrorFormat("Failed to get component of type: {0}, on game object: {1}", typeof(T), go.name);
			return comp;
		}

		/// <summary>
		/// Returns component of given type, if it is the only one attached, otherwise throws exception.
		/// </summary>
		public static T GetSingleComponentOrThrow<T>(this GameObject go)
		{
			T[] components = go.GetComponents<T>();
			if (components.Length == 0)
				throw new MissingComponentException(string.Format("Failed to get component of type: {0}, on game object: {1}", typeof(T), go.name));
			if (components.Length > 1)
				throw new System.InvalidOperationException($"Found {components.Length} components of type {typeof(T)} on game object {go.name}, but required exactly 1");
			return components[0];
		}

        public static T GetComponentInChildrenOrThrow<T>(this GameObject go)
        {
            T comp = go.GetComponentInChildren<T>();
            if (null == comp)
                throw new MissingComponentException(string.Format("Failed to get component in children of type: {0}, on game object: {1}", typeof(T), go.name));
            return comp;
        }

        public static T GetComponentInParentOrThrow<T>(this GameObject go)
        {
            T comp = go.GetComponentInParent<T>();
            if (null == comp)
                throw new MissingComponentException(string.Format("Failed to get component in parent of type: {0}, on game object: {1}", typeof(T), go.name));
            return comp;
        }

        public static void DestroyComponent<T>(this GameObject go)
		{
            if (!go.TryGetComponent(out T component))
                return;

			UnityEngine.Object.Destroy(component as Component);
		}

        public static void DestroyComponentEvenInEditMode<T>(this GameObject go)
        {
			if (!go.TryGetComponent(out T component))
				return;

			F.DestroyEvenInEditMode(component as Component);
        }

        public static IEnumerable<Transform> GetFirstLevelChildren(this GameObject go)
		{
			return go.transform.GetFirstLevelChildren();
        }

        public static IEnumerable<T> GetFirstLevelChildrenComponents<T>(this GameObject go)
		{
			return go.transform.GetFirstLevelChildren().SelectMany(c => c.GetComponents<T>());
		}

		public static IEnumerable<T> GetFirstLevelChildrenSingleComponent<T>(this GameObject go)
		{
			return go.transform.GetFirstLevelChildren().Select(c => c.GetComponent<T>()).Where(_ => _ is Component component && component != null);
		}

		public static string GetGameObjectPath(this GameObject obj)
		{
			string path = obj.name;
			while (obj.transform.parent != null)
			{
				obj = obj.transform.parent.gameObject;
				path = obj.name + "/" + path;
			}
			return path;
		}

		public static void SetLayerRecursive(this GameObject go, int Layer)
		{
			go.layer = Layer;

			for (int i = 0; i < go.transform.childCount; i++)
			{
				go.transform.GetChild(i).gameObject.SetLayerRecursive(Layer);
			}
		}

		public static void SetLayerRecursive(this GameObject go, string layer)
		{
			var layerint = LayerMask.NameToLayer(layer);
			if (layerint == 0)
			{
				Debug.LogWarning("SetLayerRecursive: couldn't find layer: " + layer);
				return;
			}

			go.SetLayerRecursive(layerint);
		}

        public static Bounds? CombineBounds(IEnumerable<Bounds> boundsEnumerable)
		{
            Bounds bounds = default;
            bool hasBounds = false;
            foreach (var b in boundsEnumerable)
            {
                if (hasBounds)
                    bounds.Encapsulate(b);
                else
                    bounds = b;
                hasBounds = true;
            }

            return hasBounds ? bounds : null;
        }

        public static Bounds GetRenderersBounds(this GameObject go)
        {
			var renderers = go.GetComponentsInChildren<Renderer>();
			
            return CombineBounds(renderers.Select(r => r.bounds)) ?? new Bounds(go.transform.position, Vector3.zero);
        }

        public static Bounds GetRenderersAndCollidersBounds(this GameObject go)
		{
            var renderers = go.GetComponentsInChildren<Renderer>();
            var colliders = go.GetComponentsInChildren<Collider>();

            return CombineBounds(renderers.Select(r => r.bounds).Concat(colliders.Select(c => c.bounds)))
				?? new Bounds(go.transform.position, Vector3.zero);
        }
    }
}
