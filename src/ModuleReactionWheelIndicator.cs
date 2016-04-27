using System;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// A module that sets the display based on the current status of a reaction wheel.
    /// </summary>
    class ModuleReactionWheelIndicator : ModuleEmissiveController
    {
        private static readonly Color OFF_COLOR = Color.black;
        private static readonly ColorGradient PROBLEM_GRADIENT = new ColorGradient(Color.black, Configuration.reactionWheelProblemColor);
        private static readonly ColorGradient NORMAL_GRADIENT = new ColorGradient(Color.black, Configuration.reactionWheelNormalColor);
        private static readonly ColorGradient PILOT_ONLY_GRADIENT = new ColorGradient(Color.black, Configuration.reactionWheelPilotOnlyColor);
        private static readonly ColorGradient SAS_ONLY_GRADIENT = new ColorGradient(Color.black, Configuration.reactionWheelSasOnlyColor);

        private static readonly int STARVED_BLINK_MILLIS = 250;
        private static readonly AnimateGradient BROKEN_ANIMATION = AnimateGradient.Blink(PROBLEM_GRADIENT, 100, 1100);
        private static readonly AnimateGradient NORMAL_STARVED = AnimateGradient.Blink(NORMAL_GRADIENT, STARVED_BLINK_MILLIS, STARVED_BLINK_MILLIS);
        private static readonly AnimateGradient PILOT_ONLY_STARVED = AnimateGradient.Blink(PILOT_ONLY_GRADIENT, STARVED_BLINK_MILLIS, STARVED_BLINK_MILLIS);
        private static readonly AnimateGradient SAS_ONLY_STARVED = AnimateGradient.Blink(SAS_ONLY_GRADIENT, STARVED_BLINK_MILLIS, STARVED_BLINK_MILLIS);

        private ModuleReactionWheel wheel = null;
        private StartState startState = StartState.None;

        internal override string EditorGuiDescription
        {
            get { return "Reaction Wheel Status"; }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            this.startState = state;
            this.wheel = FindReactionWheel();
        }

        void Update()
        {
            if (wheel == null) return;
            Color = WheelStateColor;
        }

        /// <summary>
        /// Gets the color to use for indicating the wheel, based on its current state.
        /// </summary>
        private Color WheelStateColor
        {
            get
            {
                switch (wheel.State)
                {
                    case ModuleReactionWheel.WheelState.Disabled:
                        return OFF_COLOR;
                    case ModuleReactionWheel.WheelState.Broken:
                        return BROKEN_ANIMATION.Color;
                    default:
                        break;
                }
                AnimateGradient animation = CurrentAnimation; // this is the "deprived" animation
                if (IsDeprived) return animation.Color; // use the animated color, because we're deprived
                // Not deprived, so either pick the "full on" color or the "halfway on" color, depending
                // on whether autopilot is active or not.
                return IsAutopilotActive ? animation.Gradient.To : animation.Gradient[0.5f];
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
                if (startState == StartState.Editor) return true;
                Vessel vessel = FlightGlobals.ActiveVessel;
                if (vessel == null) return true;
                return vessel.Autopilot.Enabled;
            }
        }

        private bool IsDeprived
        {
            get
            {
                if (startState == StartState.Editor) return false;
                if ((wheel == null) || (wheel.inputResources == null)) return false;
                for (int i = 0; i < wheel.inputResources.Count; ++i)
                {
                    ModuleResource resource = wheel.inputResources[i];
                    // I'm using !available rather than isDeprived, because as far as I can tell, isDeprived is always false
                    if (!resource.available) return true;
                }
                return false;
            }
        }

        private AnimateGradient CurrentAnimation
        {
            get
            {
                if (wheel == null) return NORMAL_STARVED;
                switch ((VesselActuatorMode)wheel.actuatorModeCycle)
                {
                    case VesselActuatorMode.Pilot:
                        return PILOT_ONLY_STARVED;
                    case VesselActuatorMode.SAS:
                        return SAS_ONLY_STARVED;
                    default:
                        return NORMAL_STARVED;
                }
            }
        }

        private ModuleReactionWheel FindReactionWheel()
        {
            if (part == null) return null;
            for (int i = 0; i < part.Modules.Count; ++i)
            {
                ModuleReactionWheel candidate = part.Modules[i] as ModuleReactionWheel;
                if (candidate != null) return candidate;
            }
            Logging.Warn("No ModuleReactionWheel found for " + part.GetTitle() + "; ModuleReactionWheelIndicator is inactive");
            return null;
        }
    }
}
