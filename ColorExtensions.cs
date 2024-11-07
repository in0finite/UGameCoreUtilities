using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UGameCore.Utilities
{
    public static class ColorExtensions
    {
        public static Color OrangeColor = Color.Lerp(Color.red, Color.yellow, 0.5f);

        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 ToColor32(this Vector4 vec4)
        {
            return (Color)vec4;
        }

        public static Color32 FromInt(int n)
        {
            return MemoryMarshal.CreateReadOnlySpan(ref n, 1).CastWithSameLength<int, Color32>()[0];
        }
    }
}
