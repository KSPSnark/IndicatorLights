using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// This module is a controller for resource converters that toggles the emissive indicator
    /// on/off based on whether the converter is currently activated.
    /// 
    /// There's no in-editor customization (typically this runs with a modded part that's
    /// just had an emissive mesh added to it somewhere, and we don't want to tinker with
    /// the default part experience in the editor). So, no UI.
    /// 
    /// However, the "lit" color can be customized via part config. The default if not
    /// specified is bright green, but any arbitrary color can be configured.
    /// </summary>
    class ModuleConverterIndicator : ModuleEmissiveController, IToggle
    {
        private BaseConverter converter = null;
        private IColorSource activeSource;
        private IColorSource inactiveSource;

        /// <summary>
        /// The name of the converter to which this controller is bound. This
        /// must match the ConverterName field of the converter module it's targeting.
        /// </summary>
        [KSPField]
        public string converterName = null;

        /// <summary>
        /// The color to use when the converter is active.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string activeColor = Colors.ToString(DefaultColor.ToggleLED);

        /// <summary>
        /// The color to use when the converter is inactive.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string inactiveColor = Colors.ToString(DefaultColor.Off);

        /// <summary>
        /// Called when the module is starting up.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            activeSource = FindColorSource(activeColor);
            inactiveSource = FindColorSource(inactiveColor);

            converter = FindConverter();
            if (converter == null)
            {
                // bad config!
                Logging.Warn("Can't find converter named '" + converterName + "' on " + part.GetTitle());
                return;
            }
        }

        /// <summary>
        /// Find the converter module that drives this controller (null if can't be found).
        /// </summary>
        /// <returns></returns>
        private BaseConverter FindConverter()
        {
            for (int i = 0; i < part.Modules.Count; ++i)
            {
                BaseConverter candidate = part.Modules[i] as BaseConverter;
                if (candidate == null) continue;
                if (object.Equals(converterName, candidate.ConverterName)) return candidate;
            }
            return null; // not found!
        }

        /// <summary>
        /// IToggle implementation.
        /// </summary>
        public bool ToggleStatus
        {
            get { return (converter != null) && converter.IsActivated; }
        }

        public override bool HasColor
        {
            get { return (converter != null) && CurrentSource.HasColor; }
        }

        public override Color OutputColor
        {
            get { return CurrentSource.OutputColor; }
        }

        private IColorSource CurrentSource
        {
            get { return ToggleStatus ? activeSource : inactiveSource; }
        }
    }
}
