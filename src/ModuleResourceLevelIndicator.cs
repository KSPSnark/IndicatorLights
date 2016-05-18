using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// A controller that sets the display based on the empty/full percentage of a
    /// particular resource.
    /// </summary>
    class ModuleResourceLevelIndicator : ModuleResourceIndicator
    {
        private IColorSource highSource = null;
        private IColorSource mediumSource = null;
        private IColorSource lowSource = null;
        private IColorSource criticalSource = null;
        private IColorSource emptySource = null;

        /// <summary>
        /// The color to display when the resource is full or nearly full.
        /// </summary>
        [KSPField]
        public string highColor = Colors.ToString(DefaultColor.HighResource);

        /// <summary>
        /// The color to display when the resource is partially full. (Or, depending on your
        /// outlook on life, partially empty.)
        /// </summary>
        [KSPField]
        public string mediumColor = Colors.ToString(DefaultColor.MediumResource);

        /// <summary>
        /// The color to display when the resource is mostly empty.
        /// </summary>
        [KSPField]
        public string lowColor = Colors.ToString(DefaultColor.LowResource);

        /// <summary>
        /// The color to display when the resource is almost completely empty.
        /// </summary>
        [KSPField]
        public string criticalColor = ColorSources.Pulsate(ColorSources.Constant(DefaultColor.LowResource), 1200, 0.6f).ColorSourceID;

        /// <summary>
        /// The color to display when the resource is completely empty.
        /// </summary>
        [KSPField]
        public string emptyColor = Colors.ToString(DefaultColor.Off);

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

            highSource = ColorSources.Find(part, highColor);
            mediumSource = ColorSources.Find(part, mediumColor);
            lowSource = ColorSources.Find(part, lowColor);
            criticalSource = ColorSources.Find(part, criticalColor);
            emptySource = ColorSources.Find(part, emptyColor);
        }

        public override bool HasColor
        {
            get
            {
                return base.HasColor && CurrentSource.HasColor;
            }
        }

        public override Color OutputColor
        {
            get { return CurrentSource.OutputColor; }
        }

        private IColorSource CurrentSource
        {
            get
            {
                if (Resource.amount == 0) return emptySource;
                double fraction = Resource.amount / Resource.maxAmount;
                if (fraction > highThreshold) return highSource;
                if (fraction < criticalThreshold) return criticalSource;
                if (fraction < lowThreshold) return lowSource;
                return mediumSource;
            }
        }
    }
}
