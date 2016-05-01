using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Base class for emissive controllers that can toggle between two states.
    /// </summary>
    abstract class ModuleBiStateIndicator : ModuleEmissiveController
    {
        /// <summary>
        /// Gets the color to use when active.
        /// </summary>
        protected abstract Color ActiveColor { get; }

        /// <summary>
        /// Gets the color to use when inactive.
        /// </summary>
        protected abstract Color InactiveColor { get; }

        /// <summary>
        /// Gets the state of the indicator:  true for active, false for inactive.
        /// </summary>
        protected abstract bool State { get; }

        public sealed override Color OutputColor
        {
            get
            {
                return State ? ActiveColor : InactiveColor;
            }
        }
    }
}
