using System;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Indicates whether science is available to be gathered in the current location.
    /// </summary>
    class ModuleScienceAvailabilityIndicator : ModuleSourceIndicator<ModuleScienceExperiment>, IToggle, IScalar
    {
        private static readonly TimeSpan UPDATE_INTERVAL = TimeSpan.FromMilliseconds(300);

        private string _subjectIdForCurrentSituation = null;
        private DateTime nextSituationUpdate = DateTime.MinValue;
        
        private IColorSource unavailableSource;
        private IColorSource lowValueSource;
        private IColorSource mediumValueSource;
        private IColorSource highValueSource;

        /// <summary>
        /// Indicates the experiment which corresponds to this indicator (mainly useful
        /// for cases where one part may have multiple science experiments on it).
        /// Optional field.  If null or empty, will simply use the first science experiment
        /// found on the part.
        /// </summary>
        [KSPField]
        public string experimentID = null;

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
        /// The color to use when no new science is available, the available science is already stored
        /// on board, or the controller is toggled off. If left blank, defers to $Off.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string unavailableColor = Colors.ToString(DefaultColor.Off);

        /// <summary>
        /// The color to use for science that's nearly worthless (e.g. because we've
        /// recovered it previously). If left blank, defers to unavailableColor.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string lowValueColor = string.Empty;

        /// <summary>
        /// The color to use for science that's partially valuable (e.g. that we've
        /// transmitted but not physically recovered).
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string mediumValueColor = string.Empty;

        /// <summary>
        /// The color to use for science that's highly valuable (because we've never
        /// recovered it before).
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string highValueColor = string.Empty;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            unavailableSource = FindColorSource(unavailableColor);
            lowValueSource = string.IsNullOrEmpty(lowValueColor) ? unavailableSource : FindColorSource(lowValueColor);
            mediumValueSource = FindColorSource(mediumValueColor);
            highValueSource = FindColorSource(highValueColor);
            nextSituationUpdate = DateTime.MinValue;
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
            get
            {
                return CurrentSource.OutputColor;
            }
        }
        
        private IColorSource CurrentSource
        {
            get
            {
                // Is it actually *possible* to get science here? If not, no light.
                if (!IsScienceAvailable) return unavailableSource;

                // What's the science here?
                string subjectId = SituationSubjectId;

                // Do we already have it on board somewhere? If so, shut up about it.
                if (AlreadyHasAboard(subjectId)) return unavailableSource;

                // Okay, how valuable is it?
                float fraction = ScienceValue.Get(subjectId);
                if (fraction == 0) return unavailableSource;
                switch (ScienceValue.FractionOf(fraction, lowScienceThreshold, highScienceThreshold))
                {
                    case ScienceValue.Fraction.High:
                        return highValueSource;
                    case ScienceValue.Fraction.Medium:
                        return mediumValueSource;
                    case ScienceValue.Fraction.Low:
                        return lowValueSource;
                    default:
                        return unavailableSource;
                }
            }
        }

        /// <summary>
        /// Gets whether science is available in the current situation.
        /// </summary>
        private bool IsScienceAvailable
        {
            get
            {
                if (vessel == null) return false;
                if (!vessel.isCommandable) return false;
                if (!SourceModule.rerunnable && SourceModule.Inoperable) return false;

                return SourceModule.experiment.IsAvailableWhile(ScienceUtil.GetExperimentSituation(vessel), vessel.mainBody);
            }
        }

        private string SituationSubjectId
        {
            get
            {
                DateTime now = DateTime.Now;
                if (now > nextSituationUpdate)
                {
                    nextSituationUpdate = now + UPDATE_INTERVAL;
                    _subjectIdForCurrentSituation = GetSubjectId(SourceModule.experiment, vessel);
                }
                return _subjectIdForCurrentSituation;
            }
        }

        /// <summary>
        /// IToggle implementation
        /// </summary>
        public bool ToggleStatus
        {
            get
            {
                return ReferenceEquals(CurrentSource, unavailableSource);
            }
        }

        /// <summary>
        /// IScalar implementation.
        /// </summary>
        public double ScalarValue
        {
            get
            {
                if (!IsScienceAvailable) return 0;
                string subjectId = SituationSubjectId;
                return AlreadyHasAboard(subjectId) ? 0 : ScienceValue.Get(subjectId);
            }
        }

        private bool AlreadyHasAboard(string subjectId)
        {
            VesselScienceContents science = VesselScienceTracker.Get(vessel);
            return (science != null) && science[subjectId];
        }

        /// <summary>
        /// Get the subject ID to use for the vessel's current situation.
        /// </summary>
        /// <param name="experiment"></param>
        /// <param name="vessel"></param>
        /// <returns></returns>
        private static string GetSubjectId(ScienceExperiment experiment, Vessel vessel)
        {
            string celestialBodyName = vessel.mainBody.name;
            ExperimentSituations situation = ScienceUtil.GetExperimentSituation(vessel);
            string biome = GetBiome(vessel);

            if (experiment.BiomeIsRelevantWhile(situation))
            {
                return experiment.id + "@" + celestialBodyName + situation.ToString() + biome.Replace(" ", string.Empty);
            }
            else
            {
                return experiment.id + "@" + celestialBodyName + situation.ToString();
            }
        }

        /// <summary>
        /// Gets the biome of the specified vessel, as used in science reports.
        /// </summary>
        /// <param name="vessel"></param>
        /// <returns></returns>
        private static string GetBiome(Vessel vessel)
        {
            if (string.IsNullOrEmpty(vessel.landedAt))
            {
                return ScienceUtil.GetExperimentBiome(vessel.mainBody, vessel.latitude, vessel.longitude);
            }
            else
            {
                // This is for the micro-biomes around KSC ("LaunchPad", etc.) If you call ScienceUtil.GetExperimentBiome,
                // it just says "Shores".
                return Vessel.GetLandedAtString(vessel.landedAt);
            }
        }

        protected override bool IsSource(ModuleScienceExperiment candidate) {
          return string.IsNullOrEmpty(experimentID) ? base.IsSource(candidate) : (experimentID.Equals(candidate.experimentID));
        }

  }
}
