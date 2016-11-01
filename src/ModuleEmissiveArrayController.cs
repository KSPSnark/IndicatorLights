namespace IndicatorLights
{
    /// <summary>
    /// A simple array controller that sets all meshes to the same basic pattern.
    /// Allows for an optional master toggle switch for picking which pattern is
    /// shown.
    /// </summary>
    [ExperimentalController]
    class ModuleEmissiveArrayController : ModuleEmissiveControllerBase
    {
        [KSPField]
        [ToggleIDField]
        public string toggleID = string.Empty;

        [KSPField]
        public string activeColor = string.Empty;

        [KSPField]
        public string inactiveColor = string.Empty;

        private IToggle toggle = null;
        private ColorSourceArray activeSource = null;
        private ColorSourceArray inactiveSource = null;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            toggle = Identifiers.FindFirst<IToggle>(part, toggleID);
            activeSource = ColorSourceArray.of(activeColor);
            if (toggle == null)
            {
                if (!string.IsNullOrEmpty(inactiveColor))
                {
                    Logging.Warn("Ignoring inactiveColor on " + GetType() + " of " + part.GetTitle() + " (no toggle was specified)");
                }
            }
            else
            {
                inactiveSource = ColorSourceArray.of(inactiveColor);
            }
        }

        /// <summary>
        /// Called on every frame when it's time to set colors on the controllable emissives.
        /// </summary>
        protected override void SetColors()
        {
            if ((toggle == null) || toggle.ToggleStatus)
            {
                activeSource.SetColors(this);
            }
            else
            {
                inactiveSource.SetColors(this);
            }
        }
    }
}
