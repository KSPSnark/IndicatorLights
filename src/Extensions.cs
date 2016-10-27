namespace IndicatorLights
{
    /// <summary>
    /// Convenient place for putting extension logic that doesn't particularly have an
    /// obvious home elsewhere.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Gets whether the given UI_Scene value should be enabled in the flight scene.
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static bool IsFlightEnabled(this UI_Scene scene)
        {
            switch (scene)
            {
                case UI_Scene.Editor:
                case UI_Scene.None:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Gets whether the given UI_Scene value should be enabled in the flight scene.
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static bool IsEditorEnabled(this UI_Scene scene)
        {
            switch (scene)
            {
                case UI_Scene.Flight:
                case UI_Scene.None:
                    return false;
                default:
                    return true;
            }
        }
    }
}
