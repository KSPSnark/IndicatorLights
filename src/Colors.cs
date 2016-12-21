using System;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Utility helper class for working with colors.
    /// </summary>
    internal static class Colors
    {
        private const string HEX_DIGITS = "0123456789ABCDEF";
        private static readonly string[] DEFAULT_COLOR_NAMES = GetDefaultColorNames();

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
            Color color;
            bool success = Parse(text, out color);
            return success ? color : defaultColor;
        }

        /// <summary>
        /// Determines whether the specified string is a valid color specification.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsColorString(string text)
        {
            Color color;
            return Parse(text, out color);
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

        /// <summary>
        /// Given a default color, produce a string of it that is parseable via Parse.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string ToString(DefaultColor id)
        {
            return "$" + id.ToString();
        }

        /// <summary>
        /// Try to parse the specified text into a color. Returns true if successful.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private static bool Parse(string text, out Color color)
        {
            if (text != null)
            {
                text = text.Trim().ToUpper();
                if (text.Length > 1)
                {
                    // Did they specify a default color by name?
                    if (text.StartsWith("$"))
                    {
                        string logicalName = text.Substring(1);
                        for (int i = 0; i < DEFAULT_COLOR_NAMES.Length; ++i)
                        {
                            if (DEFAULT_COLOR_NAMES[i].Equals(logicalName))
                            {
                                color = ((DefaultColor)i).Value();
                                return true;
                            }
                        }
                    }
                    // Maybe it's a literal color string?
                    if (TryParseLiteralColor(text, out color)) return true;
                } // if the text is long enough
            } // if the text's not null

            // Not a valid color string.
            color = Color.black;
            return false;
        }

        /// <summary>
        /// Attempt to parse a literal color from a string. Handles formats #RRGGBB and #RRGGBBAA.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private static bool TryParseLiteralColor(string text, out Color color)
        {
            color = Color.black;
            if (!text.StartsWith("#")) return false;
            bool needAlpha;
            switch (text.Length)
            {
                case 7:
                    needAlpha = false;
                    break;
                case 9:
                    needAlpha = true;
                    break;
                default:
                    // wrong length; not a literal color
                    return false;
            }

            // get the RGB values
            int red, green, blue;
            if (!TryParseHex(text.Substring(1, 2), out red)
                || !TryParseHex(text.Substring(3, 2), out green)
                || !TryParseHex(text.Substring(5, 2), out blue))
            {
                // nope, couldn't get them
                return false;
            }

            // get the alpha value, if needed
            int alpha = 255;
            if (needAlpha && !TryParseHex(text.Substring(7, 2), out alpha))
            {
                // needed alpha, but couldn't get it
                return false;
            }

            // got all the necessary ingredients
            color = new Color(ChannelValue(red), ChannelValue(green), ChannelValue(blue), ChannelValue(alpha));
            return true;
        }

        /// <summary>
        /// Attempt to parse an integer value in the range 0-255 out of a hexadecimal
        /// representation thereof. Returns true for success, false for invalid format.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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

        private static string[] GetDefaultColorNames()
        {
            // We make them all uppercase for case insensitivity
            string[] names = Enum.GetNames(typeof(DefaultColor));
            for (int i = 0; i < names.Length; ++i)
            {
                names[i] = names[i].ToUpper();
            }
            return names;
        }
    }
}
