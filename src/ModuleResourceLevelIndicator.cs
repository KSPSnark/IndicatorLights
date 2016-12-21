using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// A controller that sets the display based on the empty/full percentage of a
    /// particular resource.
    /// </summary>
    class ModuleResourceLevelIndicator : ModuleResourceIndicator, IToggle, IScalar
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
        [ColorSourceIDField]
        public string highColor = Colors.ToString(DefaultColor.HighResource);

        /// <summary>
        /// The color to display when the resource is partially full. (Or, depending on your
        /// outlook on life, partially empty.)
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string mediumColor = Colors.ToString(DefaultColor.MediumResource);

        /// <summary>
        /// The color to display when the resource is mostly empty.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string lowColor = Colors.ToString(DefaultColor.LowResource);

        /// <summary>
        /// The color to display when the resource is almost completely empty.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string criticalColor = ColorSources.Pulsate(ColorSources.Constant(DefaultColor.LowResource), 1200, 1f, 0.6f).ColorSourceID;

        /// <summary>
        /// The color to display when the resource is completely empty.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string emptyColor = Colors.ToString(DefaultColor.Off);

        /// <summary>
        /// If resource content is above this fraction, display using the "high" color.
        /// </summary>
        [KSPField]
        [StaticField]
        public double highThreshold = 0.7;

        /// <summary>
        /// If resource content is below this fraction, display using the "low" color.
        /// </summary>
        [KSPField]
        [StaticField]
        public double lowThreshold = 0.3;

        /// <summary>
        /// If resource content is below this faction, use a pulsating animation for
        /// the light's brightness.
        /// </summary>
        [KSPField]
        [StaticField]
        public double criticalThreshold = 0.03;

        public override void ParseIDs()
        {
            base.ParseIDs();
            highSource = FindColorSource(highColor);
            mediumSource = FindColorSource(mediumColor);
            lowSource = FindColorSource(lowColor);
            criticalSource = FindColorSource(criticalColor);
            emptySource = FindColorSource(emptyColor);
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
                if (!ToggleStatus) return emptySource;
                double fraction = ScalarValue;
                if (fraction > highThreshold) return highSource;
                if (fraction < criticalThreshold) return criticalSource;
                if (fraction < lowThreshold) return lowSource;
                return mediumSource;
            }
        }

        /// <summary>
        /// IToggle implementation.
        /// </summary>
        public bool ToggleStatus
        {
            get
            {
                PartResource resource = Resource;
                return (resource != null) && (resource.amount > 0.0);
            }
        }

        /// <summary>
        /// IScalar implementation.
        /// </summary>
        public double ScalarValue
        {
            get
            {
                PartResource resource = Resource;
                return (resource == null) ? 0.0 : resource.amount / resource.maxAmount;
            }
        }
    }
}
