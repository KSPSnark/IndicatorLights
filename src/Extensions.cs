using System;
using System.Text;

namespace IndicatorLights
{
    /// <summary>
    /// Convenient place for putting extension logic that doesn't particularly have an
    /// obvious home elsewhere.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Gets whether the given UI_Scene value should be enabled in the flight scene.
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static bool IsFlightEnabled(this UI_Scene scene)
        {
            switch (scene)
            {
                case UI_Scene.Editor:
                case UI_Scene.None:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Gets whether the given UI_Scene value should be enabled in the flight scene.
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static bool IsEditorEnabled(this UI_Scene scene)
        {
            switch (scene)
            {
                case UI_Scene.Flight:
                case UI_Scene.None:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Get a sub-array of the provided array, starting at the specified index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static T[] SubArray<T>(this T[] array, int startIndex)
        {
            if (startIndex == 0) return array;
            if ((startIndex < 0) || (startIndex > array.Length))
            {
                throw new ArgumentOutOfRangeException(
                    "startIndex",
                    startIndex,
                    "Array index out of range");
            }

            T[] result = new T[array.Length - startIndex];
            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = array[i + startIndex];
            }
            return result;
        }

        /// <summary>
        /// Joins an array of strings into a single string, using the specified joiner.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="joiner"></param>
        /// <returns></returns>
        public static string Join(this string[] array, string joiner)
        {
            if (array.Length < 1) return string.Empty;
            if (array.Length < 2) return array[0];
            StringBuilder builder = new StringBuilder(array[0]);
            for (int i = 1; i < array.Length; ++i)
            {
                builder.Append(joiner).Append(array[i]);
            }
            return builder.ToString();
        }
    }
}
