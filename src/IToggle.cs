namespace IndicatorLights
{
    /// <summary>
    /// A simple interface for toggle controllers.
    /// </summary>
    public interface IToggle
    {
        /// <summary>
        /// The current on/off status of the toggle.
        /// </summary>
        bool ToggleStatus { get; }
    }
}
