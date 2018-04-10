using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Controls color based on the occupancy of a seat in a crewable part.
    /// </summary>
    class ModuleCrewIndicator : ModuleEmissiveController, IToggle
    {
        /// <summary>
        /// Config node where crew indicator default colors are stored. This gets read
        /// by the Loader class in this mod.
        /// </summary>
        public const string CONFIG_NODE_NAME = "CrewIndicatorDefaultColors";

        private const string KERBAL_CLASS_UNKNOWN = "Unknown";

        private static Dictionary<string, string> _kerbalClassColorDefaults = null;

        private const int NO_SLOT = -1;

        private int lastKnownPartCrewCapacity = -1;

        private IColorSource emptySource = null;
        private IColorSource otherSource = null;
        private Dictionary<string, IColorSource> colorSources;

        [KSPField(isPersistant = true)]
        [StaticField]
        public int slot = NO_SLOT;

        [KSPField]
        [ToggleIDField]
        public string toggleName = null;

        [KSPField]
        [ColorSourceIDField]
        public string emptyColor = Colors.ToString(DefaultColor.Off);

        [KSPField]
        [ColorSourceIDField]
        public string pilotColor = string.Empty; // means "default to global config"

        [KSPField]
        [ColorSourceIDField]
        public string engineerColor = string.Empty; // means "default to global config"

        [KSPField]
        [ColorSourceIDField]
        public string scientistColor = string.Empty; // means "default to global config"

        [KSPField]
        [ColorSourceIDField]
        public string touristColor = string.Empty; // means "default to global config"

        [KSPField]
        [ColorSourceIDField]
        public string otherColor = string.Empty; // means "default to global config"

        private IToggle toggle = null;

        /// <summary>
        /// Here when we're starting up.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            // The default value for slot is NO_SLOT. When we start up, we scan for all ModuleCrewIndicators
            // on the part, and assign them sequentially to slots, if available.
            if (part == null) return;
            InitializePartCrewCapacity();
        }

        public override void ParseIDs()
        {
            base.ParseIDs();
            toggle = TryFindToggle(toggleName);

            colorSources = ParseColorSources();

            emptySource = FindColorSource(emptyColor);
            otherSource = colorSources[KERBAL_CLASS_UNKNOWN];
        }

        public override bool HasColor
        {
            get
            {
                if (part == null) return false;

                // This is to cope with a part's crew capacity changing dynamically. Not something
                // that commonly happens, but it's possible. For example, the inflatable airlock
                // in "Making History" has a module that changes the part's capacity when the
                // airlock inflates or deflates.
                if (part.CrewCapacity != lastKnownPartCrewCapacity) InitializePartCrewCapacity();

                return CurrentSource.HasColor;
            }
        }

        public override Color OutputColor
        {
            get
            {
                if ((toggle != null) && (!toggle.ToggleStatus)) return DefaultColor.Off.Value();
                if ((slot < 0) || (slot >= part.CrewCapacity)) return DefaultColor.Off.Value();
                return CurrentSource.OutputColor;
            }
        }

        public override string DebugDescription
        {
            get
            {
                ProtoCrewMember occupant = Crew;
                if (occupant == null) return string.Format("slot {0} = empty", slot);
                return string.Format(
                    "slot {0} = {1} ({2}, {3})",
                    slot,
                    occupant.name,
                    occupant.type,
                    (occupant.experienceTrait == null) ? "NULL" : occupant.experienceTrait.Title);
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

        private IColorSource CurrentSource
        {
            get
            {
                // If there's no kerbal in the slot, or class can't be identified, it's the empty source.
                string kerbalClass = Kerbals.ClassOf(Crew);
                if (kerbalClass == null) return emptySource;
                // Otherwise, get the source based on the kerbal's class.
                IColorSource current;
                return colorSources.TryGetValue(kerbalClass, out current) ? current : otherSource;
            }
        }

        /// <summary>
        /// IToggle implementation.
        /// </summary>
        public bool ToggleStatus
        {
            get
            {
                return Crew != null;
            }
        }

        /// <summary>
        /// Parse a set of color sources, using fields on this module (if available) or globally
        /// configured values (if not).
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, IColorSource> ParseColorSources()
        {
            Dictionary<string, IColorSource> sources = new Dictionary<string, IColorSource>();
            if (!string.IsNullOrEmpty(pilotColor)) sources.Add(Kerbals.PilotClass, ColorSources.Find(this, pilotColor));
            if (!string.IsNullOrEmpty(engineerColor)) sources.Add(Kerbals.EngineerClass, ColorSources.Find(this, engineerColor));
            if (!string.IsNullOrEmpty(scientistColor)) sources.Add(Kerbals.ScientistClass, ColorSources.Find(this, scientistColor));
            if (!string.IsNullOrEmpty(touristColor)) sources.Add(Kerbals.TouristClass, ColorSources.Find(this, touristColor));
            if (!string.IsNullOrEmpty(otherColor)) sources.Add(KERBAL_CLASS_UNKNOWN, ColorSources.Find(this, otherColor));
            foreach (KeyValuePair<string, string> pair in _kerbalClassColorDefaults)
            {
                if (sources.ContainsKey(pair.Key)) continue;
                sources.Add(pair.Key, ColorSources.Find(this, pair.Value));
            }
            return sources;
        }

        private void InitializePartCrewCapacity()
        {
            lastKnownPartCrewCapacity = part.CrewCapacity;
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

        /// <summary>
        /// This gets called once at game load time, from the Loader class in this mod. We load our kerbal
        /// colors from this. It's assumed that it has one key for each kerbal class, whose value is the
        /// default ColorSource value to use for that class.  If there's a kerbal class that doesn't have
        /// a corresponding entry here, then it will get displayed with the "Unknown" color.
        /// </summary>
        /// <param name="config"></param>
        public static void LoadConfig(ConfigNode config)
        {
            // Read in all the configs.
            _kerbalClassColorDefaults = new Dictionary<string, string>();
            for (int i = 0; i < config.values.Count; ++i)
            {
                ConfigNode.Value entry = config.values[i];
                if (Kerbals.Classes.Contains(entry.name) || KERBAL_CLASS_UNKNOWN.Equals(entry.name))
                {
                    if (string.IsNullOrEmpty(entry.value))
                    {
                        Logging.Warn(config.name + " config: No value specified for class " + entry.name + ", skipping");
                        continue;
                    }
                    try
                    {
                        ColorSources.Validate(entry.value);
                    }
                    catch (ArgumentException e)
                    {
                        Logging.Warn(config.name + " config: Invalid value '" + entry.value + "' for class " + entry.name + ", skipping. " + e.Message);
                        continue;
                    }
                    Logging.Log(config.name + " config: " + entry.name + " = " + entry.value);
                    _kerbalClassColorDefaults.Add(entry.name, entry.value);
                }
                else
                {
                    Logging.Warn(config.name + " config: No such class '" + entry.name + "' exists, skipping");
                    continue;
                }
            }
        }

        /// <summary>
        /// Community Trait Icons integration.
        /// This gets called *after* LoadConfig, adding to / overwriting the IndicatorLights defaults.
        /// This method is only called if the Community Trait Icons mod has already been detected
        /// as being present. It's called *after* LoadConfig is called (see above), which means it will
        /// overwrite any value that's found in IndicatorLights config.
        /// </summary>
        public static IEnumerator LoadCommunityTraitIconColors()
        {
            if (!Compatibility.CTIWrapper.CTI.Loaded)
            {
                Logging.Log("Waiting for Community Trait Icons to load...");
                yield return new WaitForEndOfFrame();
            }
            if (!Compatibility.CTIWrapper.CTI.Loaded) yield break;
            Logging.Log("Obtaining crew indicator colors from Community Trait Icons.");
            foreach (string kerbalclass in Kerbals.Classes)
            {
                // Query CommunityTraitIcons for each kerbal class.
                Compatibility.CTIWrapper.KerbalTraitSetting kts = Compatibility.CTIWrapper.CTI.getTrait(kerbalclass);
                // If CTI doesn't have a setting for this class, it would return settings for "Unknown"; we should ignore such cases
                if (kerbalclass != kts.Name) continue;
                // CTI has a setting for the class. Upsert the color for this kerbal class.
                String colorString = Colors.ToString(kts.Color);
                Logging.Log("Setting " + kerbalclass + " color to " + colorString + " (from Community Trait Icons)");
                _kerbalClassColorDefaults[kerbalclass] = colorString;
            }
        }
    }
}
