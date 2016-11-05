using System;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// A bi-state indicator whose output is controlled by other toggle controllers.
    /// </summary>
    class ModuleBooleanIndicator : ModuleEmissiveController, IToggle
    {
        /// <summary>
        /// Required. Specifies the input toggle that controls this module. Could be a simple
        /// identifier (such as a module or controller name), or it could be parseable toggle
        /// syntax such as "and(toggle1, !toggle2)".
        /// </summary>
        [KSPField]
        [ToggleIDField]
        public string input = string.Empty;

        /// <summary>
        /// Color used when in the "on" state.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string activeColor = Colors.ToString(DefaultColor.ToggleLED);

        /// <summary>
        /// Color used when in the "off" state.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string inactiveColor = Colors.ToString(DefaultColor.Off);

        bool isValid = false;
        private IToggle inputToggle = null;
        private IColorSource activeSource = null;
        private IColorSource inactiveSource = null;

        public override void ParseIDs()
        {
            base.ParseIDs();

            try
            {
                inputToggle = RequireToggle(input);
                isValid = true;
            }
            catch (ArgumentException e)
            {
                Logging.Warn("Invalid input for " + Identifier + " on " + part.GetTitle() + ": " + e.Message);
            }
            activeSource = FindColorSource(activeColor);
            inactiveSource = FindColorSource(inactiveColor);
        }

        public override bool HasColor
        {
            get
            {
                return CurrentSource.HasColor;
            }
        }

        public override Color OutputColor
        {
            get
            {
                return CurrentSource.OutputColor;
            }
        }

        /// <summary>
        /// IToggle implementation.
        /// </summary>
        public bool ToggleStatus
        {
            get
            {
                return isValid ? inputToggle.ToggleStatus : false;
            }
        }

        private IColorSource CurrentSource
        {
            get
            {
                if (!isValid) return ColorSources.ERROR;
                return ToggleStatus ? activeSource : inactiveSource;
            }
        }
    }
}
