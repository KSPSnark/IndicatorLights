using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Picks a solid color, based on crossfeed enabled/disabled status.
    /// </summary>
    public class ModuleDockingCrossfeedIndicator : ModuleSourceIndicator<ModuleDockingNode>
    {
        [KSPField(isPersistant = true)]
        public string crossfeedOnSource = Colors.ToString(DefaultColor.DOCKING_CROSSFEED_ON);

        [KSPField(isPersistant = true)]
        public string crossfeedOffSource = Colors.ToString(DefaultColor.DOCKING_CROSSFEED_OFF);

        private IColorSource onSource = null;
        private IColorSource offSource = null;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            onSource = ColorSources.Find(part, crossfeedOnSource);
            offSource = ColorSources.Find(part, crossfeedOffSource);
        }

        public override Color OutputColor
        {
            get
            {
                return SourceModule.crossfeed ? onSource.OutputColor : offSource.OutputColor;
            }
        }
    }
}
