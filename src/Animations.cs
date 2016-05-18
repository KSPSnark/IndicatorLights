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

        /// <summary>
        /// An animation that repeatedly ramps back and forth between two values.
        /// </summary>
        public class TriangleWave
        {
            private readonly long cycleMillis;
            private readonly float level1;
            private readonly float level2;

            private TriangleWave(long cycleMillis, float level1, float level2)
            {
                this.cycleMillis = cycleMillis;
                this.level1 = level1;
                this.level2 = level2;
            }

            /// <summary>
            /// Get a triangle-wave animation with the specified cycle length.
            /// </summary>
            /// <param name="cycleMillis">Length of the cycle, in milliseconds.</param>
            /// <param name="level1">Value at one end of the cycle.</param>
            /// <param name="level2">Value at the other end of the cycle.</param>
            /// <returns></returns>
            public static TriangleWave of(long cycleMillis, float level1, float level2)
            {
                return new TriangleWave(cycleMillis, level1, level2);
            }

            /// <summary>
            /// Gets the current value of the wave.
            /// </summary>
            public float Value
            {
                get
                {
                    float phase = 2.0f * (float)(CurrentMillis % cycleMillis) / (float)(cycleMillis);
                    if (phase < 1)
                    {
                        // ramp from level1 to level2
                        return level1 + phase * (level2 - level1);
                    }
                    else
                    {
                        // ramp from level2 to level1
                        return level2 + (phase - 1f) * (level1 - level2);
                    }
                }
            }
        }
    }
}
