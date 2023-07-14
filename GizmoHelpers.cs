using UnityEngine;

namespace UGameCore.Utilities
{
    public enum GizmoSolidityType
    {
        Solid,
        Wireframe,
    }

    public static class GizmoHelpers
    {
        public static void DrawCube(Vector3 center, Vector3 size, GizmoSolidityType solidityType)
        {
            if (solidityType == GizmoSolidityType.Wireframe)
                Gizmos.DrawWireCube(center, size);
            else
                Gizmos.DrawCube(center, size);
        }

        public static void DrawCube(Bounds bounds, GizmoSolidityType solidityType)
            => DrawCube(bounds.center, bounds.size, solidityType);

        /// <summary>
        /// Draws text using <see cref="UnityEditor.Handles"/>.
        /// </summary>
        public static void DrawText(string text, Vector3 position, Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(position, text);
#endif
        }
    }
}
