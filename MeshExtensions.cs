using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UGameCore.Utilities
{
    public static class MeshExtensions
    {
        delegate void InternalSetVertexBufferData(
            int stream, IntPtr data, int dataStart, int meshBufferStart, int count, int elemSize, MeshUpdateFlags flags);

        static System.Reflection.MethodInfo s_InternalSetVertexBufferDataFunc;

        delegate void InternalSetIndexBufferData(
            IntPtr data, int dataStart, int meshBufferStart, int count, int elemSize, MeshUpdateFlags flags);

        static System.Reflection.MethodInfo s_InternalSetIndexBufferDataFunc;

        delegate void SetSizedNativeArrayForChannel(
            VertexAttribute channel, VertexAttributeFormat format, int dim, IntPtr values, int valuesArrayLength, int valuesStart, int valuesCount, MeshUpdateFlags flags);

        static System.Reflection.MethodInfo s_SetSizedNativeArrayForChannelFunc;


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

        public static void SetVertexBufferDataFromPtr(
            this Mesh mesh, int stream, IntPtr data, int dataStart, int meshBufferStart, int count, int elemSize, MeshUpdateFlags flags)
        {
            if (null == s_InternalSetVertexBufferDataFunc)
            {
                s_InternalSetVertexBufferDataFunc = typeof(Mesh).GetMethod(
                    "InternalSetVertexBufferData",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            }

            // this function is marked with [FreeFunction], but it seems it's fine to call it

            var d = (InternalSetVertexBufferData)Delegate.CreateDelegate(
                    typeof(InternalSetVertexBufferData), mesh, s_InternalSetVertexBufferDataFunc, throwOnBindFailure: true);

            d(stream, data, dataStart, meshBufferStart, count, elemSize, flags);
        }

        public static void SetIndexBufferDataFromPtr(
            this Mesh mesh, IntPtr data, int dataStart, int meshBufferStart, int count, int elemSize, MeshUpdateFlags flags)
        {
            if (null == s_InternalSetIndexBufferDataFunc)
            {
                s_InternalSetIndexBufferDataFunc = typeof(Mesh).GetMethod(
                    "InternalSetIndexBufferData",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            }

            // this function is marked with [FreeFunction], but it seems it's fine to call it

            var d = (InternalSetIndexBufferData)Delegate.CreateDelegate(
                    typeof(InternalSetIndexBufferData), mesh, s_InternalSetIndexBufferDataFunc, throwOnBindFailure: true);

            d(data, dataStart, meshBufferStart, count, elemSize, flags);
        }

        public static void SetVeticesFromPtr(
            this Mesh mesh, IntPtr data, int lengthInBytes, MeshUpdateFlags flags)
        {
            // note: element size must be 12 bytes

            if (lengthInBytes % 12 != 0)
                throw new ArgumentException($"Element size must be 12 bytes");

            int lengthInValues = lengthInBytes / 12;

            if (null == s_SetSizedNativeArrayForChannelFunc)
            {
                s_SetSizedNativeArrayForChannelFunc = typeof(Mesh).GetMethod(
                    "SetSizedNativeArrayForChannel",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            }

            var d = (SetSizedNativeArrayForChannel)Delegate.CreateDelegate(
                    typeof(SetSizedNativeArrayForChannel), mesh, s_SetSizedNativeArrayForChannelFunc, throwOnBindFailure: true);

            d(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, data, lengthInValues, 0, lengthInValues, flags);
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
