using System;
using System.Collections.Generic;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Shows the status of an orbital surveyor (e.g. can it take a scan or not).
    /// Includes a warning indication if no antenna is available (required for doing a scan).
    /// </summary>
    class ModuleOrbitalSurveyorIndicator : ModuleSourceIndicator<ModuleOrbitalSurveyor>
    {
        private static readonly TimeSpan ORBIT_CALCULATION_INTERVAL = TimeSpan.FromMilliseconds(200);

        private IColorSource alreadyScannedSource;
        private IColorSource potentialScanSource;
        private IColorSource readyScanSource;
        private IColorSource unusableSource;

        private int vesselPartCount = -1;
        private bool cachedHasTransmitter = false;
        private DateTime nextOrbitCheck = DateTime.MinValue;
        private bool cachedIsValidOrbit = false;

        /// <summary>
        /// The color to display when the current body has already been scanned.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string alreadyScannedColor = string.Empty;

        /// <summary>
        /// The color to display when there's a "potential" scan (i.e. we're in the SoI of a scannable
        /// body that hasn't been scanned yet, but we're not in a valid orbit around that body).
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string potentialScanColor = string.Empty;

        /// <summary>
        /// The color to display when the scanner is "ready", i.e. can take a scan right now.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string readyScanColor = string.Empty;

        /// <summary>
        /// The color to display when the scanner is unusable (for example, because there is no
        /// antenna available).
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string unusableColor = string.Empty;

        /// <summary>
        /// When set to true, the indicator will only light up when the body whose SoI it's
        /// in has terrain. (Examples of bodies that don't:  the sun; gas giants such as Jool.)
        /// </summary>
        [KSPField]
        public bool requireTerrain = true;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            alreadyScannedSource = FindColorSource(alreadyScannedColor);
            potentialScanSource = FindColorSource(potentialScanColor);
            readyScanSource = FindColorSource(readyScanColor);
            unusableSource = FindColorSource(unusableColor);
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
                // Inactive unless we're actually in the flight scene.
                if (!HighLogic.LoadedSceneIsFlight) return ColorSources.BLACK;

                // Show the "unusable" state if there's no antenna (even in the editor).
                if (!HasTransmitter) return unusableSource;

                // If the "require terrain" flag is set, make sure that the body has terrain.
                if (requireTerrain && !HasTerrain) return ColorSources.BLACK;

                // If we've already scanned the current body, go inactive.
                if (!SourceModule.Events["PerformSurvey"].active) return alreadyScannedSource;

                // Show either "potential" or "ready", depending on whether we're in a valid orbital situation.
                return IsValidOrbit ? readyScanSource : potentialScanSource;
            }
        }

        /// <summary>
        /// Gets whether a transmitter is currently available.
        /// </summary>
        private bool HasTransmitter
        {
            get
            {
                if (vessel == null)
                {
                    cachedHasTransmitter = false;
                }
                else
                {
                    if (vessel.parts.Count != vesselPartCount)
                    {
                        vesselPartCount = vessel.Parts.Count;
                        cachedHasTransmitter = FindTransmitter();
                    }
                }
                return cachedHasTransmitter;
            }
        }

        private bool HasTerrain
        {
            get
            {
                return (vessel != null) && (vessel.mainBody.pqsController != null);
            }
        }

        /// <summary>
        /// Gets whether the current orbital situation is valid for doing a scan.
        /// </summary>
        private bool IsValidOrbit
        {
            get
            {
                if (vessel == null) return false;
                DateTime now = DateTime.Now;
                if (now > nextOrbitCheck)
                {
                    nextOrbitCheck = now + ORBIT_CALCULATION_INTERVAL;

                    // And now a bunch of code which, unfortunately, is totally duplicating what
                    // ModuleOrbitalSurveyor does.  I wish that ModuleOrbitalSurveyor exposed a
                    // simple "is valid orbit" boolean function so that I could call that instead.
                    cachedIsValidOrbit = (vessel.situation == Vessel.Situations.ORBITING)
                        && (vessel.orbit.inclination >= 80.0)
                        && (vessel.orbit.PeA > Math.Max(vessel.mainBody.Radius / 10.0, SourceModule.minThreshold))
                        && (vessel.orbit.ApA < Math.Min(vessel.mainBody.Radius * 5.0, SourceModule.maxThreshold));
                }
                return cachedIsValidOrbit;
            }
        }

        /// <summary>
        /// Find whether the vessel has an available transmitter. This is an expensive function, call it sparingly.
        /// </summary>
        /// <returns></returns>
        private bool FindTransmitter()
        {
            if (vessel == null) return false;
            List<IScienceDataTransmitter> transmitters = vessel.FindPartModulesImplementing<IScienceDataTransmitter>();
            return (transmitters != null) && (transmitters.Count > 0);
        }
    }
}
