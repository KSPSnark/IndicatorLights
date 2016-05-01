using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// A simple bi-state controller that indicates whether the docking port has crossfeed enabled or disabled.
    /// </summary>
    class ModuleDockingPortIndicator : ModuleBiStateIndicator
    {
        private ModuleDockingNode docker = null;

        /// <summary>
        /// Called when the module is starting up.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            docker = FindFirst<ModuleDockingNode>();
            if (docker == null)
            {
                Logging.Warn("No docking node module found for " + part.GetTitle());
            }
        }

        protected override Color ActiveColor
        {
            get { return Configuration.dockingCrossfeedOnColor; }
        }

        protected override Color InactiveColor
        {
            get { return Configuration.dockingCrossfeedOffColor; }
        }

        public override bool HasColor
        {
            get { return docker != null; }
        }

        protected override bool State
        {
            get { return docker.crossfeed; }
        }
    }
}
