using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Controls color based on the docking state (docked, undocked, engaged, disengaging).
    /// </summary>
    class ModuleDockingStateIndicator : ModuleSourceIndicator<ModuleDockingNode>
    {
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
                if (SourceModule.state.StartsWith("Acquire")) return acquire.OutputColor;
                if (SourceModule.state.StartsWith("Disengage")) return disengage.OutputColor;
                return ready.OutputColor;
            }
        }
    }
}
