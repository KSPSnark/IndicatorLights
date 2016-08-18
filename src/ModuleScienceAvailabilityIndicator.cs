using System;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Indicates whether science is available to be gathered in the current location.
    /// </summary>
    class ModuleScienceAvailabilityIndicator : ModuleSourceIndicator<ModuleScienceExperiment>, IToggle
    {
        private static readonly TimeSpan UPDATE_INTERVAL = TimeSpan.FromMilliseconds(300);
        private static readonly IColorSource NO_SCIENCE_SOURCE = ColorSources.BLACK;

        private string _subjectIdForCurrentSituation = null;
        private DateTime nextSituationUpdate = DateTime.MinValue;
        
        private IColorSource mediumValueSource;
        private IColorSource highValueSource;

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
                if (!IsScienceAvailable) return NO_SCIENCE_SOURCE;

                // What's the science here?
                string subjectId = SituationSubjectId;

                // Do we already have it on board somewhere? If so, shut up about it.
                VesselScienceContents science = VesselScienceTracker.Get(vessel);
                if ((science != null) && science[subjectId]) return NO_SCIENCE_SOURCE;

                // Okay, how valuable is it?
                ScienceValue.Fraction fraction = ScienceValue.Get(subjectId, lowScienceThreshold, highScienceThreshold);
                switch (fraction)
                {
                    case ScienceValue.Fraction.High:
                        return highValueSource;
                    case ScienceValue.Fraction.Medium:
                        return mediumValueSource;
                    default:
                        return NO_SCIENCE_SOURCE;
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
                return ReferenceEquals(CurrentSource, NO_SCIENCE_SOURCE);
            }
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
    }
}
