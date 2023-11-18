using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// A module that sets the display based on the current status of a reaction wheel.
    /// </summary>
    class ModuleReactionWheelIndicator : ModuleSourceIndicator<ModuleReactionWheel>, IToggle, IScalar
    {
        private StartState startState = StartState.None;
        private IColorSource problemSource = null;
        private IColorSource normalSource = null;
        private IColorSource pilotOnlySource = null;
        private IColorSource sasOnlySource = null;

        /// <summary>
        /// The color to display when the reaction wheel has a problem.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string problemColor = Colors.ToString(DefaultColor.Warning);

        /// <summary>
        /// The color to display when the reaction wheel is operating in "normal" mode.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string normalColor = Colors.ToString(DefaultColor.ReactionWheelNormal);

        /// <summary>
        /// The color to display when the reaction wheel is operating in "pilot only" mode.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string pilotOnlyColor = Colors.ToString(DefaultColor.ReactionWheelPilotOnly);

        /// <summary>
        /// The color to display when the reaction wheel is operating in "SAS only" mode.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string sasOnlyColor = Colors.ToString(DefaultColor.ReactionWheelSASOnly);


        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            startState = state;
        }

        public override void ParseIDs()
        {
            base.ParseIDs();
            problemSource = FindColorSource(problemColor);
            normalSource = FindColorSource(normalColor);
            pilotOnlySource = FindColorSource(pilotOnlyColor);
            sasOnlySource = FindColorSource(sasOnlyColor);
        }

        public override bool HasColor
        {
            get { return base.HasColor && CurrentSource.HasColor; }
        }

        public override Color OutputColor
        {
            get
            {
                Color baseColor = CurrentSource.OutputColor;
                return (IsAutopilotActive) ? baseColor : (0.5f * baseColor);
            }
        }

        /// <summary>
        /// Gets whether SAS is turned on. Returns true if we're in a situation where it's
        /// irrelevant (e.g. in the vehicle editor).
        /// </summary>
        private bool IsAutopilotActive
        {
            get
            {
                return ((startState == StartState.Editor) || (vessel == null)) ? true : vessel.Autopilot.Enabled;
            }
        }

        private IColorSource CurrentSource
        {
            get
            {
                if (SourceModule == null) return ColorSources.BLACK;
                switch (SourceModule.State)
                {
                    case ModuleReactionWheel.WheelState.Disabled:
                        return ColorSources.BLACK;
                    case ModuleReactionWheel.WheelState.Broken:
                        return problemSource;
                    default:
                        break;
                }
                switch ((VesselActuatorMode)SourceModule.actuatorModeCycle)
                {
                    case VesselActuatorMode.Pilot:
                        return pilotOnlySource;
                    case VesselActuatorMode.SAS:
                        return sasOnlySource;
                    default:
                        return normalSource;
                }
            }
        }

        public bool ToggleStatus
        {
            get
            {
                return (SourceModule != null)
                    && (SourceModule.State == ModuleReactionWheel.WheelState.Active)
                    && (SourceModule.authorityLimiter > 0);
            }
        }

        public double ScalarValue
        {
            get
            {
                if (SourceModule == null) return 0.0;
                return (SourceModule.State == ModuleReactionWheel.WheelState.Active)
                    ? SourceModule.authorityLimiter
                    : 0.0;
            }
        }
    }
}
