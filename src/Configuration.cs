using KSP.IO;
using UnityEngine;

namespace IndicatorLights
{
    internal static class Configuration
    {
        // "Stealth" settings that aren't written to the config file when missing;
        // they have to be manually added by the user.
        public static  bool isVerbose;

        // For manually toggled LEDs
        public static  Color toggleLEDColor;

        // For resource indicators
        public static  Color highResourceColor;
        public static  Color mediumResourceColor;
        public static  Color lowResourceColor;

        // For reaction wheels
        public static  Color reactionWheelNormalColor;
        public static  Color reactionWheelPilotOnlyColor;
        public static  Color reactionWheelSasOnlyColor;

        // For docking ports
        public static  Color dockingCrossfeedOnColor;
        public static  Color dockingCrossfeedOffColor;

        // For crew status
        public static  bool crewIndicatorDefaultStatus;
        public static  Color crewPilotColor;
        public static  Color crewEngineerColor;
        public static  Color crewScientistColor;
        public static  Color crewTouristColor;

        // For resources
        public static  Color lfoColor;
        public static  Color liquidFuelColor;
        public static  Color oxidizerColor;
        public static  Color monopropellantColor;

        // For science instruments
        public static  Color highScienceColor;
        public static  Color mediumScienceColor;
        public static  Color lowScienceColor;

        // General warning status
        public static  Color warningColor;

        // For none-of-the-above cases
        public static  Color unknownColor;

        static public void Configuration_Init()
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<ModuleControllableEmissive>();
            config.load();

            isVerbose = config.GetValue<bool>("VerboseLogging");

            toggleLEDColor = ParseColor(config, DefaultColor.ToggleLED);

            highResourceColor = ParseColor(config, DefaultColor.HighResource);
            mediumResourceColor = ParseColor(config, DefaultColor.MediumResource);
            lowResourceColor = ParseColor(config, DefaultColor.LowResource);

            reactionWheelNormalColor = ParseColor(config, DefaultColor.ReactionWheelNormal);
            reactionWheelPilotOnlyColor = ParseColor(config, DefaultColor.ReactionWheelPilotOnly);
            reactionWheelSasOnlyColor = ParseColor(config, DefaultColor.ReactionWheelSASOnly);

            dockingCrossfeedOnColor = ParseColor(config, DefaultColor.DockingCrossfeedOn);
            dockingCrossfeedOffColor = ParseColor(config, DefaultColor.DockingCrossfeedOff);

            crewIndicatorDefaultStatus = config.GetValue("CrewIndicatorDefaultStatus", true);
            crewPilotColor = ParseColor(config, DefaultColor.CrewPilot);
            crewEngineerColor = ParseColor(config, DefaultColor.CrewEngineer);
            crewScientistColor = ParseColor(config, DefaultColor.CrewScientist);
            crewTouristColor = ParseColor(config, DefaultColor.CrewTourist);

            liquidFuelColor = ParseColor(config, DefaultColor.ResourceLiquidFuel);
            oxidizerColor = ParseColor(config, DefaultColor.ResourceOxidizer);
            lfoColor = Color.Lerp(liquidFuelColor, oxidizerColor, 0.5f);
            monopropellantColor = ParseColor(config, DefaultColor.ResourceMonopropellant);

            highScienceColor = ParseColor(config, DefaultColor.HighScience);
            mediumScienceColor = ParseColor(config, DefaultColor.MediumScience);
            lowScienceColor = ParseColor(config, DefaultColor.LowScience);

            warningColor = ParseColor(config, DefaultColor.Warning);

            unknownColor = ParseColor(config, DefaultColor.Unknown);

            config.save();
        }

        private static Color ParseColor(PluginConfiguration config, DefaultColor color)
        {
            string configValue = config.GetValue(color.ConfigurationName(), Colors.ToString(color.DefaultValue()));
            return Colors.Parse(configValue, color.DefaultValue());
        }
    }
}
