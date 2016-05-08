using System;
using UnityEngine;

namespace IndicatorLights
{
    internal static class ColorSources
    {
        public static readonly IColorSource BLACK = Constant(Color.black);

        /// <summary>
        /// Signature for a function that knows how to parse an IColorSource from a part and
        /// a ParsedParams. Returns null if it can't parse.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="parsedParams"></param>
        /// <returns></returns>
        private delegate IColorSource TryParseSource(Part part, ParsedParameters parsedParams);

        /// <summary>
        /// This is the list of all parseable types that we can handle.
        /// </summary>
        private static readonly TryParseSource[] PARSEABLE_SOURCES =
        {
            BlinkColorSource.TryParse
        };

        /// <summary>
        /// Gets a constant-color source.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static IColorSource Constant(Color color)
        {
            return new ConstantColorSource(color);
        }

        /// <summary>
        /// Gets a source that "blinks" between the outputs of two other sources.
        /// </summary>
        /// <param name="onSource"></param>
        /// <param name="onMillis"></param>
        /// <param name="offSource"></param>
        /// <param name="offMillis"></param>
        /// <returns></returns>
        public static IColorSource Blink(IColorSource onSource, long onMillis, IColorSource offSource, long offMillis)
        {
            return new BlinkColorSource(onSource, onMillis, offSource, offMillis);
        }


        /// <summary>
        /// Given a color source ID (which might be a literal color, or might be the ID
        /// of a controller), try to find the appropriate source. If there's no match,
        /// returns a constant black source.
        /// </summary>
        /// <param name="sourceID"></param>
        /// <returns></returns>
        public static IColorSource Find(Part part, string sourceID)
        {
            return TryFind(part, sourceID) ?? BLACK;
        }

        /// <summary>
        /// Given a color source ID (which might be a literal color, or might be the ID
        /// of a controller), try to find the appropriate source. If there's no match,
        /// returns null.
        /// </summary>
        /// <param name="sourceID"></param>
        /// <returns></returns>
        public static IColorSource TryFind(Part part, string sourceID)
        {
            if (!string.IsNullOrEmpty(sourceID))
            {
                // Maybe it's a color string.
                if (Colors.IsColorString(sourceID))
                {
                    return Constant(Colors.Parse(sourceID, Color.black));
                }

                // Is it a parameterized source?
                ParsedParameters parsedParams = ParsedParameters.TryParse(sourceID);
                if (parsedParams != null)
                {
                    for (int i = 0; i < PARSEABLE_SOURCES.Length; ++i)
                    {
                        IColorSource candidate = PARSEABLE_SOURCES[i](part, parsedParams);
                        if (candidate != null) return candidate;
                    }
                    return null; // no match
                }

                // Maybe it's a module on the part?
                for (int i = 0; i < part.Modules.Count; ++i)
                {
                    IColorSource candidate = part.Modules[i] as IColorSource;
                    if (candidate == null) continue;
                    if (sourceID.Equals(candidate.ColorSourceID))
                    {
                        return candidate;
                    }
                }
            }

            // not found
            Logging.Warn("Can't find a color source named '" + sourceID + "' on " + part.GetTitle());
            return null;
        }

        /// <summary>
        /// Puts an IColorSource wrapper around a constant color.
        /// </summary>
        private class ConstantColorSource : IColorSource
        {
            private readonly Color color;
            private readonly string id;

            public ConstantColorSource(Color color)
            {
                this.color = color;
                this.id = Colors.ToString(color);
            }

            public bool HasColor
            {
                get { return true; }
            }

            public Color OutputColor
            {
                get { return color; }
            }

            public string ColorSourceID
            {
                get { return id; }
            }

        }

        /// <summary>
        /// A color source that blinks between two other sources.
        /// </summary>
        private class BlinkColorSource : IColorSource
        {
            private static readonly string TYPE_NAME = "blink";
            private readonly Animations.Blink blink;
            private readonly IColorSource onSource;
            private readonly IColorSource offSource;
            private readonly string id;

            public BlinkColorSource(IColorSource onSource, long onMillis, IColorSource offSource, long offMillis)
            {
                this.blink = Animations.Blink.of(onMillis, offMillis);
                this.onSource = onSource;
                this.offSource = offSource;
                this.id = string.Format(
                    "{0}({1},{2},{3},{4})",
                    TYPE_NAME,
                    onSource.ColorSourceID,
                    onMillis,
                    offSource.ColorSourceID,
                    offMillis);
            }

            /// <summary>
            /// Try to get a blink color source from a ParsedParameters. The expected format is:
            /// 
            /// blink(onSource, onMillis, offSource, offMillis)
            /// </summary>
            /// <param name="part"></param>
            /// <param name="parsedParams"></param>
            /// <returns></returns>
            public static BlinkColorSource TryParse(Part part, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                for (int i = 0; i < parsedParams.Count; ++i)
                {
                }
                if (parsedParams.Count != 4) return null;
                if (!TYPE_NAME.Equals(parsedParams.Identifier)) return null;

                IColorSource onSource = TryFind(part, parsedParams[0]);
                if (onSource == null) return null;

                long onMillis;
                try
                {
                    onMillis = long.Parse(parsedParams[1]);
                }
                catch (FormatException e)
                {
                    Logging.Warn("Invalid 'on' milliseconds value '" + parsedParams[1] + "': " + e.Message);
                    return null;
                }

                IColorSource offSource = TryFind(part, parsedParams[2]);
                if (offSource == null) return null;

                long offMillis;
                try
                {
                    offMillis = long.Parse(parsedParams[3]);
                }
                catch (FormatException e)
                {
                    Logging.Warn("Invalid 'off' milliseconds value '" + parsedParams[3] + "': " + e.Message);
                    return null;
                }

                return new BlinkColorSource(onSource, onMillis, offSource, offMillis);
            }

            public string ColorSourceID
            {
                get { return id; }
            }

            public bool HasColor
            {
                get { return CurrentSource.HasColor; }
            }

            public Color OutputColor
            {
                get { return CurrentSource.OutputColor; }
            }

            /// <summary>
            /// Gets whether the blinker is in the "on" (true) or "off" (false) state.
            /// </summary>
            private IColorSource CurrentSource
            {
                get
                {
                    return blink.State ? onSource : offSource;
                }
            }
        }
    }
}
