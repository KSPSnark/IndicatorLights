using UnityEngine;

namespace IndicatorLights
{
    class ModuleResourceScannerIndicator : ModuleSourceIndicator<ModuleResourceScanner>, IToggle
    {
        private static readonly Animations.Blink LOCKED_BLINK = Animations.Blink.of(300, 900, 0);

        private IColorSource inactiveSource;
        private IColorSource unavailableSource;
        private IColorSource lowSource;
        private IColorSource mediumSource;
        private IColorSource highSource;
        private ResourceAbundanceTracker tracker;

        /// <summary>
        /// The color to display when the scanner is inactive (i.e. can't scan right now).
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string inactiveColor = Colors.ToString(DefaultColor.Off);

        /// <summary>
        /// The color to display when the resource content is zero.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string unavailableColor = "inactiveColor";

        /// <summary>
        /// The color to display when the resource content is "low".
        /// </summary>
        [KSPField]
        public string lowResourceColor = Colors.ToString(DefaultColor.LowResource);

        /// <summary>
        /// The color to display when the resource content is "medium".
        /// </summary>
        [KSPField]
        public string mediumResourceColor = Colors.ToString(DefaultColor.MediumResource);

        /// <summary>
        /// The color to display when the resource content is "high".
        /// </summary>
        [KSPField]
        public string highResourceColor = Colors.ToString(DefaultColor.HighResource);

        /// <summary>
        /// The resource abundance below which we treat it as having zero availability.
        /// </summary>
        [KSPField]
        public double unavailableResourceThreshold = 1E-03;

        /// <summary>
        /// The resource abundance threshold between "low" and "medium". The default value of
        /// 0.025 is chosen specifically to align with the minimum mining threshold of the mini-drill.
        /// </summary>
        [KSPField]
        public double lowResourceThreshold = 0.025;

        /// <summary>
        /// The resource abundance threshold between "medium" and "high".
        /// </summary>
        [KSPField]
        public double highResourceThreshold = 0.05;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            inactiveSource = FindColorSource(inactiveColor);
            unavailableSource = FindColorSource(unavailableColor);
            lowSource = FindColorSource(lowResourceColor);
            mediumSource = FindColorSource(mediumResourceColor);
            highSource = FindColorSource(highResourceColor);

            tracker = new ResourceAbundanceTracker(SourceModule);
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            tracker.OnFixedUpdate(vessel);
        }

        public override bool HasColor
        {
            get
            {
                return CurrentSource.HasColor;
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
                // No scanning in the editor!
                if (!HighLogic.LoadedSceneIsFlight) return inactiveSource;

                // Can we scan, based on where we're currently located?
                bool canScan;
                HarvestTypes scannerType = (HarvestTypes)SourceModule.ScannerType;
                switch (scannerType)
                {
                    case HarvestTypes.Planetary:
                        canScan = vessel.Landed || (ResourceUtilities.GetAltitude(vessel) <= SourceModule.MaxAbundanceAltitude);
                        break;

                    case HarvestTypes.Oceanic:
                        canScan = vessel.Splashed;
                        break;

                    case HarvestTypes.Atmospheric:
                        canScan = vessel.mainBody.atmosphere;
                        break;

                    case HarvestTypes.Exospheric:
                        canScan = true;
                        break;

                    default:
                        // Unknown scanner type! This should never happen; basically the only way is
                        // if Squad has updated the HarvestTypes enum since this code was written,
                        // to include new scanner types (in which case this code needs to be updated
                        // to take the new types into account).
                        return ColorSources.ERROR;
                }
                if (!canScan) return inactiveSource;

                // Scanning is available. Have we unlocked the current biome?
                bool isUnlocked = true;
                if (scannerType == HarvestTypes.Planetary)
                {
                    // It's a planetary scanner, so we have to care about biome.
                    string biomeName = vessel.GetCurrentBiome() ?? ResourceMap.GetDefaultSituation(scannerType);
                    isUnlocked = ResourceMap.Instance.IsBiomeUnlocked(vessel.mainBody.flightGlobalsIndex, biomeName);
                }

                // Now pick a source based on resource abundance.
                IColorSource abundanceSource;
                if (tracker.Abundance < unavailableResourceThreshold)
                {
                    abundanceSource = unavailableSource;
                }
                else if (tracker.Abundance < lowResourceThreshold)
                {
                    abundanceSource = lowSource;
                }
                else if (tracker.Abundance < highResourceThreshold)
                {
                    abundanceSource = mediumSource;
                }
                else
                {
                    abundanceSource = highSource;
                }

                return isUnlocked ? abundanceSource : (LOCKED_BLINK.State ? abundanceSource : ColorSources.BLACK);
            }
        }

        /// <summary>
        /// IToggle implementation.
        /// </summary>
        public bool ToggleStatus
        {
            get
            {
                // Toggle status is considered "active" if it can scan and there's anything available.
                if (!HighLogic.LoadedSceneIsFlight) return false;
                HarvestTypes scannerType = (HarvestTypes)SourceModule.ScannerType;
                bool canScan = false;
                switch (scannerType)
                {
                    case HarvestTypes.Planetary:
                        canScan = vessel.Landed || (ResourceUtilities.GetAltitude(vessel) <= SourceModule.MaxAbundanceAltitude);
                        break;

                    case HarvestTypes.Oceanic:
                        canScan = vessel.Splashed;
                        break;

                    case HarvestTypes.Atmospheric:
                        canScan = vessel.mainBody.atmosphere;
                        break;

                    case HarvestTypes.Exospheric:
                        canScan = true;
                        break;

                    default:
                        // Unknown scanner type! This should never happen; basically the only way is
                        // if Squad has updated the HarvestTypes enum since this code was written,
                        // to include new scanner types (in which case this code needs to be updated
                        // to take the new types into account).
                        return false;
                }
                return canScan && (tracker.Abundance >= unavailableResourceThreshold);
            }
        }


        #region class ResourceAbundanceTracker
        /// <summary>
        /// Utility class for tracking resource abundance; unfortunately necessary because the
        /// ModuleResourceScanner doesn't publicly expose its abundanceValue member.
        /// </summary>
        private class ResourceAbundanceTracker
        {
            private AbundanceRequest request;
            private double abundance;

            public ResourceAbundanceTracker(ModuleResourceScanner scanner)
            {
                request = new AbundanceRequest();
                request.CheckForLock = scanner.RequiresUnlock;
                request.ResourceType = (HarvestTypes)scanner.ScannerType;
                request.ResourceName = scanner.ResourceName;

                abundance = 0;
            }

            public double Abundance
            {
                get { return abundance; }
            }

            public void OnFixedUpdate(Vessel vessel)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    request.Altitude = vessel.altitude;
                    request.BodyId = vessel.mainBody.flightGlobalsIndex;
                    request.Latitude = vessel.latitude;
                    request.Longitude = vessel.longitude;
                    abundance = ResourceMap.Instance.GetAbundance(request);
                }
                else
                {
                    abundance = 0;
                }
            }
        }
        #endregion // ResourceAbundanceTracker
    }
}
