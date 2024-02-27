using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
    public static class CameraUtilities
    {
        static Bounds? GetBounds(IEnumerable<GameObject> gos)
        {
            Bounds? bounds = null;
            foreach (GameObject go in gos)
            {
                Bounds b = go.GetRenderersBounds();
                if (bounds.HasValue)
                    bounds.Value.Encapsulate(b);
                else
                    bounds = b;
            }
            return bounds;
        }

        public static void MoveObjectInFrontOf(this Camera camera, GameObject go)
        {
            Bounds bounds = go.GetRenderersBounds();
            go.transform.position = camera.transform.position + camera.transform.forward * bounds.size.magnitude.Max(0.5f);
        }

        public static void MoveInFrontOfObjects(this Camera camera, IEnumerable<GameObject> gos)
        {
            Bounds? bounds = GetBounds(gos);
            if (!bounds.HasValue)
                return;

            camera.transform.position = bounds.Value.center - camera.transform.forward * bounds.Value.size.magnitude.Max(0.5f);
        }

        public static void MoveInFrontOfObject(this Camera camera, GameObject go)
        {
            camera.MoveInFrontOfObjects(new GameObject[] { go });
        }
    }
}
