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
    }
}
