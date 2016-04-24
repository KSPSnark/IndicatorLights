using System;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Utility class for animating colors efficiently.
    /// </summary>
    internal class ColorGradient
    {
        private static readonly Color OFF_COLOR = Color.black;
        private static readonly int LEVELS = 30;
        private readonly Color fromColor;
        private readonly Color toColor;
        private readonly Color[] colorLevels;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fromColor">The color emitted for a fraction of 0</param>
        /// <param name="toColor">The color emitted for a fraction of 1</param>
        public ColorGradient(Color fromColor, Color toColor)
        {
            this.fromColor = fromColor;
            this.toColor = toColor;
            colorLevels = new Color[LEVELS + 1];
            for (int i = 0; i <= LEVELS; ++i)
            {
                float fraction = (float)i / (float)LEVELS;
                colorLevels[i] = Color.Lerp(fromColor, toColor, fraction);
            }
        }

        /// <summary>
        /// Gets the color when it's at minimum brightness.
        /// </summary>
        public Color From
        {
            get { return fromColor; }
        }

        /// <summary>
        /// Gets the color when it's at full brightness.
        /// </summary>
        public Color To
        {
            get { return toColor; }
        }

        /// <summary>
        /// Gets the faded color value for the specified fraction in the range 0 to 1.
        /// </summary>
        /// <param name="fraction"></param>
        /// <returns></returns>
        public Color this[double fraction]
        {
            get
            {
                if (fraction < 0) return fromColor;
                if (fraction > 1) return toColor;
                int index = (int)(0.5 + fraction * (double)LEVELS);
                return colorLevels[index];
            }
        }

        /// <summary>
        /// Get the color for a blinking animation of the specified cycle length.
        /// </summary>
        /// <param name="cycleLengthTicks">Length of blink cycle, in system ticks (1 millisecond = 10,000 ticks)</param>
        /// <returns></returns>
        public Color Blink(long cycleLengthTicks)
        {
            long phase = DateTime.Now.Ticks % cycleLengthTicks;
            return (phase < cycleLengthTicks / 2) ? toColor : OFF_COLOR;
        }

        /// <summary>
        /// Get the color for a smooth, fading pulse of the specified cycle length.
        /// </summary>
        /// <param name="cycleLengthTicks">Length of blink cycle, in system ticks (1 millisecond = 10,000 ticks)</param>
        /// <returns></returns>
        public Color Fade(long cycleLengthTicks)
        {
            long phase = DateTime.Now.Ticks % cycleLengthTicks;
            long halfCycle = cycleLengthTicks / 2;
            float fraction;
            if (phase < halfCycle)
            {
                fraction = 1.0f - (float)phase / (float)halfCycle;
            }
            else
            {
                fraction = (float)(phase - halfCycle) / (float)halfCycle;
            }
            int index = (int)(fraction * (float)LEVELS + 0.5);
            return colorLevels[index];
        }
    }
}
