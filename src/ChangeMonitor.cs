namespace IndicatorLights
{
    /// <summary>
    /// Watches a value and identifies when it has changed.
    /// </summary>
    internal class ChangeMonitor<T>
    {
        private T lastValue;

        /// <summary>
        /// Construct the monitor with an initial value.
        /// </summary>
        /// <param name="initialValue"></param>
        public ChangeMonitor(T initialValue)
        {
            lastValue = initialValue;
        }

        /// <summary>
        /// Check the monitor against a new value. Returns true if the
        /// value has changed.
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public bool Update(T newValue)
        {
            if (object.Equals(lastValue, newValue))
            {
                // Nope, no change.
                return false;
            }
            else
            {
                lastValue = newValue;
                return true;
            }
        }
    }
}
