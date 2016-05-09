using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// A controller that sets the display based on the empty/full percentage of a
    /// particular resource.
    /// </summary>
    class ModuleResourceIndicator : ModuleEmissiveController
    {
        private static readonly ColorGradient HIGH_COLOR = new ColorGradient(Color.black, Configuration.highResourceColor);
        private static readonly ColorGradient MEDIUM_COLOR = new ColorGradient(Color.black,Configuration.mediumResourceColor);
        private static readonly ColorGradient LOW_COLOR = new ColorGradient(Color.black, Configuration.lowResourceColor);
        private static readonly AnimateGradient CRITICAL_FADE = AnimateGradient.Fade(LOW_COLOR, 1200, 0.5);
        private static readonly AnimateGradient HIGH_BLINK = AnimateGradient.Blink(HIGH_COLOR, 900, 300);
        private static readonly AnimateGradient MEDIUM_BLINK = AnimateGradient.Blink(MEDIUM_COLOR, 900, 300);
        private static readonly AnimateGradient LOW_BLINK = AnimateGradient.Blink(LOW_COLOR, 900, 300);
        private static readonly Color EMPTY_COLOR = Color.black;

        private PartResource resource = null;

        /// <summary>
        /// The name of the resource which this controller tracks. If left null, the controller will
        /// pick the first resource it finds.
        /// </summary>
        [KSPField]
        public string resourceName = null;

        /// <summary>
        /// If resource content is above this fraction, display using the "high" color.
        /// </summary>
        [KSPField]
        public double highThreshold = 0.7;

        /// <summary>
        /// If resource content is below this fraction, display using the "low" color.
        /// </summary>
        [KSPField]
        public double lowThreshold = 0.3;

        /// <summary>
        /// If resource content is below this faction, use a pulsating animation for
        /// the light's brightness.
        /// </summary>
        [KSPField]
        public double criticalThreshold = 0.03;

        /// <summary>
        /// Called when the module is starting up.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            resource = FindResource();
            if (resource == null)
            {
                Logging.Warn("ModuleResourceIndicator is inactive");
            }
        }

        public override bool HasColor
        {
            get { return resource != null; }
        }

        public override Color OutputColor
        {
            get
            {
                if (resource.amount == 0) return Color.black;
                double fraction = resource.amount / resource.maxAmount;
                if (!resource.flowState)
                {
                    return ((fraction > highThreshold) ? HIGH_BLINK : ((fraction > lowThreshold) ? MEDIUM_BLINK : LOW_BLINK)).Color;
                }
                if (fraction < criticalThreshold) return CRITICAL_FADE.Color;
                return ((fraction > highThreshold) ? HIGH_COLOR : ((fraction > lowThreshold) ? MEDIUM_COLOR : LOW_COLOR)).To;
            }
        }

        /// <summary>
        /// Picks a resource to track.
        /// </summary>
        /// <returns></returns>
        private PartResource FindResource()
        {
            if (part == null) return null;
            if ((part.Resources == null) || (part.Resources.Count == 0))
            {
                Logging.Warn(part.GetTitle() + " has no resources, can't track");
                return null;
            }
            if ((resourceName == null) || (resourceName.Length == 0))
            {
                if (part.Resources.Count > 1)
                {
                    Logging.Log(part.GetTitle() + " has multiple resources; indicator is defaulting to " + part.Resources[0].resourceName);
                }
                return part.Resources[0];
            }
            for (int i = 0; i < part.Resources.Count; ++i)
            {
                PartResource resource = part.Resources[i];
                if (resourceName.Equals(resource.resourceName) && (resource.maxAmount > 0))
                {
                    return resource;
                }
            }
            Logging.Warn("No resource '" + resourceName + "' found in " + part.GetTitle() + ", defaulting to " + part.Resources[0].resourceName);
            return part.Resources[0];
        }
    }
}
