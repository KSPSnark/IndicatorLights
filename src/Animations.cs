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
            get
            {
                if ((HighLogic.LoadedScene == GameScenes.EDITOR) || IsTimeWarp)
                {
                    // When we're time-warping (i.e. not physics warp), we're at very high speed, so just
                    // use real-world time instead of kerbal-world time.  Otherwise the animations would
                    // get freaky fast.
                    return DateTime.Now.Ticks / 10000L;
                }
                else
                {
                    // Otherwise, use game-world time.  This means that when you turn on physics
                    // warp, animations will get faster, too.
                    return (long) (1000.0 * Planetarium.GetUniversalTime());
                }
            }
        }

        private static bool IsTimeWarp
        {
            get
            {
                return (TimeWarp.WarpMode == TimeWarp.Modes.HIGH) && (TimeWarp.CurrentRate > 1.0);
            }
        }

        #region Blink
        /// <summary>
        /// An animation that repeatedly toggles between two states.
        /// </summary>
        public class Blink
        {
            private long onMillis;
            private long offMillis;
            private float phase;
            private long phaseMillis;

            private Blink(long onMillis, long offMillis, float phase)
            {
                this.onMillis = onMillis;
                this.offMillis = offMillis;
                this.phase = phase;
                UpdatePhaseMillis();
            }

            /// <summary>
            /// Get a blink animation with the specified period.
            /// </summary>
            /// <param name="onMillis">Milliseconds spent in the "on" part of the blink cycle.</param>
            /// <param name="offMillis">Milliseconds spent in the "off" part of the blink cycle.</param>
            /// <param name="phase">Blink phase, in the range 0 to 1.</param>
            /// <returns></returns>
            public static Blink of(long onMillis, long offMillis, float phase)
            {
                return new Blink(onMillis, offMillis, phase);
            }

            public long OnMillis
            {
                get { return onMillis; }
                set { onMillis = value; UpdatePhaseMillis(); }
            }

            public long OffMillis
            {
                get { return offMillis; }
                set { offMillis = value; UpdatePhaseMillis(); }
            }

            public float Phase
            {
                get { return phase; }
                set { phase = value; UpdatePhaseMillis(); }
            }

            /// <summary>
            /// Get the blink state (true = on, false = off).
            /// </summary>
            public bool State
            {
                get
                {
                    return ((CurrentMillis + phaseMillis) % (onMillis + offMillis)) < onMillis;
                }
            }

            private void UpdatePhaseMillis()
            {
                phaseMillis = (long)((float)(onMillis + offMillis) * phase);
            }
        }
        #endregion


        #region TriangleWave
        /// <summary>
        /// An animation that repeatedly ramps back and forth between two values.
        /// </summary>
        public class TriangleWave
        {
            private readonly long cycleMillis;
            private readonly long phaseMillis;
            private readonly float level1;
            private readonly float level2;

            private TriangleWave(long cycleMillis, float level1, float level2, float phase)
            {
                this.cycleMillis = cycleMillis;
                this.level1 = level1;
                this.level2 = level2;
                phaseMillis = (long)((float)cycleMillis * phase);
            }

            /// <summary>
            /// Get a triangle-wave animation with the specified cycle length.
            /// </summary>
            /// <param name="cycleMillis">Length of the cycle, in milliseconds.</param>
            /// <param name="level1">Value at one end of the cycle.</param>
            /// <param name="level2">Value at the other end of the cycle.</param>
            /// <param name="phase">Animation phase, in the range 0 to 1.</param>
            /// <returns></returns>
            public static TriangleWave of(long cycleMillis, float level1, float level2, float phase)
            {
                return new TriangleWave(cycleMillis, level1, level2, phase);
            }

            /// <summary>
            /// Gets the current value of the wave.
            /// </summary>
            public float Value
            {
                get
                {
                    float phase = 2.0f * (float)((CurrentMillis + phaseMillis) % cycleMillis) / (float)(cycleMillis);
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
        #endregion
    }
}
