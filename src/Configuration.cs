using KSP.IO;
using UnityEngine;

namespace IndicatorLights
{
    internal static class Configuration
    {
        #region Private members
        // "Stealth" settings that aren't written to the config file when missing;
        // they have to be manually added by the user.
        private static bool isVerbose;

        // For manually toggled LEDs
        private static Color toggleLEDColor;

        // For resource indicators
        private static Color highResourceColor;
        private static Color mediumResourceColor;
        private static Color lowResourceColor;

        // For reaction wheels
        private static Color reactionWheelNormalColor;
        private static Color reactionWheelPilotOnlyColor;
        private static Color reactionWheelSasOnlyColor;

        // For docking ports
        private static Color dockingCrossfeedOnColor;
        private static Color dockingCrossfeedOffColor;

        // For crew status
        private static Color crewPilotColor;
        private static Color crewEngineerColor;
        private static Color crewScientistColor;
        private static Color crewTouristColor;

        // For resources
        private static Color lfoColor;
        private static Color liquidFuelColor;
        private static Color oxidizerColor;
        private static Color monopropellantColor;

        // For science instruments
        private static Color highScienceColor;
        private static Color mediumScienceColor;
        private static Color lowScienceColor;

        // General warning status
        private static Color warningColor;

        // For none-of-the-above cases
        private static Color unknownColor;
        #endregion // Private members

        public static bool IsVerbose { get { return isVerbose; } }

        // For manually toggled LEDs
        public static Color ToggleLEDColor { get { return toggleLEDColor; } }

        // For resource indicators
        public static Color HighResourceColor { get { return highResourceColor; } }
        public static Color MediumResourceColor { get { return mediumResourceColor; } }
        public static Color LowResourceColor { get { return lowResourceColor; } }

        // For reaction wheels
        public static Color ReactionWheelNormalColor { get { return reactionWheelNormalColor; } }
        public static Color ReactionWheelPilotOnlyColor { get { return reactionWheelPilotOnlyColor; } }
        public static Color ReactionWheelSasOnlyColor { get { return reactionWheelSasOnlyColor; } }

        // For docking ports
        public static Color DockingCrossfeedOnColor { get { return dockingCrossfeedOnColor; } }
        public static Color DockingCrossfeedOffColor { get { return dockingCrossfeedOffColor; } }

        // For crew status
        public static Color CrewPilotColor { get { return crewPilotColor; } }
        public static Color CrewEngineerColor { get { return crewEngineerColor; } }
        public static Color CrewScientistColor { get { return crewScientistColor; } }
        public static Color CrewTouristColor { get { return crewTouristColor; } }

        // For resources
        public static Color LfoColor { get { return lfoColor; } }
        public static Color LiquidFuelColor { get { return liquidFuelColor; } }
        public static Color OxidizerColor { get { return oxidizerColor; } }
        public static Color MonopropellantColor { get { return monopropellantColor; } }

        // For science instruments
        public static Color HighScienceColor { get { return highScienceColor; } }
        public static Color MediumScienceColor { get { return mediumScienceColor; } }
        public static Color LowScienceColor { get { return lowScienceColor; } }

        // General warning status
        public static Color WarningColor { get { return warningColor; } }

        // For none-of-the-above cases
        public static Color UnknownColor { get { return unknownColor; } }

        /// <summary>
        /// To be called once, at Start() time.
        /// </summary>
        public static void Initialize()
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
