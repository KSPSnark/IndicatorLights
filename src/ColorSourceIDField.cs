using System;

namespace IndicatorLights
{
    /// <summary>
    /// Used for decorating module properties that are intended to be parsed as color source IDs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ColorSourceIDField : Attribute
    {
        /// <summary>
        /// Gets whether the specified field is a color source ID field.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool Is(BaseField field)
        {
            return field.FieldInfo.IsDefined(typeof(ColorSourceIDField), true)
                && (typeof(string).IsAssignableFrom(field.FieldInfo.FieldType));
        }
    }
}
