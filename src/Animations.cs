using System;

namespace IndicatorLights
{
    /// <summary>
    /// Useful time functions for driving animations.
    /// </summary>
    static class Animations
    {
        /// <summary>
        /// Gets the current system time in milliseconds, useful for driving animations.
        /// </summary>
        public static long CurrentMillis
        {
            get { return DateTime.Now.Ticks / 10000L; }
        }

        /// <summary>
        /// An animation that repeatedly toggles between two states.
        /// </summary>
        public class Blink
        {
            private long onMillis;
            private long offMillis;

            private Blink(long onMillis, long offMillis)
            {
                this.onMillis = onMillis;
                this.offMillis = offMillis;
            }

            /// <summary>
            /// Get a blink animation with the specified period.
            /// </summary>
            /// <param name="onMillis"></param>
            /// <param name="offMillis"></param>
            /// <returns></returns>
            public static Blink of(long onMillis, long offMillis)
            {
                return new Blink(onMillis, offMillis);
            }

            public long OnMillis
            {
                get { return onMillis; }
                set { onMillis = value; }
            }

            public long OffMillis
            {
                get { return offMillis; }
                set { offMillis = value; }
            }

            /// <summary>
            /// Get the blink state (true = on, false = off).
            /// </summary>
            public bool State
            {
                get
                {
                    return (CurrentMillis % (onMillis + offMillis)) < onMillis;
                }
            }
        }
    }
}
