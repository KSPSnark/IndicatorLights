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

        // The default color source to use for high-value science, if not specified in config.
        // blink($HighScience, 200, $Off, 200)
        private static readonly IColorSource DEFAULT_HIGH_VALUE_SOURCE = ColorSources.Blink(
            ColorSources.Constant(DefaultColor.HighScience), 200,
            ColorSources.Constant(DefaultColor.Off), 200);

        // The default color source to use for medium-value science, if not specified in config.
        // blink(dim($MediumScience, 0.9), 150, $Off, 1050)
        private static readonly IColorSource DEFAULT_MEDIUM_VALUE_SOURCE = ColorSources.Blink(
            ColorSources.Dim(ColorSources.Constant(DefaultColor.MediumScience), 0.9f), 150,
            ColorSources.Constant(DefaultColor.Off), 1050);

        private string _subjectIdForCurrentSituation = null;
        private DateTime nextSituationUpdate = DateTime.MinValue;

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
        [StaticField]
        public float lowScienceThreshold = ScienceValue.DEFAULT_LOW_SCIENCE_THRESHOLD;

        /// <summary>
        /// Science value (as a fraction from 0 to 1) above which it should be treated as "high value".
        /// </summary>
        [KSPField]
        [StaticField]
        public float highScienceThreshold = ScienceValue.DEFAULT_HIGH_SCIENCE_THRESHOLD;

        /// <summary>
        /// The color to use for science that's unavailable (or has a value nearly zero).
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string lowValueColor = Colors.ToString(DefaultColor.Off);

        /// <summary>
        /// The color to use for science that's partially valuable (e.g. that we've
        /// transmitted but not physically recovered).
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string mediumValueColor = DEFAULT_MEDIUM_VALUE_SOURCE.ColorSourceID;

        /// <summary>
        /// The color to use for science that's highly valuable (because we've never
        /// recovered it before).
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string highValueColor = DEFAULT_HIGH_VALUE_SOURCE.ColorSourceID;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            nextSituationUpdate = DateTime.MinValue;
        }

        public override void ParseIDs()
        {
            base.ParseIDs();
            lowValueSource = FindColorSource(lowValueColor);
            mediumValueSource = FindColorSource(mediumValueColor);
            highValueSource = FindColorSource(highValueColor);
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

        protected override bool IsSource(ModuleScienceExperiment candidate)
        {
            return string.IsNullOrEmpty(experimentID) || (experimentID.Equals(candidate.experimentID));
        }

        private IColorSource CurrentSource
        {
            get
            {
                // Is it actually *possible* to get science here? If not, no light.
                if (!ToggleStatus) return lowValueSource;

                // What's the science here?
                string subjectId = SituationSubjectId;

                // Do we already have it on board somewhere? If so, shut up about it.
                if (AlreadyHasAboard(subjectId)) return lowValueSource;

                // Okay, how valuable is it?
                ScienceValue.Fraction fraction = ScienceValue.Get(subjectId, lowScienceThreshold, highScienceThreshold);
                switch (fraction)
                {
                    case ScienceValue.Fraction.High:
                        return highValueSource;
                    case ScienceValue.Fraction.Medium:
                        return mediumValueSource;
                    default:
                        return lowValueSource;
                }
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
        /// IToggle implementation. Gets whether science is available in the current situation.
        /// </summary>
        public bool ToggleStatus
        {
            get
            {
                if (vessel == null) return false;
                if (!vessel.isCommandable) return false;
                if (!SourceModule.rerunnable && SourceModule.Inoperable) return false;

                return SourceModule.experiment.IsAvailableWhile(ScienceUtil.GetExperimentSituation(vessel), vessel.mainBody);
            }
        }

        /// <summary>
        /// IScalar implementation.
        /// </summary>
        public double ScalarValue
        {
            get
            {
                if (!ToggleStatus) return 0;
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
    }
}
