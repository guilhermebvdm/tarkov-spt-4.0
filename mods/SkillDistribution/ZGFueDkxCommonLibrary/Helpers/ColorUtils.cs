using UnityEngine;

namespace ZGFueDkx.ZGCLib.helpers
{
    internal class ColorUtils
    {
        public static Color From32(byte r, byte g, byte b, byte a = 255)
        {
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        public static Color FromHex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }

            return Color.white;
        }

        public string ToHex(Color c)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(c)}";
        }
    }
}
