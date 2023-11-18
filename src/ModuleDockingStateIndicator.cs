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


        public override void ParseIDs()
        {
            base.ParseIDs();
            ready = FindColorSource(readyColor);
            acquire = FindColorSource(acquireColor);
            disengage = FindColorSource(disengageColor);
        }

        public override Color OutputColor
        {
            get
            {
                if (SourceModule == null) return ready.OutputColor;
                string state = SourceModule.state;
                if (string.IsNullOrEmpty(state)) return ready.OutputColor;
                if (state.StartsWith(ACQUIRE)) return acquire.OutputColor;
                if (state.StartsWith(DISENGAGE)) return disengage.OutputColor;
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
                if (SourceModule == null) return false;
                string state = SourceModule.state;
                return string.IsNullOrEmpty(state)
                    || state.StartsWith(ACQUIRE)
                    || state.StartsWith(DISENGAGE);
            }
        }
    }
}
