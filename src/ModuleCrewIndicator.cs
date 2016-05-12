using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Controls color based on the occupancy of a seat in a crewable part.
    /// </summary>
    class ModuleCrewIndicator : ModuleEmissiveController
    {
        private static readonly int NO_SLOT = -1;

        [KSPField(isPersistant = true)]
        public int slot = NO_SLOT;

        [KSPField(isPersistant = true)]
        public string toggleName = null;

        private IToggle toggle = null;

        /// <summary>
        /// Here when we're starting up.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            toggle = Identifiers.FindFirst<IToggle>(part, toggleName);

            // The default value for slot is NO_SLOT. When we start up, we scan for all ModuleCrewIndicators
            // on the part, and assign them sequentially to slots, if available.
            if (part == null) return;
            if (part.CrewCapacity < 1) return;
            // First, go through and note which slots already have a ModuleCrewIndicator assigned to them.
            bool[] slotAssignments = new bool[part.CrewCapacity];
            for (int moduleIndex = 0; moduleIndex < part.Modules.Count; ++moduleIndex)
            {
                ModuleCrewIndicator indicator = part.Modules[moduleIndex] as ModuleCrewIndicator;
                if (indicator == null) continue;
                if ((indicator.slot < 0) || (indicator.slot >= part.CrewCapacity))
                {
                    indicator.slot = NO_SLOT;
                }
                else
                {
                    slotAssignments[indicator.slot] = true;
                }
            }
            // Next, go through and assign any unassigned ModuleCrewIndicators to any open slots.
            int slotIndex = 0;
            for (int moduleIndex = 0; moduleIndex < part.Modules.Count; ++moduleIndex)
            {
                ModuleCrewIndicator indicator = part.Modules[moduleIndex] as ModuleCrewIndicator;
                if (indicator == null) continue;
                if (indicator.slot != NO_SLOT) continue; // explicitly specifies a slot
                while (slotAssignments[slotIndex])
                {
                    ++slotIndex;
                    if (slotIndex >= part.CrewCapacity) return;
                }
                indicator.slot = slotIndex;
                slotAssignments[slotIndex] = true;
            }
        }

        public override bool HasColor
        {
            get
            {
                return (slot >= 0) && (part != null) && (slot < part.CrewCapacity);
            }
        }

        public override Color OutputColor
        {
            get
            {
                if ((toggle != null) && (!toggle.ToggleStatus)) return DefaultColor.Off.Value();

                ProtoCrewMember crew = Crew;
                if (crew == null) return DefaultColor.Off.Value();
                if ((crew.type == ProtoCrewMember.KerbalType.Tourist)
                    || (crew.experienceTrait == null)) {
                    return DefaultColor.CrewTourist.Value();
                }
                string title = crew.experienceTrait.Title;
                switch (title)
                {
                    case "Pilot":
                        return DefaultColor.CrewPilot.Value();
                    case "Engineer":
                        return DefaultColor.CrewEngineer.Value();
                    case "Scientist":
                        return DefaultColor.CrewScientist.Value();
                    default:
                        // Should never happen, but put this as a placeholder so we'll know if it does.
                        return Color.magenta;
                }
            }
        }

        /// <summary>
        /// Gets the crew member currently driving this indicator (null if the seat is empty).
        /// </summary>
        public ProtoCrewMember Crew
        {
            get
            {
                if ((part == null)
                    || (part.protoModuleCrew == null)
                    || (slot < 0)
                    || (slot >= part.protoModuleCrew.Count))
                {
                    return null;
                }
                else
                {
                    return part.protoModuleCrew[slot];
                }
            }
        }
    }
}
