using System;
using System.Runtime.InteropServices;

namespace UGameCore.Utilities
{
    /// <summary>
    /// C-equivalent of union, 8 bytes.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct Union8 : IEquatable<Union8>
    {
        [FieldOffset(0)]
        public int IntValuePart1;
        [FieldOffset(0)]
        public float FloatValuePart1;

        [FieldOffset(0)]
        public long Int64Value;
        [FieldOffset(0)]
        public ulong Uint64Value;
        [FieldOffset(0)]
        public double DoubleValue;

        [FieldOffset(4)]
        public int IntValuePart2;
        [FieldOffset(4)]
        public float FloatValuePart2;

        public bool BoolValue { readonly get => this.Uint64Value != 0; set => this.Uint64Value = value ? 1UL : 0UL; }

        public readonly bool Equals(Union8 other)
        {
            return this.Int64Value == other.Int64Value;
        }
    }

    /// <summary>
    /// C-equivalent of union, 16 bytes.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct Union16 : IEquatable<Union16>
    {
        [FieldOffset(0)]
        public Union8 Part1;
        [FieldOffset(8)]
        public Union8 Part2;

        public readonly bool Equals(Union16 other)
        {
            return this.Part1.Equals(other.Part1) && this.Part2.Equals(other.Part2);
        }
    }
}
