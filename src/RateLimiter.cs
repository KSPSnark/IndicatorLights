using System;

namespace IndicatorLights
{
    /// <summary>
    /// Utility class for enabling performing an action no more frequently than
    /// a specified time interval.  (For example, if updating on every Unity frame
    /// would be too expensive, so you want to do it no more often than every
    /// N milliseconds.)
    /// </summary>
    internal class RateLimiter
    {
        private readonly TimeSpan interval;
        private DateTime nextUpdate;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="interval"></param>
        public RateLimiter(TimeSpan interval)
        {
            this.interval = interval;
            this.nextUpdate = DateTime.MinValue;
        }

        /// <summary>
        /// Resets the limiter, i.e. forces the next call to Check() to return true.
        /// </summary>
        public void Reset()
        {
            nextUpdate = DateTime.MinValue;
        }

        /// <summary>
        /// Returns true if it's never been called before, or if the time since it
        /// last returned true is at least the specified interval. Otherwise, returns false.
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            DateTime now = DateTime.Now;
            if (now > nextUpdate)
            {
                nextUpdate = now + interval;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
