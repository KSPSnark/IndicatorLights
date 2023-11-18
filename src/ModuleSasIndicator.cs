using UnityEngine;

namespace IndicatorLights
{
    public class ModuleSasIndicator : ModuleEmissiveController, IToggle
    {
        private static readonly Color COLOR_STABILITY_ASSIST = Color.white;
        private static readonly Color COLOR_PROGRADE         = new Color(1, 1, 0); // since Color.yellow is actually something else
        private static readonly Color COLOR_RADIAL           = Color.cyan;
        private static readonly Color COLOR_NORMAL           = Color.magenta;
        private static readonly Color COLOR_TARGET           = Color.magenta;
        private static readonly Color COLOR_MANEUVER         = Color.blue;

        private StartState startState = StartState.None;
        private IColorSource stabilityAssistSource = null;
        private IColorSource progradeSource = null;
        private IColorSource retrogradeSource = null;
        private IColorSource normalSource = null;
        private IColorSource antinormalSource = null;
        private IColorSource radialInSource = null;
        private IColorSource radialOutSource = null;
        private IColorSource targetSource = null;
        private IColorSource antitargetSource = null;
        private IColorSource maneuverSource = null;
        private IColorSource inactiveSource = null;

        /// <summary>
        /// The color to display when SAS is set to "stability assist" mode.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string stabilityAssistColor = Colors.ToString(COLOR_STABILITY_ASSIST);

        /// <summary>
        /// The color to display when SAS is set to "prograde" mode.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string progradeColor = Colors.ToString(COLOR_PROGRADE);

        /// <summary>
        /// The color to display when SAS is set to "retrograde" mode.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string retrogradeColor = Colors.ToString(COLOR_PROGRADE);

        /// <summary>
        /// The color to display when SAS is set to "normal" mode.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string normalColor = Colors.ToString(COLOR_NORMAL);

        /// <summary>
        /// The color to display when SAS is set to "antinormal" mode.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string antinormalColor = Colors.ToString(COLOR_NORMAL);

        /// <summary>
        /// The color to display when SAS is set to "radial in" mode.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string radialInColor = Colors.ToString(COLOR_RADIAL);

        /// <summary>
        /// The color to display when SAS is set to "radial out" mode.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string radialOutColor = Colors.ToString(COLOR_RADIAL);

        /// <summary>
        /// The color to display when SAS is set to "target" mode.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string targetColor = Colors.ToString(COLOR_TARGET);

        /// <summary>
        /// The color to display when SAS is set to "antitarget" mode.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string antitargetColor = Colors.ToString(COLOR_TARGET);

        /// <summary>
        /// The color to display when SAS is set to "maneuver" mode.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string maneuverColor = Colors.ToString(COLOR_MANEUVER);

        /// <summary>
        /// The color to display when SAS is inactive.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string inactiveColor = Colors.ToString(DefaultColor.Off);

        public override Color OutputColor => CurrentSource.OutputColor;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            startState = state;
        }

        public override void ParseIDs()
        {
            base.ParseIDs();
            stabilityAssistSource = FindColorSource(stabilityAssistColor);
            progradeSource = FindColorSource(progradeColor);
            retrogradeSource = FindColorSource(retrogradeColor);
            normalSource = FindColorSource(normalColor);
            antinormalSource = FindColorSource(antinormalColor);
            radialInSource = FindColorSource(radialInColor);
            radialOutSource = FindColorSource(radialOutColor);
            targetSource = FindColorSource(targetColor);
            antitargetSource = FindColorSource(antitargetColor);
            maneuverSource = FindColorSource(maneuverColor);
            inactiveSource = FindColorSource(inactiveColor);
        }

        private IColorSource CurrentSource
        {
            get
            {
                // in the editor, act like stability assist is on
                if (startState == StartState.Editor) return stabilityAssistSource;

                VesselAutopilot autopilot = Autopilot;
                if (!ToggleStatus) return inactiveSource;

                switch (autopilot.Mode)
                {
                    case VesselAutopilot.AutopilotMode.StabilityAssist:
                        return stabilityAssistSource;
                    case VesselAutopilot.AutopilotMode.Prograde:
                        return progradeSource;
                    case VesselAutopilot.AutopilotMode.Retrograde:
                        return retrogradeSource;
                    case VesselAutopilot.AutopilotMode.Normal:
                        return normalSource;
                    case VesselAutopilot.AutopilotMode.Antinormal:
                        return antinormalSource;
                    // yes, I know that RadialOut and RadialIn are flipped. Has to be
                    // that way to deal with a KSP bug.
                    case VesselAutopilot.AutopilotMode.RadialOut:
                        return radialInSource;
                    case VesselAutopilot.AutopilotMode.RadialIn:
                        return radialOutSource;
                    case VesselAutopilot.AutopilotMode.Target:
                        return targetSource;
                    case VesselAutopilot.AutopilotMode.AntiTarget:
                        return antitargetSource;
                    case VesselAutopilot.AutopilotMode.Maneuver:
                        return maneuverSource;
                    default:
                        // unknown enum value, should never happen unless KSP adds more modes,
                        // which will never happen
                        return ColorSources.ERROR;
                }
            }
        }

        /// <summary>
        /// IToggle implementation.
        /// </summary>
        public bool ToggleStatus
        {
            get
            {
                VesselAutopilot autopilot = Autopilot;
                return (autopilot != null) && autopilot.Enabled;
            }
        }

        private VesselAutopilot Autopilot => (vessel == null) ? null : vessel.Autopilot;
    }
}
