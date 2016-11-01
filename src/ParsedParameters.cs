using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace IndicatorLights
{
    /// <summary>
    /// Utility class for parsing strings of the form "identifier(param1, param2, param3)", where
    /// the params themselves may contain nested parentheses & commas.
    /// </summary>
    public class ParsedParameters
    {
        private static readonly Regex PATTERN = new Regex("^([A-Za-z]+)\\((.*)\\)$");
        private readonly string identifier;
        private readonly string[] parameters;

        private ParsedParameters(string identifier, string[] parameters)
        {
            this.identifier = identifier;
            this.parameters = parameters;
        }

        /// <summary>
        /// The primary identifier of the parsed parameters. For functional expressions of
        /// the form "foo(arg1, arg2, ...)", this is the "foo".
        /// </summary>
        public string Identifier
        {
            get { return identifier; }
        }

        /// <summary>
        /// Gets the number of parameters (zero or more).
        /// </summary>
        public int Count
        {
            get { return parameters.Length; }
        }

        /// <summary>
        /// Gets the indexth parameter.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[int index]
        {
            get { return parameters[index]; }
        }

        /// <summary>
        /// Helper method that evaluates the number of parameters and throws an ArgumentException
        /// if the count is wrong.
        /// </summary>
        /// <param name="module">The module where this occurs (used for composing error message)</param>
        /// <param name="minimum">Minimum allowed number of parameters.  -1 if no minimum.</param>
        /// <param name="maximum">Maximum allowed number of parameters. -1 if no maximum.</param>
        public void RequireCount(PartModule module, int minimum, int maximum)
        {
            if ((minimum < 0) && (maximum < 0)) return;
            string correct;
            if (minimum < 0)
            {
                if (maximum < 0) return;
                correct = "at most " + maximum;
            }
            else
            {
                correct = (maximum < 0)
                    ? ("at least " + minimum)
                    : ((minimum == maximum) ? minimum.ToString() : (minimum.ToString() + "-" + maximum));
            }
            if ((minimum >= 0) && (parameters.Length < minimum))
            {
                throw new ArgumentException("Not enough parameters for " + identifier + "() on "
                    + module.ClassName + " of " + module.part.GetTitle() + " (must be " + correct + ")");
            }
            if ((maximum >= 0) && (parameters.Length > maximum))
            {
                throw new ArgumentException("Too many parameters for " + identifier + "() on "
                    + module.ClassName + " of " + module.part.GetTitle() + " (must be " + correct + ")");
            }
        }

        /// <summary>
        /// Helper method that evaluates the number of parameters and throws an ArgumentException
        /// if the count is wrong.
        /// </summary>
        /// <param name="module">The module where this occurs (used for composing error message)</param>
        /// <param name="count">Required number of parameters.</param>
        public void RequireCount(PartModule module, int count)
        {
            RequireCount(module, count, count);
        }

        public override string ToString()
        {
            if (parameters.Length < 1) return Identifier + "()";
            StringBuilder builder = new StringBuilder(Identifier)
                .Append("(")
                .Append(parameters[0]);
            for (int i = 1; i < parameters.Length; ++i)
            {
                builder.Append(",").Append(parameters[i]);
            }
            return builder.Append(")").ToString();
        }

        /// <summary>
        /// Attempt to extract a ParsedParameters object from the specified string. Returns
        /// null if it's not a valid format to be extracted.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static ParsedParameters TryParse(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            text = text.Replace(" ", string.Empty);
            Match match = PATTERN.Match(text);
            if (!match.Success) return null;
            string identifier = match.Groups[1].Value;
            string[] parameters = TryParseParameters(match.Groups[2].Value);
            if (parameters == null) return null;
            return new ParsedParameters(identifier, parameters);
        }

        /// <summary>
        /// Split the input text into a set of zero or more tokens, delimited by commas. The
        /// returned tokens will have any leading/trailing whitespace removed.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string[] Tokenize(string text, char delimiter)
        {
            text = text.Trim();
            if (string.IsNullOrEmpty(text)) return new string[0];
            string[] tokens = text.Split(delimiter);
            for (int i = 0; i < tokens.Length; ++i)
            {
                tokens[i] = tokens[i].Trim();
            }
            return tokens;
        }

        private static string[] TryParseParameters(string text)
        {
            List<string> parameters = new List<string>();
            int depth = 0;
            int prev = 0;
            for (int i = 0; i < text.Length; ++i)
            {
                char current = text[i];
                if (current == '(')
                {
                    ++depth;
                    continue;
                }
                if (current == ')')
                {
                    if (depth < 1)
                    {
                        Logging.Warn("Mismatched parentheses in string: " + text);
                        return null;
                    }
                    --depth;
                    continue;
                }
                if ((current == ',') && (depth == 0))
                {
                    parameters.Add(text.Substring(prev, i - prev));
                    prev = i + 1;
                }
            }
            if (depth != 0)
            {
                Logging.Warn("Mismatched parentheses in string: " + text);
                return null;
            }
            if (prev < text.Length) parameters.Add(text.Substring(prev, text.Length - prev));
            return parameters.ToArray();
        }
    }
}
