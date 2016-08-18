using System;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// A controller that sets the display based on whether a resource is enabled/disabled.
    /// </summary>
    class ModuleResourceEnabledIndicator : ModuleResourceIndicator, IToggle
    {
        private IColorSource enabledSource = null;
        private IColorSource disabledSource = null;

        /// <summary>
        /// The color to display when the resource is enabled.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string enabledColor = Colors.ToString(DefaultColor.ToggleLED.Value());

        /// <summary>
        /// The color to display when the resource is disabled.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string disabledColor = ColorSources.Blink(
            ColorSources.Constant(DefaultColor.ToggleLED),
            900,
            ColorSources.Constant(DefaultColor.Off),
            300).ColorSourceID;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            enabledSource = FindColorSource(enabledColor);
            disabledSource = FindColorSource(disabledColor);
        }

        public override bool HasColor
        {
            get
            {
                return base.HasColor && CurrentSource.HasColor;
            }
        }

        public override Color OutputColor
        {
            get { return CurrentSource.OutputColor; }
        }

        private IColorSource CurrentSource
        {
            get { return Resource.flowState ? enabledSource : disabledSource; }
        }

        /// <summary>
        /// IToggle implementation.
        /// </summary>
        public bool ToggleStatus
        {
            get { return Resource.flowState; }
        }
    }
}
