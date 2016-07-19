using System;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Indicates whether a science experiment contains data or not. Has the ability
    /// to show different colors depending on the value of the science data.
    /// </summary>
    class ModuleScienceDataIndicator : ModuleSourceIndicator<ModuleScienceExperiment>
    {
        private static readonly TimeSpan UPDATE_INTERVAL = new TimeSpan(0, 0, 0, 0, 500);

        private IColorSource dataSource;
        private IColorSource partialDataSource;
        private IColorSource lowDataSource;
        private IColorSource emptySource;
        private DateTime nextUpdate;
        private string _subjectId;

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
            _subjectId = GetData();
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
                if (SourceModule.GetScienceCount() == 0) return emptySource;
                if ((partialDataSource == null) || (lowDataSource == null)) return dataSource;
                string subjectId = SubjectId;
                if (subjectId == null) return dataSource;
                ScienceValue.Fraction fraction = ScienceValue.Get(subjectId, lowScienceThreshold, highScienceThreshold);
                switch (fraction)
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

        private string SubjectId
        {
            get
            {
                DateTime now = DateTime.Now;
                if (now > nextUpdate)
                {
                    _subjectId = GetData();
                    nextUpdate = now + UPDATE_INTERVAL;
                }
                return _subjectId;
            }
        }

        /// <summary>
        /// Gets the subject ID of data stored in the experiment, or null if it's empty.
        /// </summary>
        /// <returns></returns>
        private string GetData()
        {
            if (SourceModule.GetScienceCount() == 0) return null;
            ScienceData data = SourceModule.GetData()[0];
            return data.subjectID;
        }
    }
}
