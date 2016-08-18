using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// A bi-state indicator whose output is controlled by another toggle controller.
    /// </summary>
    class ModuleIndirectToggleIndicator : ModuleEmissiveController, IToggle
    {
        /// <summary>
        /// Identifies the source module to use for deciding toggle status.
        /// </summary>
        [KSPField]
        public string toggleName = null;

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

        private IToggle toggle = null;
        private IColorSource activeSource = null;
        private IColorSource inactiveSource = null;

        /// <summary>
        /// Here when we're starting up.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            toggle = Identifiers.FindFirst<IToggle>(part, toggleName);
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
                return (toggle == null) ? false : toggle.ToggleStatus;
            }
        }

        private IColorSource CurrentSource
        {
            get
            {
                return ToggleStatus ? activeSource : inactiveSource;
            }
        }
    }
}
