using UnityEngine;
using UnityEngine.Rendering;

namespace UGameCore.Utilities
{
    public static class MeshExtensions
    {
        /// <summary>
        /// Get size in bytes of single vertex in a vertex buffer.
        /// </summary>
        public static int GetVBStride(this Mesh mesh)
        {
            int vbCount = mesh.vertexBufferCount;
            int stride = 0;
            for (int i = 0; i < vbCount; i++)
                stride += mesh.GetVertexBufferStride(i);
            return stride;
        }

        public static long GetVBSize(this Mesh mesh)
        {
            return mesh.GetVBStride() * (long)mesh.vertexCount;
        }

        public static uint GetNumIndexes(this Mesh mesh)
        {
            int subMeshCount = mesh.subMeshCount;
            uint indexCount = 0;
            for (int i = 0; i < subMeshCount; i++)
                indexCount += mesh.GetIndexCount(i);
            return indexCount;
        }

        public static uint GetNumTriangles(this Mesh mesh)
        {
            return mesh.GetNumIndexes() / 3;
        }

        public static long GetIBSize(this Mesh mesh)
        {
            return mesh.GetNumIndexes() * (long)(mesh.indexFormat == IndexFormat.UInt16 ? 2 : 4);
        }

        public static Mesh CreateWireframeCubeMesh()
        {
            var positions = new Vector3[8];
            int index = 0;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        var vec = new Vector3(x == 0 ? -0.5f : 0.5f, y == 0 ? -0.5f : 0.5f, z == 0 ? -0.5f : 0.5f);
                        positions[index++] = vec;
                    }
                }
            }

            int[] indexes = new int[]
            {
                0, 1, 0, 2, 1, 3, 2, 3, // right side
                4, 5, 4, 6, 5, 7, 6, 7, // left side
                0, 4, 2, 6, // front side
                1, 5, 3, 7, // back side
            };

            var mesh = new Mesh();
            mesh.name = "Wireframe cube";
            mesh.vertices = positions;
            mesh.subMeshCount = 1;
            mesh.SetIndices(indexes, MeshTopology.Lines, 0);

            return mesh;
        }
    }
}
