namespace IndicatorLights
{
    /// <summary>
    /// Stores important settings that apply across the whole mod, and which can be modified
    /// at run time.
    /// </summary>
    internal static class GlobalSettings
    {
        private static bool isEnabled = true;

        /// <summary>
        /// Gets or sets whether all lights everywhere are enabled, across the module.
        /// True by default. When set to false, all lights go dark.
        /// </summary>
        public static bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }
    }
}
