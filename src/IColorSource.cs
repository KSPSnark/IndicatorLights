using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Interface for objects that can provide a color.
    /// </summary>
    public interface IColorSource
    {
        /// <summary>
        /// Whether a color is currently available.
        /// </summary>
        bool HasColor { get; }

        /// <summary>
        /// Current color.  Do not call this unless HasColor returns true.
        /// </summary>
        Color OutputColor { get; }

        /// <summary>
        /// Gets the ID of this source.
        /// </summary>
        string ColorSourceID { get; }
    }
}
