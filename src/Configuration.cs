using KSP.IO;
using UnityEngine;

namespace IndicatorLights
{
    internal static class Configuration
    {
        // For manually toggled LEDs
        public static readonly Color toggleLEDColor;

        // For resource indicators
        public static readonly Color highResourceColor;
        public static readonly Color mediumResourceColor;
        public static readonly Color lowResourceColor;

        // For reaction wheels
        public static readonly Color reactionWheelProblemColor;
        public static readonly Color reactionWheelNormalColor;
        public static readonly Color reactionWheelPilotOnlyColor;
        public static readonly Color reactionWheelSasOnlyColor;

        // For resource converters

        // For docking ports
        public static readonly Color dockingCrossfeedOnColor;
        public static readonly Color dockingCrossfeedOffColor;

        // For crew status
        public static readonly bool crewIndicatorDefaultStatus;
        public static readonly Color crewPilotColor;
        public static readonly Color crewEngineerColor;
        public static readonly Color crewScientistColor;
        public static readonly Color crewTouristColor;

        // For resources
        public static readonly Color lfoColor;
        public static readonly Color liquidFuelColor;
        public static readonly Color oxidizerColor;
        public static readonly Color monopropellantColor;

        static Configuration()
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<ModuleControllableEmissive>();
            config.load();

            toggleLEDColor = ParseColor(config, DefaultColor.ToggleLED);

            highResourceColor = ParseColor(config, DefaultColor.HighResource);
            mediumResourceColor = ParseColor(config, DefaultColor.MediumResource);
            lowResourceColor = ParseColor(config, DefaultColor.LowResource);

            reactionWheelProblemColor = ParseColor(config, DefaultColor.ReactionWheelProblem);
            reactionWheelNormalColor = ParseColor(config, DefaultColor.ReactionWheelNormal);
            reactionWheelPilotOnlyColor = ParseColor(config, DefaultColor.ReactionWheelPilotOnly);
            reactionWheelSasOnlyColor = ParseColor(config, DefaultColor.ReactionWheelSASOnly);

            dockingCrossfeedOnColor = ParseColor(config, DefaultColor.DockingCrossfeedOn);
            dockingCrossfeedOffColor = ParseColor(config, DefaultColor.DockingCrossfeedOff);

            crewIndicatorDefaultStatus = config.GetValue("CrewIndicatorDefaultStatus", false);
            crewPilotColor = ParseColor(config, DefaultColor.CrewPilot);
            crewEngineerColor = ParseColor(config, DefaultColor.CrewEngineer);
            crewScientistColor = ParseColor(config, DefaultColor.CrewScientist);
            crewTouristColor = ParseColor(config, DefaultColor.CrewTourist);

            liquidFuelColor = ParseColor(config, DefaultColor.ResourceLiquidFuel);
            oxidizerColor = ParseColor(config, DefaultColor.ResourceOxidizer);
            lfoColor = Color.Lerp(liquidFuelColor, oxidizerColor, 0.5f);
            monopropellantColor = ParseColor(config, DefaultColor.ResourceMonopropellant);

            config.save();
        }

        private static Color ParseColor(PluginConfiguration config, DefaultColor color)
        {
            string configValue = config.GetValue(color.ConfigurationName(), Colors.ToString(color.DefaultValue()));
            return Colors.Parse(configValue, color.DefaultValue());
        }
    }
}
