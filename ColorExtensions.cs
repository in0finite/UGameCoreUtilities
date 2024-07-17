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
    }
}
