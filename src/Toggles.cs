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
            LogicalOr.TryParse
        };

        /// <summary>
        /// Try to parse an IToggle from the specified text. Throws ArgumentException if there's a problem.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IToggle Parse(PartModule module, string text)
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
                return Inverter.of(Parse(module, text.Substring(NOT_OPERATOR.Length)));
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
                if (parsedParams.Count < 1) throw new ArgumentException("Invalid " + TYPE_NAME + "() syntax: must have at least one argument");
                if (parsedParams.Count == 1) return Parse(module, parsedParams[0]);
                IToggle[] inputs = new IToggle[parsedParams.Count];
                for (int i = 0; i < inputs.Length; ++i)
                {
                    inputs[i] = Parse(module, parsedParams[i]);
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
                if (parsedParams.Count < 1) throw new ArgumentException("Invalid " + TYPE_NAME + "() syntax: must have at least one argument");
                if (parsedParams.Count == 1) return Parse(module, parsedParams[0]);
                IToggle[] inputs = new IToggle[parsedParams.Count];
                for (int i = 0; i < inputs.Length; ++i)
                {
                    inputs[i] = Parse(module, parsedParams[i]);
                }
                return new LogicalOr(inputs);
            }

            private LogicalOr(IToggle[] inputs)
            {
                this.inputs = inputs;
            }
        }
        #endregion

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
    }
}
