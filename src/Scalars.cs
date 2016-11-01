using System;

namespace IndicatorLights
{
    /// <summary>
    /// Various utility methods for working with IScalar inputs.
    /// </summary>
    internal static class Scalars
    {
        private const string INVERT_OPERATOR = "-";

        /// <summary>
        /// Signature for a function that knows how to parse an IScalar from a module and
        /// a ParsedParams. Returns null if it it's not recognized. Throws ArgumentException
        /// if it's recognized, but invalid syntax.
        /// </summary>
        private delegate IScalar TryParseScalar(PartModule module, ParsedParameters parsedParams);

        private static readonly TryParseScalar[] PARSEABLE_SCALARS =
        {
            LinearTransform.TryParse,
            Range.TryParse,
            Maximum.TryParse,
            Minimum.TryParse,
            Average.TryParse,
            ToggleAsScalar.TryParse
        };

        /// <summary>
        /// Try to parse an IScalar from the specified text. Throws ArgumentException if there's a problem.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IScalar Parse(PartModule module, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("must supply a value");
            }
            text = text.Trim();

            // Perhaps it's an inverted scalar?
            if (text.StartsWith(INVERT_OPERATOR))
            {
                return LinearTransform.Invert(Parse(module, text.Substring(INVERT_OPERATOR.Length)));
            }

            // Maybe it's an identifier for a scalar.
            IScalar found = Identifiers.FindFirst<IScalar>(module.part, text);
            if (found != null) return found;

            // Perhaps it's a parameterized expression?
            ParsedParameters parsedParams = ParsedParameters.TryParse(text);
            if (parsedParams != null)
            {
                for (int i = 0; i < PARSEABLE_SCALARS.Length; ++i)
                {
                    IScalar parsed = PARSEABLE_SCALARS[i](module, parsedParams);
                    if (parsed != null) return parsed;
                }
            }

            // Last chance:  it could be a static value.
            return Constant.Of(Statics.Parse(module, text));
        }


        #region Constant
        /// <summary>
        /// IScalar implementation that returns a constant value.
        /// </summary>
        private class Constant : IScalar
        {
            private readonly double value;

            public static IScalar Of(double value)
            {
                return new Constant(value);
            }

            private Constant(double value)
            {
                this.value = value;
            }

            public double ScalarValue
            {
                get { return value; }
            }
        }
        #endregion


        #region LinearTransform
        /// <summary>
        /// IScalar implementation that takes a linear transform of another IScalar.
        /// </summary>
        private class LinearTransform : IScalar
        {
            private const string SCALE = "scale";
            private const string OFFSET = "offset";
            private readonly IScalar input;
            private readonly double scale;
            private readonly double offset;

            public static IScalar TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                switch (parsedParams.Identifier)
                {
                    case SCALE:
                        {
                            parsedParams.RequireCount(module, 2, 3);
                            IScalar input = Scalars.Parse(module, parsedParams[0]);
                            double scale = Statics.Parse(module, parsedParams[1]);
                            double offset = (parsedParams.Count > 2) ? Statics.Parse(module, parsedParams[2]) : 0.0;
                            return Of(input, scale, offset);
                        }
                    case OFFSET:
                        {
                            parsedParams.RequireCount(module, 2);
                            IScalar input = Scalars.Parse(module, parsedParams[0]);
                            double offset = Statics.Parse(module, parsedParams[1]);
                            return Offset(input, offset);
                        }
                    default:
                        return null;
                }
            }

            /// <summary>
            /// Get a linear transform of another scalar.
            /// </summary>
            /// <param name="input"></param>
            /// <param name="scale"></param>
            /// <param name="offset"></param>
            /// <returns></returns>
            public static IScalar Of(IScalar input, double scale, double offset)
            {
                if ((scale == 1.0) && (offset == 0.0)) return input;

                if (scale == 0.0) return Constant.Of(offset);

                LinearTransform xform = input as LinearTransform;
                if (xform != null) return new LinearTransform(input, scale * xform.scale, (scale * xform.offset) + offset);

                Constant constant = input as Constant;
                if (constant != null) return Constant.Of(scale * constant.ScalarValue + offset);

                ToggleAsScalar toggle = input as ToggleAsScalar;
                if (toggle != null) return ToggleAsScalar.Of(
                    toggle.Input,
                    toggle.Active * scale + offset,
                    toggle.Inactive * scale + offset);

                return new LinearTransform(input, scale, offset);
            }

            /// <summary>
            /// Get a linear transform that inverts the input.
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static IScalar Invert(IScalar input)
            {
                return Of(input, -1, 0);
            }

            /// <summary>
            /// Get a linear transform that scales the input.
            /// </summary>
            /// <param name="input"></param>
            /// <param name="scale"></param>
            /// <returns></returns>
            public static IScalar Scale(IScalar input, double scale)
            {
                return Of(input, scale, 0);
            }

            /// <summary>
            /// Get a linear transform that offsets the input.
            /// </summary>
            /// <param name="input"></param>
            /// <param name="offset"></param>
            /// <returns></returns>
            public static IScalar Offset(IScalar input, double offset)
            {
                return Of(input, 1.0, offset);
            }

            private LinearTransform(IScalar input, double scale, double offset)
            {
                this.input = input;
                this.scale = scale;
                this.offset = offset;
            }

            public double ScalarValue
            {
                get { return (input.ScalarValue * scale) + offset; }
            }
        }
        #endregion


        #region Range
        /// <summary>
        /// IScalar implementation that constrains another IScalar to a range.
        /// </summary>
        private class Range : IScalar
        {
            private const string RANGE = "range";
            private const string GREATER_THAN = "gt";
            private const string LESS_THAN = "lt";
            private readonly IScalar input;
            private readonly double minimum;
            private readonly double maximum;

            public static IScalar TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                switch (parsedParams.Identifier)
                {
                    case RANGE:
                        {
                            parsedParams.RequireCount(module, 3);
                            IScalar input = Scalars.Parse(module, parsedParams[0]);
                            double minimum = Statics.Parse(module, parsedParams[1]);
                            double maximum = Statics.Parse(module, parsedParams[2]);
                            return Between(input, minimum, maximum);
                        }
                    case GREATER_THAN:
                        {
                            parsedParams.RequireCount(module, 2);
                            IScalar input = Scalars.Parse(module, parsedParams[0]);
                            double minimum = Statics.Parse(module, parsedParams[1]);
                            return AtLeast(input, minimum);
                        }
                    case LESS_THAN:
                        {
                            parsedParams.RequireCount(module, 2);
                            IScalar input = Scalars.Parse(module, parsedParams[0]);
                            double maximum = Statics.Parse(module, parsedParams[1]);
                            return AtMost(input, maximum);
                        }
                    default:
                        return null;
                }
            }

            /// <summary>
            /// Get a range-constrained value.
            /// </summary>
            /// <param name="input"></param>
            /// <param name="minimum"></param>
            /// <param name="maximum"></param>
            /// <returns></returns>
            public static IScalar Between(IScalar input, double minimum, double maximum)
            {
                if (minimum == maximum) return Constant.Of(minimum);
                if (double.IsNegativeInfinity(minimum) && double.IsPositiveInfinity(maximum)) return input;
                Range other = input as Range;
                if (other == null)
                {
                    Constant constant = input as Constant;
                    if (constant == null)
                    {
                        return new Range(input, minimum, maximum);
                    }
                    else
                    {
                        return Constant.Of(Math.Min(maximum, Math.Max(minimum, constant.ScalarValue)));
                    }
                }
                else
                {
                    if (minimum >= other.maximum) return Constant.Of(minimum);
                    if (maximum <= other.minimum) return Constant.Of(maximum);

                    double combinedMinimum = Math.Max(minimum, other.minimum);
                    double combinedMaximum = Math.Min(maximum, other.maximum);
                    return new Range(other.input, combinedMinimum, combinedMaximum);
                }
            }


            /// <summary>
            /// Get a scalar that's constrained to be at least the specified minimum value.
            /// </summary>
            /// <param name="input"></param>
            /// <param name="minimum"></param>
            /// <returns></returns>
            public static IScalar AtLeast(IScalar input, double minimum)
            {
                return Between(input, minimum, double.PositiveInfinity);
            }


            /// <summary>
            /// Get a scalar that's constrained to be at least the specified maximum value.
            /// </summary>
            /// <param name="input"></param>
            /// <param name="maximum"></param>
            /// <returns></returns>
            public static IScalar AtMost(IScalar input, double maximum)
            {
                return Between(input, double.NegativeInfinity, maximum);
            }


            private Range(IScalar input, double minimum, double maximum)
            {
                if (minimum > maximum)
                {
                    throw new ArgumentException("minimum can't exceed maximum");
                }
                this.input = input;
                this.minimum = minimum;
                this.maximum = maximum;
            }

            public double ScalarValue
            {
                get
                {
                    return Math.Min(maximum, Math.Max(minimum, input.ScalarValue));
                }
            }
        }
        #endregion


        #region Maximum
        /// <summary>
        /// IScalar implementation that returns the maximum of multiple inputs. 
        /// </summary>
        private class Maximum : IScalar
        {
            private static readonly string[] TYPE_NAMES = { "max", "maximum" };
            private readonly IScalar[] inputs;

            public static IScalar TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (TYPE_NAMES.IndexOf(parsedParams.Identifier) < 0) return null;
                parsedParams.RequireCount(module, 2, -1);
                IScalar[] inputs = new IScalar[parsedParams.Count];
                for (int i = 0; i < inputs.Length; ++i)
                {
                    inputs[i] = Scalars.Parse(module, parsedParams[i]);
                }
                return Of(inputs);
            }

            public static IScalar Of(IScalar[] inputs)
            {
                if (inputs.Length == 1) return inputs[0];
                return new Maximum(inputs);
            }

            private Maximum(IScalar[] inputs)
            {
                this.inputs = inputs;
            }

            public double ScalarValue
            {
                get
                {
                    double result = inputs[0].ScalarValue;
                    for (int i = 1; i < inputs.Length; ++i)
                    {
                        double value = inputs[i].ScalarValue;
                        if (value > result) result = value;
                    }
                    return result;
                }
            }
        }
        #endregion


        #region Minimum
        /// <summary>
        /// IScalar implementation that returns the minimum of multiple inputs. 
        /// </summary>
        private class Minimum : IScalar
        {
            private static readonly string[] TYPE_NAMES = { "min", "minimum" };
            private readonly IScalar[] inputs;

            public static IScalar TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (TYPE_NAMES.IndexOf(parsedParams.Identifier) < 0) return null;
                parsedParams.RequireCount(module, 2, -1);
                IScalar[] inputs = new IScalar[parsedParams.Count];
                for (int i = 0; i < inputs.Length; ++i)
                {
                    inputs[i] = Scalars.Parse(module, parsedParams[i]);
                }
                return Of(inputs);
            }

            public static IScalar Of(IScalar[] inputs)
            {
                if (inputs.Length == 1) return inputs[0];
                return new Minimum(inputs);
            }

            private Minimum(IScalar[] inputs)
            {
                this.inputs = inputs;
            }

            public double ScalarValue
            {
                get
                {
                    double result = inputs[0].ScalarValue;
                    for (int i = 1; i < inputs.Length; ++i)
                    {
                        double value = inputs[i].ScalarValue;
                        if (value < result) result = value;
                    }
                    return result;
                }
            }
        }
        #endregion


        #region Average
        /// <summary>
        /// IScalar implementation that returns the average of multiple inputs. 
        /// </summary>
        private class Average : IScalar
        {
            private const string TYPE_NAME = "average";
            private readonly IScalar[] inputs;

            public static IScalar TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (parsedParams.Identifier != TYPE_NAME) return null;
                parsedParams.RequireCount(module, 2, -1);
                IScalar[] inputs = new IScalar[parsedParams.Count];
                for (int i = 0; i < inputs.Length; ++i)
                {
                    inputs[i] = Scalars.Parse(module, parsedParams[i]);
                }
                return Of(inputs);
            }

            public static IScalar Of(IScalar[] inputs)
            {
                if (inputs.Length == 1) return inputs[0];
                return new Average(inputs);
            }

            private Average(IScalar[] inputs)
            {
                this.inputs = inputs;
            }

            public double ScalarValue
            {
                get
                {
                    double sum = inputs[0].ScalarValue;
                    for (int i = 1; i < inputs.Length; ++i)
                    {
                        sum += inputs[i].ScalarValue;
                    }
                    return sum / (double)inputs.Length;
                }
            }
        }
        #endregion


        #region ToggleAsScalar
        /// <summary>
        /// Allows treating an IToggle input as a square-wave IScalar.
        /// </summary>
        private class ToggleAsScalar : IScalar
        {
            private const string TYPE_NAME = "scalar";
            private readonly IToggle input;
            private readonly double activeValue;
            private readonly double inactiveValue;

            public static IScalar TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (parsedParams.Identifier != TYPE_NAME) return null;
                parsedParams.RequireCount(module, 1);
                return Of(Toggles.Parse(module, parsedParams[0]), 1, 0);
            }

            public static IScalar Of(IToggle input, double activeValue, double inactiveValue)
            {
                return new ToggleAsScalar(input, activeValue, inactiveValue);
            }

            public IToggle Input
            {
                get { return input; }
            }

            public double Active
            {
                get { return activeValue; }
            }

            public double Inactive
            {
                get { return inactiveValue; }
            }

            private ToggleAsScalar(IToggle input, double activeValue, double inactiveValue)
            {
                this.input = input;
                this.activeValue = activeValue;
                this.inactiveValue = inactiveValue;
            }

            public double ScalarValue
            {
                get { return input.ToggleStatus ? activeValue : inactiveValue; }
            }
        }
        #endregion
    }
}
