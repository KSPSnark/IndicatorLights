using System;
using System.Reflection;

namespace IndicatorLights
{
    /// <summary>
    /// Used for decorating module properties that are intended to be used as static inputs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    class StaticField : Attribute
    {
        /// <summary>
        /// Gets whether the specified field is a scalar field.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool Is(FieldInfo field)
        {
            return field.IsDefined(typeof(StaticField), true)
                && (typeof(double).IsAssignableFrom(field.FieldType));
        }
    }
}
