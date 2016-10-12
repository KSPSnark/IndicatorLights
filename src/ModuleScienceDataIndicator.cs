namespace IndicatorLights
{
    /// <summary>
    /// Indicates whether a science experiment contains data or not. Has the ability
    /// to show different colors depending on the value of the science data.
    /// </summary>
    class ModuleScienceDataIndicator : ModuleScienceDataIndicatorBase<ModuleScienceExperiment>
    {
        /// <summary>
        /// Indicates the experiment which corresponds to this indicator (mainly useful
        /// for cases where one part may have multiple science experiments on it).
        /// Optional field.  If null or empty, will simply use the first science experiment
        /// found on the part.
        /// </summary>
        [KSPField]
        public string experimentID = null;

        protected override bool IsSource(ModuleScienceExperiment candidate)
        {
            return string.IsNullOrEmpty(experimentID) || (experimentID.Equals(candidate.experimentID));
        }
    }
}
