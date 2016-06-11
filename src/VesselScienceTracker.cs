
using System;

namespace IndicatorLights
{
    /// <summary>
    /// Utility class for tracking which science subjects are contained in vessels.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class VesselScienceTracker : VesselRegistrar<VesselScienceContents>
    {
        private static VesselScienceTracker instance = null;

        public void Start()
        {
            instance = this;
        }

        /// <summary>
        /// Gets the science contents information for the specified vessel, or null if not found.
        /// </summary>
        /// <param name="vessel"></param>
        /// <returns></returns>
        public static VesselScienceContents Get(Vessel vessel)
        {
            return (instance == null) ? null : instance[vessel];
        }

        /// <summary>
        /// Here when a vessel is added.
        /// </summary>
        /// <param name="vessel"></param>
        /// <returns></returns>
        protected override VesselScienceContents OnAddVessel(Vessel vessel)
        {
            return new VesselScienceContents(vessel);
        }

        /// <summary>
        /// Here when a vessel is modified.
        /// </summary>
        /// <param name="vessel"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override VesselScienceContents OnModifyVessel(Vessel vessel, VesselScienceContents data)
        {
            return new VesselScienceContents(vessel);
        }

        private void OnVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> data)
        {
            throw new NotImplementedException();
        }
    }
}
