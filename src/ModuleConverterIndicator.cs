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
    class ModuleConverterIndicator : ModuleEmissiveController
    {
        private static readonly Color INACTIVE_COLOR = Color.black;

        private Color activeColor = Configuration.resourceConverterActiveColor;
        private BaseConverter converter = null;
        private ChangeMonitor<bool> converterActiveMonitor = null;

        internal override string EditorGuiDescription
        {
            get { return "Converter Status"; }
        }

        /// <summary>
        /// The name of the converter to which this controller is bound. This
        /// must match the ConverterName field of the converter module it's targeting.
        /// </summary>
        [KSPField]
        public string converterName = null;

        /// <summary>
        /// The red component of the "lit" color.
        /// </summary>
        [KSPField]
        public float red = Configuration.resourceConverterActiveColor.r;

        /// <summary>
        /// The green component of the "lit" color.
        /// </summary>
        [KSPField]
        public float green = Configuration.resourceConverterActiveColor.g;

        /// <summary>
        /// The blue component of the "lit" color.
        /// </summary>
        [KSPField]
        public float blue = Configuration.resourceConverterActiveColor.b;

        /// <summary>
        /// Called when the module is starting up.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            activeColor = new Color(red, green, blue, 1);

            converter = FindConverter();
            if (converter == null)
            {
                // bad config!
                Logging.Warn("Can't find converter named '" + converterName + "' on " + part.GetTitle());
                Color = INACTIVE_COLOR;
                return;
            }
            converterActiveMonitor = new ChangeMonitor<bool>(IsConverterActivated);
            SetState();
        }

        /// <summary>
        /// Called on every frame. Note that this implements Unity's Update method, rather
        /// than overriding the OnUpdate method of PartModule, because this needs to get
        /// called regardless of whether the part is active or not.
        /// </summary>
        void Update()
        {
            if (DidConverterActivationChange) SetState();
        }

        /// <summary>
        /// Here when the converter we're tracking changes its "activated" value.
        /// </summary>
        /// <param name="value"></param>
        private void OnConverterActivationChanged(object value)
        {
            SetState();
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

        private bool DidConverterActivationChange
        {
            get { return (converterActiveMonitor != null) && converterActiveMonitor.Update(IsConverterActivated); }
        }

        private bool IsConverterActivated
        {
            get { return (converter != null) && converter.IsActivated; }
        }

        private void SetState()
        {
            if (converter == null)
            {
                return;
            }
            Color = converter.IsActivated ? activeColor : INACTIVE_COLOR;
        }
    }
}
