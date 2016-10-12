using System;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Indicates whether a science container contains data or not. Has the ability
    /// to show different colors depending on the value of the science data.
    /// </summary>
    abstract class ModuleScienceDataIndicatorBase<T> : ModuleSourceIndicator<T>, IToggle, IScalar where T : PartModule, IScienceDataContainer
    {
        private static readonly TimeSpan UPDATE_INTERVAL = new TimeSpan(0, 0, 0, 0, 300);

        private IColorSource dataSource;
        private IColorSource partialDataSource;
        private IColorSource lowDataSource;
        private IColorSource emptySource;
        private DateTime nextUpdate;
        private ScienceValue.Fraction? _scienceFraction;

        /// <summary>
        /// Science value (as a fraction from 0 to 1) below which it should be treated as "low value".
        /// </summary>
        [KSPField]
        public float lowScienceThreshold = ScienceValue.DEFAULT_LOW_SCIENCE_THRESHOLD;

        /// <summary>
        /// Science value (as a fraction from 0 to 1) above which it should be treated as "high value".
        /// </summary>
        [KSPField]
        public float highScienceThreshold = ScienceValue.DEFAULT_HIGH_SCIENCE_THRESHOLD;

        /// <summary>
        /// The color to use when science data is available.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string dataColor = Colors.ToString(DefaultColor.CrewScientist);

        /// <summary>
        /// The color to use when science data is available, but it's only partly useful
        /// (e.g. if we've previously transmitted it). If left blank, defers to dataColor.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string partialDataColor = string.Empty;

        /// <summary>
        /// The color to use when science data is available, but it's nearly worthless
        /// (e.g. because we've recovered it previously). If left blank, defers to dataColor.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string lowDataColor = string.Empty;

        /// <summary>
        /// The color to use when no data is available.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string emptyColor = Colors.ToString(DefaultColor.Off);

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            dataSource = FindColorSource(dataColor);
            partialDataSource = string.IsNullOrEmpty(partialDataColor) ? null : FindColorSource(partialDataColor);
            lowDataSource = string.IsNullOrEmpty(lowDataColor) ? null : FindColorSource(lowDataColor);
            emptySource = FindColorSource(emptyColor);
            nextUpdate = DateTime.MinValue;
            _scienceFraction = null;
        }

        public override bool HasColor
        {
            get { return base.HasColor && CurrentSource.HasColor; }
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
                if ((partialDataSource == null) || (lowDataSource == null)) return dataSource;
                switch (ScienceFraction)
                {
                    case ScienceValue.Fraction.Low:
                        return lowDataSource;
                    case ScienceValue.Fraction.Medium:
                        return partialDataSource;
                    default:
                        return dataSource;
                }
            }
        }

        /// <summary>
        /// Gets the current science fraction value stored in the container
        /// (cached for performance).
        /// </summary>
        private ScienceValue.Fraction? ScienceFraction
        {
            get
            {
                DateTime now = DateTime.Now;
                if (now > nextUpdate)
                {
                    _scienceFraction = GetCurrentFraction();
                    nextUpdate = now + UPDATE_INTERVAL;
                }
                return _scienceFraction;
            }
        }

        public bool ToggleStatus
        {
            get
            {
                return SourceModule.GetScienceCount() > 0;
            }
        }

        public double ScalarValue
        {
            get
            {
                if (!ToggleStatus) return 0;
                double bestFraction = 0;
                if (ToggleStatus)
                {
                    ScienceData[] data = SourceModule.GetData();
                    for (int i = 0; i < data.Length; ++i)
                    {
                        string subjectId = data[i].subjectID;
                        double fraction = ScienceValue.Get(data[i].subjectID);
                        if (fraction > bestFraction)
                        {
                            bestFraction = fraction;
                        }
                    }
                }
                return bestFraction;
            }
        }

        /// <summary>
        /// Gets the current science fraction value stored in the container
        /// (determined live, not cached).
        /// </summary>
        private ScienceValue.Fraction? GetCurrentFraction()
        {
            double value = ScalarValue;
            if (value == 0) return null;
            return ScienceValue.FractionOf((float)value, lowScienceThreshold, highScienceThreshold);
        }
    }
}
