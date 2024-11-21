using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UGameCore.Utilities
{
    public static class CameraUtilities
    {
        static Bounds? GetBounds(IEnumerable<GameObject> gos)
        {
            return GameObjectExtensions.CombineBounds(gos.Select(g => g.GetRenderersAndCollidersBounds()));
        }

        public static void MoveObjectInFrontOf(this Camera camera, GameObject go)
        {
            Bounds bounds = go.GetRenderersAndCollidersBounds();
            go.transform.position = camera.transform.position + camera.transform.forward * bounds.size.magnitude.Max(2.5f);
        }

        public static void MoveInFrontOfObjects(this Camera camera, IEnumerable<GameObject> gos)
        {
            Bounds? bounds = GetBounds(gos);
            if (!bounds.HasValue)
                return;

            camera.transform.position = bounds.Value.center - camera.transform.forward * bounds.Value.size.magnitude.Max(2.5f);
        }

        public static void MoveInFrontOfObject(this Camera camera, GameObject go)
        {
            camera.MoveInFrontOfObjects(new GameObject[] { go });
        }

        public static Vector3 ConvertPointToDifferentFOV(this Camera cam, Vector3 pos, float newFOV)
        {
            Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
            float aspect = cam.aspect;
            float nearClipPlane = cam.nearClipPlane;
            float farClipPlane = cam.farClipPlane;
            float currentFOV = cam.fieldOfView;

            Matrix4x4 newFOVMatrix = Matrix4x4.Perspective(newFOV, aspect, nearClipPlane, farClipPlane) * worldToCameraMatrix;
            Matrix4x4 currentFOVMatrix = Matrix4x4.Perspective(currentFOV, aspect, nearClipPlane, farClipPlane) * worldToCameraMatrix;
            Matrix4x4 m = Matrix4x4.Inverse(currentFOVMatrix) * newFOVMatrix;
            return m.MultiplyPoint(pos);
        }
    }
}
