using System;
using UnityEngine;

namespace IndicatorLights
{
    internal abstract class AnimateGradient
    {
        private readonly ColorGradient gradient;

        /// <summary>
        /// Gets a blink animation.
        /// </summary>
        /// <param name="gradient">The color gradient for the blink.</param>
        /// <param name="onMillis">Milliseconds in the "on" phase.</param>
        /// <param name="offMillis">Milliseconds in the "off" phase.</param>
        /// <returns></returns>
        public static AnimateGradient Blink(ColorGradient gradient, long onMillis, long offMillis)
        {
            return new AnimateBlink(gradient, onMillis, offMillis);
        }

        /// <summary>
        /// Gets a fade animation.
        /// </summary>
        /// <param name="gradient"></param>
        /// <param name="cycleLengthMillis"></param>
        /// <param name="fadeDepth"></param>
        /// <returns></returns>
        public static AnimateGradient Fade(ColorGradient gradient, long cycleLengthMillis, double fadeDepth)
        {
            return new AnimateFade(gradient, cycleLengthMillis, fadeDepth);
        }

        protected AnimateGradient(ColorGradient gradient)
        {
            this.gradient = gradient;
        }

        /// <summary>
        /// Gets the current color of the animation.
        /// </summary>
        public Color Color
        {
            get { return gradient[Phase]; }
        }

        /// <summary>
        /// Gets the current phase of the animation, from 0 to 1.
        /// </summary>
        protected abstract double Phase { get; }

        /// <summary>
        /// Gets the current system time in milliseconds.
        /// </summary>
        protected long CurrentMillis
        {
            get { return DateTime.Now.Ticks / 10000L; }
        }

        /// <summary>
        /// An animation of an on/off blinking pattern.
        /// </summary>
        private class AnimateBlink : AnimateGradient
        {
            private readonly long cycleLength;
            private readonly long onLength;

            public AnimateBlink(ColorGradient gradient, long onMillis, long offMillis) : base(gradient)
            {
                this.onLength = onMillis;
                this.cycleLength = onMillis + offMillis;
            }

            protected override double Phase
            {
                get
                {
                    return ((CurrentMillis % cycleLength) < onLength) ? 1 : 0;
                }
            }
        }

        private class AnimateFade : AnimateGradient
        {
            private readonly long cycleLength;
            private readonly double fadeDepth;

            public AnimateFade(ColorGradient gradient, long cycleLength, double fadeDepth) : base(gradient)
            {
                this.cycleLength = cycleLength;
                this.fadeDepth = fadeDepth;
            }

            protected override double Phase
            {
                get
                {
                    double phase = 2.0 * (double)(CurrentMillis % cycleLength) / (double)(cycleLength);
                    if (phase > 1.0) phase = 2.0 - phase;
                    return (1.0 - fadeDepth) + fadeDepth * phase;
                }
            }
        }
    }
}
