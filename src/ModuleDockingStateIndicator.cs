using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Controls color based on the docking state (docked, undocked, engaged, disengaging).
    /// </summary>
    class ModuleDockingStateIndicator : ModuleSourceIndicator<ModuleDockingNode>
    {
        [KSPField(isPersistant = true)]
        public string readyColor = null;

        [KSPField(isPersistant = true)]
        public string acquireColor = null;

        [KSPField(isPersistant = true)]
        public string disengageColor = null;

        private IColorSource ready = null;
        private IColorSource acquire = null;
        private IColorSource disengage = null;


        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            ready = ColorSources.Find(part, readyColor);
            acquire = ColorSources.TryFind(part, acquireColor);
            disengage = ColorSources.TryFind(part, disengageColor);
        }

        public override Color OutputColor
        {
            get
            {
                if (string.IsNullOrEmpty(SourceModule.state)) return ready.OutputColor;
                if (SourceModule.state.StartsWith("Acquire")) return (acquire == null) ? ready.OutputColor : acquire.OutputColor;
                if (SourceModule.state.StartsWith("Disengage")) return (disengage == null) ? ready.OutputColor : disengage.OutputColor;
                return ready.OutputColor;
            }
        }
    }
}
