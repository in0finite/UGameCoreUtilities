using UnityEngine;

namespace UGameCore.Utilities
{
    [System.Serializable]
    public struct PositionAndRotation
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public static readonly PositionAndRotation Identity = new(Vector3.zero, Quaternion.identity);

        public PositionAndRotation(Vector3 position, Quaternion rotation)
        {
            this.Position = position;
            this.Rotation = rotation;
        }

        /// <summary>
        /// Transforms position from local space to world space.
        /// </summary>
        public readonly Vector3 TransformPoint(Vector3 pos)
        {
            return this.Position + this.Rotation.TransformDirection(pos);
        }

        public readonly PositionAndRotation Multiply(PositionAndRotation other)
        {
            return new PositionAndRotation(
                this.TransformPoint(other.Position),
                this.Rotation * other.Rotation);
        }
    }
}
