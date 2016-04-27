using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Utility helper class for working with colors.
    /// </summary>
    internal static class Colors
    {
        private static readonly string HEX_DIGITS = "0123456789ABCDEF";
        /// <summary>
        /// Parse a color from the provided text string, which is assummed to be of
        /// the form "#xxxxxx", where the x's are hex digits. In case of a parse error,
        /// returns the specified default.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="defaultColor"></param>
        /// <returns></returns>
        public static Color Parse(string text, Color defaultColor)
        {
            if (text == null) return defaultColor;
            text = text.Trim().ToUpper();
            if (text.Length != 7) return defaultColor;
            if (!text.StartsWith("#")) return defaultColor;
            int red, green, blue;
            if (!TryParseHex(text.Substring(1, 2), out red)) return defaultColor;
            if (!TryParseHex(text.Substring(3, 2), out green)) return defaultColor;
            if (!TryParseHex(text.Substring(5, 2), out blue)) return defaultColor;
            return new Color(ChannelValue(red), ChannelValue(green), ChannelValue(blue), 1f);
        }

        /// <summary>
        /// Given a color, produce a string of it that is parseable via Parse.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToString(Color color)
        {
            int red = (int)(color.r * 255f + 0.5f);
            int green = (int)(color.g * 255f + 0.5f);
            int blue = (int)(color.b * 255f + 0.5f);
            return "#"
                + IntToHexString(red)
                + IntToHexString(green)
                + IntToHexString(blue);
        }

        private static bool TryParseHex(string text, out int value)
        {
            value = 0;
            for (int i = 0; i < text.Length; ++i)
            {
                int hexDigit = HEX_DIGITS.IndexOf(text[i]);
                if (hexDigit < 0) return false;
                value = (16 * value) + hexDigit;
            }
            return true;
        }

        private static string IntToHexString(int value)
        {
            return HEX_DIGITS[value / 16].ToString() + HEX_DIGITS[value % 16].ToString();
        }

        /// <summary>
        /// Given a color level in the range 0-255, produce a channel value in the range 0-1.
        /// </summary>
        /// <param name="colorLevel"></param>
        /// <returns></returns>
        private static float ChannelValue(int colorLevel)
        {
            return (float)colorLevel / 255f;
        }
    }
}
