using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Indicates whether a science experiment contains data or not.
    /// </summary>
    class ModuleScienceExperimentIndicator : ModuleSourceIndicator<ModuleScienceExperiment>
    {
        private IColorSource dataSource;
        private IColorSource emptySource;

        /// <summary>
        /// The color to use when science data is available.
        /// </summary>
        [KSPField]
        public string dataColor = Colors.ToString(DefaultColor.CrewScientist);

        /// <summary>
        /// The color to use when no data is available.
        /// </summary>
        [KSPField]
        public string emptyColor = Colors.ToString(DefaultColor.Off);

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            dataSource = ColorSources.Find(part, dataColor);
            emptySource = ColorSources.Find(part, emptyColor);
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
            get { return (SourceModule.GetScienceCount() > 0) ? dataSource : emptySource; }
        }
    }
}
