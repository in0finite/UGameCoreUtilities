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

		public static T GetComponentOrThrow<T>(this GameObject go) where T : Component
		{
			T comp = go.GetComponent<T>();
			if (null == comp)
				throw new MissingComponentException(string.Format("Failed to get component of type: {0}, on game object: {1}", typeof(T), go.name));
			return comp;
		}

		public static T GetComponentOrLogError<T>(this GameObject go) where T : Component
		{
			T comp = go.GetComponent<T>();
			if (null == comp)
				Debug.LogErrorFormat("Failed to get component of type: {0}, on game object: {1}", typeof(T), go.name);
			return comp;
		}

		public static void DestroyComponent<T>(this GameObject go) where T : Component
		{
			T comp = go.GetComponent<T>();
			if (comp != null)
				UnityEngine.Object.Destroy(comp);
		}

		public static IEnumerable<T> GetFirstLevelChildrenComponents<T>(this GameObject go) where T : Component
		{
			return go.transform.GetFirstLevelChildren().SelectMany(c => c.GetComponents<T>());
		}

		public static IEnumerable<T> GetFirstLevelChildrenSingleComponent<T>(this GameObject go) where T : Component
		{
			return go.transform.GetFirstLevelChildren().Select(c => c.GetComponent<T>()).Where(_ => _ != null);
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

		public static Bounds GetRenderersBounds(this GameObject go)
        {
			var bounds = new Bounds(go.transform.position, Vector3.zero);
            foreach (var r in go.GetComponentsInChildren<Renderer>())
				bounds.Encapsulate(r.bounds);
			return bounds;
        }
	}
}
