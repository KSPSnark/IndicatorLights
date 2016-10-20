using System;
using System.Reflection;

namespace IndicatorLights
{
    /// <summary>
    /// Various utility methods for working with scalar inputs.
    /// </summary>
    static class Scalars
    {
        /// <summary>
        /// Signature for a function that knows how to parse a scalar value from a module and
        /// a ParsedParams. Returns NaN if it it's not recognized. Throws ArgumentOutOfRangeException
        /// if it's recognized, but invalid syntax.
        /// </summary>
        private delegate double TryParseSource(PartModule module, ParsedParameters parsedParams);

        private static readonly TryParseSource[] PARSEABLE_SOURCES = {
            TryParseScalar,
            TryParseAdd,
            TryParseSubtract,
            TryParseMultiply,
            TryParseDivide,
            TryParseMinimum,
            TryParseMaximum,
            TryParseRange,
            TryParseSquareRoot
        };

        /// <summary>
        /// Parse the specified string as a scalar value. It may be a numeric literal, or a
        /// reference to a scalar field on the module, or a parameterized scalar expression.
        /// Throws if there's a problem.
        /// </summary>
        public static double Parse(PartModule module, string text)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentException("No value provided");

            // Is it a parameterized source?
            ParsedParameters parameters = ParsedParameters.TryParse(text);
            if (parameters != null)
            {
                for (int i = 0; i < PARSEABLE_SOURCES.Length; ++i)
                {
                    double value = PARSEABLE_SOURCES[i](module, parameters);
                    if (!double.IsNaN(value)) return value;
                }
                throw new ArgumentException("Unknown scalar function '" + parameters.Identifier + "'");
            }

            // Try to parse it as a literal numeric value.
            try
            {
                return double.Parse(text);
            }
            catch (FormatException e)
            {
                throw new ArgumentOutOfRangeException("'" + text + "' on " + module.ClassName + " of "
                    + module.part.GetTitle() + " can't be parsed as a number: " + e.Message);
            }
        }

        /// <summary>
        /// Parse a scalar expression thus:
        ///
        /// scalar(fieldname)
        /// </summary>
        /// <param name="module"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static double TryParseScalar(PartModule module, ParsedParameters parameters)
        {
            if (parameters.Identifier != "scalar") return double.NaN;
            if (parameters.Count != 1)
            {
                throw new ArgumentException("Wrong number of arguments for '" + parameters.Identifier
                    + "' (was " + parameters.Count + ", must be 1)");
            }
            string fieldName = parameters[0];
            FieldInfo field = module.GetType().GetField(
                fieldName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (object.ReferenceEquals(field, null))
            {
                throw new ArgumentException("Unknown scalar field '" + fieldName + "' on " + module.ClassName
                    + " of " + module.part.GetTitle());
            }
            if (ScalarField.Is(field))
            {
                return (double)field.GetValue(module);
            }
            else
            {
                throw new ArgumentOutOfRangeException("Field '" + fieldName + "' on " + module.ClassName
                    + " of " + module.part.GetTitle() + " is not a scalar field");
            }
        }

        /// <summary>
        /// Parse a scalar expression thus:
        ///
        /// add(arg1, arg2, arg3, ... argN)
        /// </summary>
        /// <param name="module"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static double TryParseAdd(PartModule module, ParsedParameters parameters)
        {
            double[] args = TryParseArguments(module, parameters, 2, false, "add", "sum", "plus");
            if (args == null) return double.NaN;
            double sum = args[0];
            for (int i = 1; i < args.Length; ++i)
            {
                sum += args[i];
            }
            return sum;
        }

        /// <summary>
        /// Parse a scalar expression thus:
        ///
        /// subtract(arg1, arg2)
        /// </summary>
        /// <param name="module"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static double TryParseSubtract(PartModule module, ParsedParameters parameters)
        {
            double[] args = TryParseArguments(module, parameters, 2, true, "subtract", "difference", "diff", "minus");
            if (args == null) return double.NaN;
            return args[0] - args[1];
        }

        /// <summary>
        /// Parse a scalar expression thus:
        ///
        /// multiply(arg1, arg2, arg3, ... argN)
        /// </summary>
        /// <param name="module"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static double TryParseMultiply(PartModule module, ParsedParameters parameters)
        {
            double[] args = TryParseArguments(module, parameters, 2, false, "multiply", "product");
            if (args == null) return double.NaN;
            double product = args[0];
            for (int i = 1; i < args.Length; ++i)
            {
                product *= args[i];
            }
            return product;
        }

        /// <summary>
        /// Parse a scalar expression thus:
        ///
        /// divide(arg1, arg2)
        /// </summary>
        /// <param name="module"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static double TryParseDivide(PartModule module, ParsedParameters parameters)
        {
            double[] args = TryParseArguments(module, parameters, 2, true, "divide", "quotient");
            if (args == null) return double.NaN;
            return args[0] / args[1];
        }

        /// <summary>
        /// Parse a scalar expression thus:
        ///
        /// minimum(arg1, arg2, arg3, ... argN)
        /// </summary>
        /// <param name="module"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static double TryParseMinimum(PartModule module, ParsedParameters parameters)
        {
            double[] args = TryParseArguments(module, parameters, 2, false, "minimum", "min");
            if (args == null) return double.NaN;
            double min = args[0];
            for (int i = 1; i < args.Length; ++i)
            {
                if (args[i] < min) min = args[i];
            }
            return min;
        }

        /// <summary>
        /// Parse a scalar expression thus:
        ///
        /// maximum(arg1, arg2, arg3, ... argN)
        /// </summary>
        /// <param name="module"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static double TryParseMaximum(PartModule module, ParsedParameters parameters)
        {
            double[] args = TryParseArguments(module, parameters, 2, false, "maximum", "max");
            if (args == null) return double.NaN;
            double max = args[0];
            for (int i = 1; i < args.Length; ++i)
            {
                if (args[i] > max) max = args[i];
            }
            return max;
        }

        /// <summary>
        /// Parse a scalar expression thus:
        ///
        /// range(arg, minimum, maximum)
        /// </summary>
        /// <param name="module"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static double TryParseRange(PartModule module, ParsedParameters parameters)
        {
            double[] args = TryParseArguments(module, parameters, 3, true, "range");
            if (args == null) return double.NaN;
            if (args[0] < args[1]) return args[1];
            if (args[0] > args[2]) return args[2];
            return args[0];
        }

        /// <summary>
        /// Parse a scalar expression thus:
        ///
        /// sqrt(arg)
        /// </summary>
        /// <param name="module"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static double TryParseSquareRoot(PartModule module, ParsedParameters parameters)
        {
            double[] args = TryParseArguments(module, parameters, 1, true, "sqrt");
            return (args == null) ? double.NaN : Math.Sqrt(args[0]);
        }

        /// <summary>
        /// Try to parse arguments for the expected function. Returns null if not a match. Throws if it's
        /// a match, but invalid.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="parsedParams"></param>
        /// <param name="identifier"></param>
        /// <param name="numArguments">Required number of arguments.</param>
        /// <param name="exactNumberOfArguments">If true, numArguments must be exact value. If false, numArguments is a minimum.</param>
        /// <returns></returns>
        private static double[] TryParseArguments(
            PartModule module,
            ParsedParameters parsedParams,
            int numArguments,
            bool exactNumberOfArguments,
            params string[] identifiers)
        {
            if (identifiers.IndexOf(parsedParams.Identifier) < 0)
            {
                // not a match!
                return null;
            }

            if (exactNumberOfArguments)
            {
                if (parsedParams.Count != numArguments)
                {
                    throw new ArgumentException("Wrong number of arguments for '" + parsedParams.Identifier
                        + "' (was " + parsedParams.Count + ", must be " + numArguments + ")");
                }
            }
            else
            {
                if (parsedParams.Count < numArguments)
                {
                    throw new ArgumentException("Wrong number of arguments for '" + parsedParams.Identifier
                        + "' (was " + parsedParams.Count + ", must be at least " + numArguments + ")");
                }
            }

            double[] args = new double[parsedParams.Count];
            for (int i = 0; i < parsedParams.Count; ++i)
            {
                args[i] = Parse(module, parsedParams[i]);
            }
            return args;
        }
    }
}
