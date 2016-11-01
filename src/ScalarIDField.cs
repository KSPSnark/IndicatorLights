using System;

namespace IndicatorLights
{
    /// <summary>
    /// Used for decorating module properties that are intended to be parsed as scalar IDs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    class ScalarIDField : Attribute
    {
        /// <summary>
        /// Gets whether the specified field is a scalar ID field.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool Is(BaseField field)
        {
            return field.FieldInfo.IsDefined(typeof(ScalarIDField), true)
                && (typeof(string).IsAssignableFrom(field.FieldInfo.FieldType));
        }
    }
}
