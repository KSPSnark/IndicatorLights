using System;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Controls color based on the docking state (docked, undocked, engaged, disengaging).
    /// </summary>
    class ModuleDockingStateIndicator : ModuleSourceIndicator<ModuleDockingNode>, IToggle
    {
        private const string ACQUIRE = "Acquire";
        private const string DISENGAGE = "Disengage";

        [KSPField]
        [ColorSourceIDField]
        public string readyColor = string.Empty;

        [KSPField]
        [ColorSourceIDField]
        public string acquireColor = string.Empty;

        [KSPField]
        [ColorSourceIDField]
        public string disengageColor = string.Empty;

        private IColorSource ready = null;
        private IColorSource acquire = null;
        private IColorSource disengage = null;


        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            ready = FindColorSource(readyColor);
            acquire = FindColorSource(acquireColor);
            disengage = FindColorSource(disengageColor);
        }

        public override Color OutputColor
        {
            get
            {
                if (string.IsNullOrEmpty(SourceModule.state)) return ready.OutputColor;
                if (SourceModule.state.StartsWith(ACQUIRE)) return acquire.OutputColor;
                if (SourceModule.state.StartsWith(DISENGAGE)) return disengage.OutputColor;
                return ready.OutputColor;
            }
        }

        /// <summary>
        /// IToggle implementation.
        /// </summary>
        public bool ToggleStatus
        {
            get
            {
                // Toggle is considered "on" when we're engaging or disengaging, off at all other times.
                return string.IsNullOrEmpty(SourceModule.state)
                    || SourceModule.state.StartsWith(ACQUIRE)
                    || SourceModule.state.StartsWith(DISENGAGE);
            }
        }
    }
}
