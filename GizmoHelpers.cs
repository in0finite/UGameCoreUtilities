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

        public static void DrawTransformAxes(Vector3 center, Quaternion rotation, float axisLength)
        {
            var previousColor = Gizmos.color;

            Vector3 forward = rotation.GetForward();
            Vector3 up = rotation.GetUp();
            Vector3 right = Vector3.Cross(up, forward);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(center, center + forward * axisLength);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, center + up * axisLength);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, center + right * axisLength);

            Gizmos.color = previousColor;
        }

        /// <summary>
        /// Draws text using <see cref="UnityEditor.Handles"/>.
        /// </summary>
        public static void DrawText(string text, Vector3 position, Color color)
        {
#if UNITY_EDITOR
            var previousColor = UnityEditor.Handles.color;
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(position, text);
            UnityEditor.Handles.color = previousColor;
#endif
        }
    }
}
