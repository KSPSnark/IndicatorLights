﻿using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Picks a solid color, based on crossfeed enabled/disabled status.
    /// </summary>
    public class ModuleDockingCrossfeedIndicator : ModuleSourceIndicator<ModuleDockingNode>, IToggle
    {
        [KSPField]
        [ColorSourceIDField]
        public string crossfeedOnSource = Colors.ToString(DefaultColor.DockingCrossfeedOn);

        [KSPField]
        [ColorSourceIDField]
        public string crossfeedOffSource = Colors.ToString(DefaultColor.DockingCrossfeedOff);

        private IColorSource onSource = null;
        private IColorSource offSource = null;

        public override void ParseIDs()
        {
            base.ParseIDs();
            onSource = FindColorSource(crossfeedOnSource);
            offSource = FindColorSource(crossfeedOffSource);
        }

        public override Color OutputColor
        {
            get
            {
                if (SourceModule == null) return Color.black;
                return SourceModule.crossfeed ? onSource.OutputColor : offSource.OutputColor;
            }
        }

        /// <summary>
        /// IToggle implementation.
        /// </summary>
        public bool ToggleStatus
        {
            get
            {
                return (SourceModule == null)? false : SourceModule.crossfeed;
            }
        }
    }
}
