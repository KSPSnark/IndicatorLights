using System;

namespace IndicatorLights
{
    /// <summary>
    /// Helper methods for parsing and combining toggle references.
    /// </summary>
    static class Toggles
    {
        private const string NOT_OPERATOR = "!";

        /// <summary>
        /// Signature for a function that knows how to parse an IToggle from a module and
        /// a ParsedParams. Returns null if it it's not recognized. Throws ArgumentException
        /// if it's recognized, but invalid syntax.
        /// </summary>
        private delegate IToggle TryParseToggle(PartModule module, ParsedParameters parsedParams);

        private static readonly TryParseToggle[] PARSEABLE_TOGGLES =
        {
            LogicalAnd.TryParse,
            LogicalOr.TryParse,
            GreaterThan.TryParse,
            LessThan.TryParse,
            GreaterThanOrEqual.TryParse,
            LessThanOrEqual.TryParse,
            Between.TryParse
        };

        /// <summary>
        /// Try to parse an optional IToggle from the specified text. Returns null if there's a problem.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IToggle TryParse(PartModule module, string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            try
            {
                return Require(module, text);
            }
            catch (ArgumentException e)
            {
                Logging.Warn(
                    "Can't parse a toggle from \"" + text + "\" on " + module.ClassName
                    + " of " + module.part.GetTitle() + ", ignoring: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Parse an IToggle from the specified text. Throws ArgumentException if there's a problem.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IToggle Require(PartModule module, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("must supply a value");
            }
            text = text.Trim();

            // Check for constants.
            if (text == "true") return Constant.TRUE;
            if (text == "false") return Constant.FALSE;

            // Perhaps it's an inverted toggle?
            if (text.StartsWith(NOT_OPERATOR))
            {
                return Inverter.of(Require(module, text.Substring(NOT_OPERATOR.Length)));
            }

            // Maybe it's an identifier for a toggle.
            IToggle found = Identifiers.FindFirst<IToggle>(module.part, text);
            if (found != null) return found;

            // Perhaps it's a parameterized expression?
            ParsedParameters parsedParams = ParsedParameters.TryParse(text);
            if (parsedParams != null)
            {
                for (int i = 0; i < PARSEABLE_TOGGLES.Length; ++i)
                {
                    IToggle parsed = PARSEABLE_TOGGLES[i](module, parsedParams);
                    if (parsed != null) return parsed;
                }
            }

            // Nope, not parseable.
            throw new ArgumentException("Invalid toggle syntax \"" + text + "\"");
        }


        #region Inverter
        /// <summary>
        /// IToggle implementation that returns the logical NOT of another toggle.
        /// </summary>
        private class Inverter : IToggle
        {
            private readonly IToggle source;

            /// <summary>
            /// Gets an inverter of the specified toggle.
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            public static IToggle of(IToggle source)
            {
                return (source is Inverter) ? ((Inverter)source).source : new Inverter(source);
            }

            private Inverter(IToggle source)
            {
                this.source = source;
            }

            public bool ToggleStatus
            {
                get { return !source.ToggleStatus; }
            }
        }
        #endregion


        #region LogicalAnd
        private class LogicalAnd : IToggle
        {
            private const string TYPE_NAME = "and";
            private readonly IToggle[] inputs;

            public bool ToggleStatus
            {
                get
                {
                    for (int i = 0; i < inputs.Length; ++i)
                    {
                        if (!inputs[i].ToggleStatus) return false;
                    }
                    return true;
                }
            }

            /// <summary>
            /// Try to get a "logical AND" toggle from a ParsedParameters. The expected format is:
            ///
            /// and(toggle1, toggle2, ... toggleN)
            /// </summary>
            /// <param name="module"></param>
            /// <param name="parameters"></param>
            /// <returns></returns>
            public static IToggle TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (!TYPE_NAME.Equals(parsedParams.Identifier)) return null;
                parsedParams.RequireCount(module, 1, -1);
                if (parsedParams.Count == 1) return Require(module, parsedParams[0]);
                IToggle[] inputs = new IToggle[parsedParams.Count];
                for (int i = 0; i < inputs.Length; ++i)
                {
                    inputs[i] = Require(module, parsedParams[i]);
                }
                return new LogicalAnd(inputs);
            }

            private LogicalAnd(IToggle[] inputs)
            {
                this.inputs = inputs;
            }
        }
        #endregion


        #region LogicalOr
        private class LogicalOr : IToggle
        {
            private const string TYPE_NAME = "or";
            private readonly IToggle[] inputs;

            public bool ToggleStatus
            {
                get
                {
                    for (int i = 0; i < inputs.Length; ++i)
                    {
                        if (inputs[i].ToggleStatus) return true;
                    }
                    return false;
                }
            }

            /// <summary>
            /// Try to get a "logical AND" toggle from a ParsedParameters. The expected format is:
            ///
            /// and(toggle1, toggle2, ... toggleN)
            /// </summary>
            /// <param name="module"></param>
            /// <param name="parameters"></param>
            /// <returns></returns>
            public static IToggle TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (!TYPE_NAME.Equals(parsedParams.Identifier)) return null;
                parsedParams.RequireCount(module, 1, -1);
                if (parsedParams.Count == 1) return Require(module, parsedParams[0]);
                IToggle[] inputs = new IToggle[parsedParams.Count];
                for (int i = 0; i < inputs.Length; ++i)
                {
                    inputs[i] = Require(module, parsedParams[i]);
                }
                return new LogicalOr(inputs);
            }

            private LogicalOr(IToggle[] inputs)
            {
                this.inputs = inputs;
            }
        }
        #endregion


        #region Constant
        private class Constant : IToggle
        {
            public static readonly Constant TRUE = new Constant(true);
            public static readonly Constant FALSE = new Constant(false);

            private readonly bool status;

            private Constant(bool status)
            {
                this.status = status;
            }

            public bool ToggleStatus
            {
                get { return status; }
            }
        }
        #endregion


        #region IScalar conversions
        /// <summary>
        /// Allows treating an IScalar as a toggle (evaluates as true when it's in a range).
        /// </summary>
        private abstract class FromScalar : IToggle
        {
            private readonly IScalar input;
            private readonly double threshold;

            protected FromScalar(IScalar input, double threshold)
            {
                this.input = input;
                this.threshold = threshold;
            }

            protected abstract bool Evaluate(double value, double threshold);

            public bool ToggleStatus
            {
                get
                {
                    return Evaluate(input.ScalarValue, threshold);
                }
            }
        }

        /// <summary>
        /// True if the input is greater than the threshold.
        /// </summary>
        private class GreaterThan : FromScalar
        {
            private const string TYPE_NAME = "gt";

            public static IToggle TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (parsedParams.Identifier != TYPE_NAME) return null;
                parsedParams.RequireCount(module, 2);
                IScalar input = Scalars.Require(module, parsedParams[0]);
                double threshold = Statics.Parse(module, parsedParams[1]);
                return new GreaterThan(input, threshold);
            }

            private GreaterThan(IScalar input, double threshold) : base(input, threshold) { }

            protected override bool Evaluate(double value, double threshold)
            {
                return value > threshold;
            }
        }

        /// <summary>
        /// True if the input is greater than the threshold.
        /// </summary>
        private class LessThan : FromScalar
        {
            private const string TYPE_NAME = "lt";

            public static IToggle TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (parsedParams.Identifier != TYPE_NAME) return null;
                parsedParams.RequireCount(module, 2);
                IScalar input = Scalars.Require(module, parsedParams[0]);
                double threshold = Statics.Parse(module, parsedParams[1]);
                return new LessThan(input, threshold);
            }

            private LessThan(IScalar input, double threshold) : base(input, threshold) { }

            protected override bool Evaluate(double value, double threshold)
            {
                return value < threshold;
            }
        }

        /// <summary>
        /// True if the input is greater than the threshold.
        /// </summary>
        private class GreaterThanOrEqual : FromScalar
        {
            private const string TYPE_NAME = "ge";

            public static IToggle TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (parsedParams.Identifier != TYPE_NAME) return null;
                parsedParams.RequireCount(module, 2);
                IScalar input = Scalars.Require(module, parsedParams[0]);
                double threshold = Statics.Parse(module, parsedParams[1]);
                return new GreaterThanOrEqual(input, threshold);
            }

            private GreaterThanOrEqual(IScalar input, double threshold) : base(input, threshold) { }

            protected override bool Evaluate(double value, double threshold)
            {
                return value >= threshold;
            }
        }

        /// <summary>
        /// True if the input is greater than the threshold.
        /// </summary>
        private class LessThanOrEqual : FromScalar
        {
            private const string TYPE_NAME = "le";

            public static IToggle TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (parsedParams.Identifier != TYPE_NAME) return null;
                parsedParams.RequireCount(module, 2);
                IScalar input = Scalars.Require(module, parsedParams[0]);
                double threshold = Statics.Parse(module, parsedParams[1]);
                return new LessThanOrEqual(input, threshold);
            }

            private LessThanOrEqual(IScalar input, double threshold) : base(input, threshold) { }

            protected override bool Evaluate(double value, double threshold)
            {
                return value <= threshold;
            }
        }

        /// <summary>
        /// True if the input is greater than the threshold.
        /// </summary>
        private class Between : IToggle
        {
            private const string TYPE_NAME = "between";
            private readonly IScalar input;
            private readonly double minimum;
            private readonly double maximum;

            public bool ToggleStatus
            {
                get
                {
                    double value = input.ScalarValue;
                    return (value >= minimum) && (value <= maximum);
                }
            }

            public static IToggle TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (parsedParams.Identifier != TYPE_NAME) return null;
                parsedParams.RequireCount(module, 3);
                IScalar input = Scalars.Require(module, parsedParams[0]);
                double minimum = Statics.Parse(module, parsedParams[1]);
                double maximum = Statics.Parse(module, parsedParams[2]);
                if (minimum > maximum) return Constant.FALSE;
                return new Between(input, minimum, maximum);
            }

            private Between(IScalar input, double minimum, double maximum)
            {
                this.input = input;
                this.minimum = minimum;
                this.maximum = maximum;
            }
        }
        #endregion
    }
}
