using System;
using UnityEngine;

namespace IndicatorLights
{
    internal static class ColorSources
    {
        public static readonly IColorSource BLACK = Constant(Color.black);

        public static readonly IColorSource ERROR = new ErrorColorSource();

        // These are used as substitution tags when parsing emissive arrays.
        private const string INDEX_TAG = "index"; // gets substituted by the index within the array
        private const string COUNT_TAG = "count"; // gets substituted by the size of the array

        /// <summary>
        /// Signature for a function that knows how to parse an IColorSource from a module and
        /// a ParsedParams. Returns null if it it's not recognized. Throws ColorSourceException
        /// if it's recognized, but invalid syntax.
        /// </summary>
        private delegate IColorSource TryParseSource(PartModule module, ParsedParameters parsedParams);

        /// <summary>
        /// This is the list of all parseable types that we can handle.
        /// </summary>
        private static readonly TryParseSource[] PARSEABLE_SOURCES =
        {
            BlinkColorSource.TryParse,
            PulsateColorSource.TryParse,
            DimColorSource.TryParse,
            RandomColorSource.TryParse,
            IfColorSource.TryParse,
            LerpColorSource.TryParse
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
        /// Gets a constant-color source.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static IColorSource Constant(DefaultColor color)
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
        /// <param name="phase"></param>
        /// <returns></returns>
        public static IColorSource Blink(IColorSource onSource, long onMillis, IColorSource offSource, long offMillis, float phase = 0F)
        {
            return new BlinkColorSource(onSource, onMillis, offSource, offMillis, phase);
        }


        /// <summary>
        /// Gets a source that applies a pulsating brightness filter to another source.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="cycleMillis"></param>
        /// <param name="multiplier1"></param>
        /// <param name="multiplier2"></param>
        /// <param name="phase"></param>
        /// <returns></returns>
        public static IColorSource Pulsate(IColorSource origin, long cycleMillis, float multiplier1, float multiplier2, float phase = 0F)
        {
            return new PulsateColorSource(origin, cycleMillis, multiplier1, multiplier2, phase);
        }


        /// <summary>
        /// Gets a source that provides a random flickering blink.
        /// </summary>
        /// <param name="onSource"></param>
        /// <param name="offSource"></param>
        /// <param name="periodMillis"></param>
        /// <param name="bias"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static IColorSource Random(IColorSource onSource, IColorSource offSource, long periodMillis, double bias = 0, int seed = 0)
        {
            return new RandomColorSource(onSource, offSource, periodMillis, bias, seed);
        }


        /// <summary>
        /// Gets a source that applies a constant brightness multiplier to another source.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public static IColorSource Dim(IColorSource origin, float multiplier)
        {
            return new DimColorSource(origin, multiplier);
        }


        /// <summary>
        /// Given a color source ID (which might be a literal color, the ID of a controller,
        /// or a parameterized source), try to find the appropriate source. If it can't
        /// be parsed or found, logs an error and returns an "error" source for debugging purposes.
        /// </summary>
        public static IColorSource Find(PartModule module, string sourceID)
        {
            try
            {
                return FindPrivate(module, sourceID);
            }
            catch (ColorSourceException e)
            {
                string message = "Invalid color source '" + sourceID + "' specified for " + module.ClassName + " on " + module.part.GetTitle() + ": " + e.Message;
                for (Exception cause = e.InnerException; cause != null; cause = cause.InnerException)
                {
                    message += " -> " + cause.Message;
                }
                Logging.Warn(message);
                if (Configuration.isVerbose)
                {
                    Logging.Exception(e);
                }
                return ERROR;
            }
        }

        /// <summary>
        /// Given a color source ID (which might be a literal color, the ID of a controller,
        /// or a parameterized source) and a count, try to get an array of that many color
        /// sources, each parsed from the source ID, but substituting the zero-based index for
        /// the string literal "index".
        /// </summary>
        /// <param name="module"></param>
        /// <param name="sourceID"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IColorSource[] FindWithIndex(PartModule module, string sourceID, int count)
        {
            IColorSource[] sources = new IColorSource[count];
            int index = 0;
            try
            {
                for ( ; index < sources.Length; ++index)
                {
                    string withIndex = sourceID.Replace(INDEX_TAG, index.ToString()).Replace(COUNT_TAG, count.ToString());
                    sources[index] = FindPrivate(module, withIndex);
                }
            }
            catch (ColorSourceException e)
            {
                string message = "Invalid color source '" + sourceID + "' specified for " + module.ClassName + " on " + module.part.GetTitle() + ": " + e.Message;
                for (Exception cause = e.InnerException; cause != null; cause = cause.InnerException)
                {
                    message += " -> " + cause.Message;
                }
                Logging.Warn(message);
                if (Configuration.isVerbose)
                {
                    Logging.Exception(e);
                }
                for ( ; index < sources.Length; ++index)
                {
                    sources[index] = ERROR;
                }
            }
            return sources;
        }

        /// <summary>
        /// Given a color source ID (which might be a literal color, the ID of a controller,
        /// or a parameterized source), try to find the appropriate source. If it can't
        /// be parsed or found, throws a ColorSourceException.
        private static IColorSource FindPrivate(PartModule module, string sourceID)
        {
            if (string.IsNullOrEmpty(sourceID))
            {
                throw new ColorSourceException(module, "Null or empty color source");
            }

            // Maybe it's a color string.
            if (Colors.IsColorString(sourceID))
            {
                return Constant(Colors.Parse(sourceID, Color.black));
            }

            // Is it a parameterized source?
            ParsedParameters parsedParams = ParsedParameters.TryParse(sourceID);
            if (parsedParams != null)
            {
                // It has the right syntax for a parameterizeed source. Do we recognize it?
                for (int i = 0; i < PARSEABLE_SOURCES.Length; ++i)
                {
                    IColorSource candidate = PARSEABLE_SOURCES[i](module, parsedParams);
                    if (candidate != null) return candidate;
                }
                throw new ColorSourceException(module, "Unknown function type '" + parsedParams.Identifier + "'");
            }

            // Maybe it's another field on the module?
            for (int i = 0; i < module.Fields.Count; ++i)
            {
                BaseField field = module.Fields[i];
                if (!sourceID.Equals(field.name)) continue;
                if (!ColorSourceIDField.Is(field))
                {
                    throw new ColorSourceException(
                        module, sourceID + " field on " + module.ClassName + " of "
                        + module.part.GetTitle() + " is not a color source ID field");
                }
                return Find(module, field.GetValue<string>(module));
            }

            // Maybe it's a module on the part?
            for (int i = 0; i < module.part.Modules.Count; ++i)
            {
                IColorSource candidate = module.part.Modules[i] as IColorSource;
                if (candidate == null) continue;
                if (sourceID.Equals(candidate.ColorSourceID))
                {
                    return candidate;
                }
            }

            // not found
            throw new ColorSourceException(module, "Can't find a color source named '" + sourceID + "'");
        }


        #region ConstantColorSource
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

            public ConstantColorSource(DefaultColor color)
            {
                this.color = color.Value();
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
        #endregion


        #region DimColorSource
        /// <summary>
        /// A color source that applies a constant dimming filter to another source.
        /// </summary>
        private class DimColorSource : IColorSource
        {
            private static readonly string[] TYPE_NAMES = { "dim", "brightness" };
            private readonly IColorSource origin;
            private readonly float multiplier;
            private readonly string id;

            public DimColorSource(IColorSource origin, float multiplier) : this(TYPE_NAMES[0], origin, multiplier)
            {
            }

            private DimColorSource(string tag, IColorSource origin, float multiplier)
            {
                this.origin = origin;
                this.multiplier = multiplier;
                this.id = string.Format("{0}({1},{2})", tag, origin.ColorSourceID, multiplier);
            }

            /// <summary>
            /// Try to get a dim color source from a ParsedParameters. The expected format is:
            ///
            /// dim(origin, multiplier)
            /// </summary>
            public static IColorSource TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;

                string tag = FindTag(parsedParams.Identifier);
                if (tag == null) return null;
                parsedParams.RequireCount(module, 2);

                IColorSource origin;
                try
                {
                    origin = FindPrivate(module, parsedParams[0]);
                }
                catch (ColorSourceException e)
                {
                    throw new ColorSourceException(module, tag + "() source has invalid origin", e);
                }

                float multiplier;
                try
                {
                    multiplier = (float)Statics.Parse(module, parsedParams[1]);
                }
                catch (ArgumentException e)
                {
                    throw new ColorSourceException(module, tag + "(): Invalid multiplier value '" + parsedParams[1] + "': " + e.Message, e);
                }
                if (multiplier < 0)
                {
                    throw new ColorSourceException(module, tag + "(): Invalid multiplier value '" + parsedParams[1] + "' (can't be negative)");
                }
                if (multiplier == 1) return origin;

                return new DimColorSource(tag, origin, multiplier);
            }

            public string ColorSourceID
            {
                get { return id; }
            }

            public bool HasColor
            {
                get { return origin.HasColor; }
            }

            public Color OutputColor
            {
                get { return origin.OutputColor * multiplier; }
            }

            private static string FindTag(string identifier)
            {
                for (int i = 0; i < TYPE_NAMES.Length; ++i)
                {
                    if (TYPE_NAMES[i].Equals(identifier))
                    {
                        return TYPE_NAMES[i];
                    }
                }
                return null;
            }
        }
        #endregion


        #region BlinkColorSource
        /// <summary>
        /// A color source that blinks between two other sources.
        /// </summary>
        private class BlinkColorSource : IColorSource
        {
            private const string TYPE_NAME = "blink";
            private readonly Animations.Blink blink;
            private readonly IColorSource onSource;
            private readonly IColorSource offSource;
            private readonly string id;

            public BlinkColorSource(IColorSource onSource, long onMillis, IColorSource offSource, long offMillis, float phase)
            {
                this.blink = Animations.Blink.of(onMillis, offMillis, phase);
                this.onSource = onSource;
                this.offSource = offSource;
                if (phase == 0F)
                {
                    this.id = string.Format(
                        "{0}({1},{2},{3},{4})",
                        TYPE_NAME,
                        onSource.ColorSourceID,
                        onMillis,
                        offSource.ColorSourceID,
                        offMillis);
                }
                else
                {
                    this.id = string.Format(
                        "{0}({1},{2},{3},{4},{5})",
                        TYPE_NAME,
                        onSource.ColorSourceID,
                        onMillis,
                        offSource.ColorSourceID,
                        offMillis,
                        phase);
                }
            }

            /// <summary>
            /// Try to get a blink color source from a ParsedParameters. The expected format is:
            /// 
            /// blink(onSource, onMillis, offSource, offMillis)
            /// blink(onSource, onMillis, offSource, offMillis, phase)
            /// </summary>
            public static IColorSource TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (!TYPE_NAME.Equals(parsedParams.Identifier)) return null;
                parsedParams.RequireCount(module, 4, 5);

                IColorSource onSource;
                try
                {
                    onSource = FindPrivate(module, parsedParams[0]);
                }
                catch (ColorSourceException e)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "() source has invalid 'on' parameter", e);
                }
                

                long onMillis;
                try
                {
                    onMillis = (long)Statics.Parse(module, parsedParams[1]);
                }
                catch (ArgumentException e)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "(): Invalid 'on' milliseconds value '" + parsedParams[1] + "': " + e.Message, e);
                }
                if (onMillis < 1)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "(): 'on' milliseconds must be positive");
                }

                IColorSource offSource;
                try
                {
                    offSource = FindPrivate(module, parsedParams[2]);
                }
                catch (ColorSourceException e)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "() source has invalid 'off' parameter", e);
                }

                long offMillis;
                try
                {
                    offMillis = (long)Statics.Parse(module, parsedParams[3]);
                }
                catch (ArgumentException e)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "(): Invalid 'off' milliseconds value '" + parsedParams[3] + "': " + e.Message, e);
                }
                if (offMillis < 1)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "(): 'off' milliseconds must be positive");
                }

                float phase = 0F;
                if (parsedParams.Count > 4)
                {
                    try
                    {
                        phase = (float)Statics.Parse(module, parsedParams[4]);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ColorSourceException(module, TYPE_NAME + "(): Invalid phase value '" + parsedParams[4] + "': " + e.Message, e);
                    }
                }

                return new BlinkColorSource(onSource, onMillis, offSource, offMillis, phase);
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
        #endregion


        #region PulsateColorSource
        /// <summary>
        /// Applies a pulsating brightness filter to another color source.
        /// </summary>
        private class PulsateColorSource : IColorSource
        {
            private const string TYPE_NAME = "pulsate";
            private readonly Animations.TriangleWave wave;
            private readonly IColorSource origin;
            private readonly string id;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="origin">The ColorSource to which to apply a pulsating brightness filter.</param>
            /// <param name="cycleMillis">The duration of a pulsate cycle, in milliseconds.</param>
            /// <param name="multiplier1">The brightness factor to apply at one end of a cycle. 0 = pulsates down to black (strongest effect), 1 = no dimming.</param>
            /// <param name="multiplier2">The brightness factor to apply at the other end of a cycle. 0 = pulsates down to black (strongest effect), 1 = no dimming.</param>
            public PulsateColorSource(IColorSource origin, long cycleMillis, float multiplier1, float multiplier2) : this(origin, cycleMillis, multiplier1, multiplier2, 0)
            {
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="origin">The ColorSource to which to apply a pulsating brightness filter.</param>
            /// <param name="cycleMillis">The duration of a pulsate cycle, in milliseconds.</param>
            /// <param name="multiplier1">The brightness factor to apply at one end of a cycle. 0 = pulsates down to black (strongest effect), 1 = no dimming.</param>
            /// <param name="multiplier2">The brightness factor to apply at the other end of a cycle. 0 = pulsates down to black (strongest effect), 1 = no dimming.</param>
            /// <param name="phase">The phase of the animation.</param>
            public PulsateColorSource(IColorSource origin, long cycleMillis, float multiplier1, float multiplier2, float phase)
            {
                this.origin = origin;
                this.wave = Animations.TriangleWave.of(cycleMillis, multiplier1, multiplier2, phase);
                if (phase == 0)
                {
                    this.id = string.Format(
                        "{0}({1},{2},{3},{4})",
                        TYPE_NAME,
                        origin.ColorSourceID,
                        cycleMillis,
                        multiplier1,
                        multiplier2);
                }
                else
                {
                    this.id = string.Format(
                        "{0}({1},{2},{3},{4},{5})",
                        TYPE_NAME,
                        origin.ColorSourceID,
                        cycleMillis,
                        multiplier1,
                        multiplier2,
                        phase);
                }
            }

            /// <summary>
            /// Try to get a pulsate color source from a ParsedParameters. The expected format is:
            /// 
            /// pulsate(origin, cycleMillis, multiplier)
            /// pulsate(origin, cycleMillis, multiplier1, multiplier2)
            /// pulsate(origin, cycleMillis, multiplier1, multiplier2, phase)
            /// 
            /// ...where multiplier is a float in the range 0 - 1.
            /// </summary>
            public static IColorSource TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (!TYPE_NAME.Equals(parsedParams.Identifier)) return null;
                parsedParams.RequireCount(module, 3, 5);

                IColorSource origin;
                try
                {
                    origin = FindPrivate(module, parsedParams[0]);
                }
                catch (ColorSourceException e)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "() source has invalid origin", e);
                }

                long cycleMillis;
                try
                {
                    cycleMillis = (long)Statics.Parse(module, parsedParams[1]);
                }
                catch (ArgumentException e)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "(): Invalid cycle milliseconds value '" + parsedParams[1] + "': " + e.Message, e);
                }
                if (cycleMillis < 1)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "(): cycle milliseconds must be positive");
                }

                float multiplier1;
                try
                {
                    multiplier1 = (float)Statics.Parse(module, parsedParams[2]);
                }
                catch (ArgumentException e)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "(): Invalid multiplier value '" + parsedParams[2] + "': " + e.Message, e);
                }
                if ((multiplier1 < 0) || (multiplier1 > 1))
                {
                    throw new ColorSourceException(module, TYPE_NAME + "(): Invalid multiplier value '" + parsedParams[2] + "' (must be in range 0 - 1)");
                }

                float multiplier2 = 1f;
                if (parsedParams.Count > 3)
                {
                    try
                    {
                        multiplier2 = (float)Statics.Parse(module, parsedParams[3]);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ColorSourceException(module, TYPE_NAME + "(): Invalid multiplier value '" + parsedParams[3] + "': " + e.Message, e);
                    }
                    if ((multiplier2 < 0) || (multiplier2 > 1))
                    {
                        throw new ColorSourceException(module, TYPE_NAME + "(): Invalid multiplier value '" + parsedParams[3] + "' (must be in range 0 - 1)");
                    }
                }

                float phase = 0;
                if (parsedParams.Count > 4)
                {
                    try
                    {
                        phase = (float)Statics.Parse(module, parsedParams[4]);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ColorSourceException(module, TYPE_NAME + "(): Invalid phase value '" + parsedParams[4] + "': " + e.Message, e);
                    }
                }

                if ((multiplier1 >= 1) && (multiplier2 >= 1)) return origin;
                if ((multiplier1 <= 0) && (multiplier2 <= 0)) return BLACK;

                return new PulsateColorSource(origin, cycleMillis, multiplier1, multiplier2, phase);
            }

            public bool HasColor
            {
                get { return origin.HasColor; }
            }

            public Color OutputColor
            {
                get { return wave.Value * origin.OutputColor; }
            }

            public string ColorSourceID
            {
                get { return id; }
            }
        }
        #endregion


        #region RandomColorSource
        /// <summary>
        /// A color source that irregularly blinks between two other sources.
        /// </summary>
        private class RandomColorSource : IColorSource
        {
            private const string TYPE_NAME = "random";
            private readonly Animations.RandomBlink blink;
            private readonly IColorSource onSource;
            private readonly IColorSource offSource;
            private readonly string id;

            public RandomColorSource(IColorSource onSource, IColorSource offSource, long periodMillis, double bias, int seed)
            {
                this.blink = Animations.RandomBlink.of(periodMillis, bias, seed);
                this.onSource = onSource;
                this.offSource = offSource;
                if (seed == 0)
                {
                    if (bias == 0.0)
                    {
                        this.id = string.Format(
                            "{0}({1},{2},{3})",
                            TYPE_NAME,
                            onSource.ColorSourceID,
                            offSource.ColorSourceID,
                            periodMillis);
                    }
                    else
                    {
                        this.id = string.Format(
                            "{0}({1},{2},{3},{4})",
                            TYPE_NAME,
                            onSource.ColorSourceID,
                            offSource.ColorSourceID,
                            periodMillis,
                            bias);
                    }
                }
                else
                {
                    this.id = string.Format(
                        "{0}({1},{2},{3},{4},{5})",
                        TYPE_NAME,
                        onSource.ColorSourceID,
                        offSource.ColorSourceID,
                        periodMillis,
                        bias,
                        seed);
                }
            }

            /// <summary>
            /// Try to get a random color source from a ParsedParameters. The expected format is:
            ///
            /// random(onSource, offSource, periodMillis, bias, seed)
            /// random(onSource, offSource, periodMillis, bias)
            /// random(onSource, offSource, periodMillis)
            /// </summary>
            public static IColorSource TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (!TYPE_NAME.Equals(parsedParams.Identifier)) return null;
                parsedParams.RequireCount(module, 3, 5);

                IColorSource onSource;
                try
                {
                    onSource = FindPrivate(module, parsedParams[0]);
                }
                catch (ColorSourceException e)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "() source has invalid 'on' parameter", e);
                }


                IColorSource offSource;
                try
                {
                    offSource = FindPrivate(module, parsedParams[1]);
                }
                catch (ColorSourceException e)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "() source has invalid 'off' parameter", e);
                }

                long periodMillis;
                try
                {
                    periodMillis = (long)Statics.Parse(module, parsedParams[2]);
                }
                catch (ArgumentException e)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "(): Invalid 'period' milliseconds value '" + parsedParams[2] + "': " + e.Message, e);
                }
                if (periodMillis < 1)
                {
                    throw new ColorSourceException(module, TYPE_NAME + "(): 'period' milliseconds must be positive");
                }

                double bias = 0;
                if (parsedParams.Count > 3)
                {
                    try
                    {
                        bias = Statics.Parse(module, parsedParams[3]);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ColorSourceException(module, TYPE_NAME + "(): Invalid bias value '" + parsedParams[3] + "': " + e.Message, e);
                    }
                    if ((bias < -1) || (bias > 1))
                    {
                        throw new ColorSourceException(module, TYPE_NAME + "(): Invalid bias value '" + parsedParams[3] + "' (must be between -1 and 1)");
                    }
                }

                int seed = 0;
                if (parsedParams.Count > 4)
                {
                    try
                    {
                        seed = (int)Statics.Parse(module, parsedParams[4]);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ColorSourceException(module, TYPE_NAME + "(): Invalid seed value '" + parsedParams[4] + "': " + e.Message, e);
                    }
                }
                if (module.vessel != null) seed ^= module.vessel.id.GetHashCode();
                if (module.part != null)
                {
                    seed ^= module.part.flightID.GetHashCode();
                    seed ^= IndexOf(module).GetHashCode();
                }

                return new RandomColorSource(onSource, offSource, periodMillis, bias, seed);
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

            private static int IndexOf(PartModule module)
            {
                for (int i = 0; i < module.part.Modules.Count; ++i)
                {
                    if (object.ReferenceEquals(module.part.Modules[i], module)) return i;
                }
                return -1; // should never happen
            }
        }
        #endregion


        #region IfColorSource
        private class IfColorSource : IColorSource
        {
            private const string TYPE_NAME = "if";
            private readonly IToggle toggle;
            private readonly IColorSource onSource;
            private readonly IColorSource offSource;
            private readonly string id;

            public IfColorSource(IToggle toggle, string toggleID, IColorSource onSource, IColorSource offSource)
            {
                this.toggle = toggle;
                this.onSource = onSource;
                this.offSource = offSource;

                if ((offSource.ColorSourceID == Colors.ToString(DefaultColor.Off))
                    || (offSource.ColorSourceID == Colors.ToString(Color.black)))
                {
                    id = string.Format(
                        "{0}({1},{2})",
                        TYPE_NAME,
                        toggleID,
                        onSource.ColorSourceID);
                }
                else
                {
                    id = string.Format(
                        "{0}({1},{2},{3})",
                        TYPE_NAME,
                        toggleID,
                        onSource.ColorSourceID,
                        offSource.ColorSourceID);
                }
            }

            /// <summary>
            /// Try to get an "if" color source from a ParsedParameters. The expected format is:
            ///
            /// if(toggle, onSource)
            /// if(toggle, onSource, offSource)
            /// </summary>
            public static IColorSource TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (!TYPE_NAME.Equals(parsedParams.Identifier)) return null;
                parsedParams.RequireCount(module, 2, 3);
                IToggle toggle = null;
                try
                {
                    toggle = Toggles.Require(module, parsedParams[0]);
                }
                catch (ArgumentException e)
                {
                    throw new ColorSourceException(module, "Invalid toggle specified for " + TYPE_NAME + "(): " + e.Message);
                }
                IColorSource onSource = Find(module, parsedParams[1]);
                IColorSource offSource = (parsedParams.Count > 2) ? Find(module, parsedParams[2]) : BLACK;
                return new IfColorSource(toggle, parsedParams[0], onSource, offSource);
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

            private IColorSource CurrentSource
            {
                get { return toggle.ToggleStatus ? onSource : offSource; }
            }
        }
        #endregion


        #region LerpColorSource
        /// <summary>
        /// Color source that does linear interpolation between two other color sources, depending on the
        /// value of a scalar input.
        /// </summary>
        private class LerpColorSource : IColorSource
        {
            private const string TYPE_NAME = "lerp";
            private readonly IScalar input;
            private readonly IColorSource minColor;
            private readonly double minValue;
            private readonly IColorSource maxColor;
            private readonly double maxValue;
            private readonly string id;

            /// <summary>
            /// Try to get a lerp color source from a ParsedParameters. The expected format is:
            ///
            /// lerp(input, source1, source2)
            /// lerp(input, source1, value1, source2, value2)
            /// </summary>
            public static IColorSource TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (parsedParams.Identifier != TYPE_NAME) return null;
                if ((parsedParams.Count != 3) && (parsedParams.Count != 5))
                {
                    throw new ColorSourceException(
                        module,
                        TYPE_NAME + "() source specified " + parsedParams.Count + " parameters (3 or 5 required)");
                }
                IScalar input = Scalars.Require(module, parsedParams[0]);
                if (parsedParams.Count == 3)
                {
                    return new LerpColorSource(
                        input,
                        Find(module, parsedParams[1]),
                        0,
                        Find(module, parsedParams[2]),
                        1,
                        parsedParams.ToString());
                }
                else
                {
                    return new LerpColorSource(
                        input,
                        Find(module, parsedParams[1]),
                        Statics.Parse(module, parsedParams[2]),
                        Find(module, parsedParams[3]),
                        Statics.Parse(module, parsedParams[4]),
                        parsedParams.ToString());
                }
            }

            public bool HasColor
            {
                get { return minColor.HasColor && maxColor.HasColor; }
            }

            public Color OutputColor
            {
                get
                {
                    double value = input.ScalarValue;
                    if (value <= minValue) return minColor.OutputColor;
                    if (value >= maxValue) return maxColor.OutputColor;
                    double fraction = (value - minValue) / (maxValue - minValue);
                    return Color.Lerp(minColor.OutputColor, maxColor.OutputColor, (float)fraction);
                }
            }

            public string ColorSourceID
            {
                get { return id; }
            }

            private LerpColorSource(
                IScalar input,
                IColorSource minColor,
                double minValue,
                IColorSource maxColor,
                double maxValue,
                string id)
            {
                this.input = input;
                this.minColor = minColor;
                this.minValue = minValue;
                this.maxColor = maxColor;
                this.maxValue = maxValue;
                this.id = id;
            }
        }
        #endregion LerpColorSource


        #region ErrorColorSource
        /// <summary>
        /// A repeating "red-green-blue-pause, red-green-blue-pause" flasher, used to
        /// indicate error conditions to assist mod authors in debugging config problems.
        /// </summary>
        private class ErrorColorSource : IColorSource
        {
            public string ColorSourceID
            {
                get { return "ERROR"; }
            }

            public bool HasColor
            {
                get { return true; }
            }

            public Color OutputColor
            {
                get
                {
                    long phase = (Animations.CurrentMillis % 1200L) / 100;
                    switch (phase)
                    {
                        case 0:
                        case 1:
                            return Color.red;
                        case 3:
                        case 4:
                            return Color.green;
                        case 6:
                        case 7:
                            return Color.blue;
                        default:
                            return Color.black;
                    }
                }
            }
        }
        #endregion


        /// <summary>
        /// Thrown when there's an error trying to parse a color source string.
        /// </summary>
        private class ColorSourceException : Exception
        {
            private readonly PartModule module;

            public ColorSourceException(PartModule module, string message) : base(message)
            {
                this.module = module;
            }

            public ColorSourceException(PartModule module, string message, Exception cause) : base(message, cause)
            {
                this.module = module;
            }

            public PartModule Module
            {
                get { return module; }
            }
        }
    }
}
