using KSP.IO;
using UnityEngine;

namespace IndicatorLights
{
    internal static class Configuration
    {
        // For manually toggled LEDs
        private static readonly Color DEFAULT_TOGGLE_LED_COLOR = Color.green;
        public static readonly Color toggleLEDColor;

        // For resource indicators
        private static readonly Color DEFAULT_HIGH_RESOURCE_COLOR = Color.green;
        private static readonly Color DEFAULT_MEDIUM_RESOURCE_COLOR = Color.yellow * 0.7f;
        private static readonly Color DEFAULT_LOW_RESOURCE_COLOR = Color.red * 0.5f;
        public static readonly Color highResourceColor;
        public static readonly Color mediumResourceColor;
        public static readonly Color lowResourceColor;

        // For reaction wheels
        private static readonly Color DEFAULT_REACTION_WHEEL_PROBLEM_COLOR = Color.red;
        private static readonly Color DEFAULT_REACTION_WHEEL_NORMAL_COLOR = Color.green;
        private static readonly Color DEFAULT_REACTION_WHEEL_PILOT_ONLY_COLOR = Color.blue;
        private static readonly Color DEFAULT_REACTION_WHEEL_SAS_ONLY_COLOR = Color.yellow * 0.8f;
        public static readonly Color reactionWheelProblemColor;
        public static readonly Color reactionWheelNormalColor;
        public static readonly Color reactionWheelPilotOnlyColor;
        public static readonly Color reactionWheelSasOnlyColor;

        // For resource converters
        private static readonly Color DEFAULT_CONVERTER_ACTIVE_COLOR = Color.green;
        public static readonly Color resourceConverterActiveColor;

        // For docking ports
        private static readonly Color DEFAULT_DOCKING_CROSSFEED_ON_COLOR = Color.green * 0.7f;
        private static readonly Color DEFAULT_DOCKING_CROSSFEED_OFF_COLOR = Color.red * 0.6f;
        public static readonly Color dockingCrossfeedOnColor;
        public static readonly Color dockingCrossfeedOffColor;


        static Configuration()
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<ModuleControllableEmissive>();
            config.load();

            toggleLEDColor = ParseColor(config, "ToggleLEDColor", DEFAULT_TOGGLE_LED_COLOR);

            highResourceColor = ParseColor(config, "HighResourceColor", DEFAULT_HIGH_RESOURCE_COLOR);
            mediumResourceColor = ParseColor(config, "MediumResourceColor", DEFAULT_MEDIUM_RESOURCE_COLOR);
            lowResourceColor = ParseColor(config, "LowResourceColor", DEFAULT_LOW_RESOURCE_COLOR);

            reactionWheelProblemColor = ParseColor(config, "ReactionWheelProblemColor", DEFAULT_REACTION_WHEEL_PROBLEM_COLOR);
            reactionWheelNormalColor = ParseColor(config, "ReactionWheelNormalColor", DEFAULT_REACTION_WHEEL_NORMAL_COLOR);
            reactionWheelPilotOnlyColor = ParseColor(config, "ReactionWheelPilotOnlyColor", DEFAULT_REACTION_WHEEL_PILOT_ONLY_COLOR);
            reactionWheelSasOnlyColor = ParseColor(config, "ReactionWheelSASOnlyColor", DEFAULT_REACTION_WHEEL_SAS_ONLY_COLOR);

            resourceConverterActiveColor = ParseColor(config, "ResourceConverterActiveColor", DEFAULT_CONVERTER_ACTIVE_COLOR);

            dockingCrossfeedOnColor = ParseColor(config, "DockingCrossfeedOnColor", DEFAULT_DOCKING_CROSSFEED_ON_COLOR);
            dockingCrossfeedOffColor = ParseColor(config, "DockingCrossfeedOffColor", DEFAULT_DOCKING_CROSSFEED_OFF_COLOR);

            config.save();
        }

        private static Color ParseColor(PluginConfiguration config, string key, Color defaultColor)
        {
            string configValue = config.GetValue(key, Colors.ToString(defaultColor));
            return Colors.Parse(configValue, defaultColor);
        }
    }
}
