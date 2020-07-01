using Experience;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
            Between.TryParse,
            VesselSituationMatch.TryParse,
            VesselControlLevelMatch.TryParse,
            CrewEffectMatch.TryParse
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

            // Could it be a named-field reference?
            Identifiers.IFieldEvaluator field = Identifiers.FindKSPField(module.part, text);
            if (field != null)
            {
                if (!typeof(bool).IsAssignableFrom(field.FieldType))
                {
                    throw new ArgumentException("Can't use " + text + " as a boolean field (it's of type " + field.FieldType.Name + ")");
                }
                return new NamedField(field);
            }

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


        #region VesselSituationMatch
        /// <summary>
        /// Returns true if the vessel's current situation matches any of the specified flags.
        /// </summary>
        private class VesselSituationMatch : IToggle
        {
            private const string TYPE_NAME = "situation";
            private static readonly Vessel.Situations[] ALL_SITUATIONS = (Vessel.Situations[])Enum.GetValues(typeof(Vessel.Situations));
            private static readonly string ALL_SITUATIONS_LIST = BuildList(ALL_SITUATIONS);
            private readonly PartModule module;
            private readonly int requiredSituations;

            public bool ToggleStatus
            {
                get
                {
                    if (module.part == null) return false;
                    if (module.part.vessel == null) return false;
                    int currentSituations = (int)module.part.vessel.situation;
                    return (currentSituations & requiredSituations) != 0;
                }
            }

            /// <summary>
            /// Try to get a "vessel situation" matcher from a ParsedParameters. The expected format is:
            ///
            /// situation(situation1, situation2, ...)
            ///
            /// Must have at least one situation. The allowed situation values are the enum constants
            /// in Vessel.Situations.
            /// </summary>
            /// <param name="module"></param>
            /// <param name="parsedParams"></param>
            /// <returns></returns>
            public static IToggle TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (!TYPE_NAME.Equals(parsedParams.Identifier)) return null;
                parsedParams.RequireCount(module, 1, -1);
                Vessel.Situations requiredSituations = ParseSituation(module, parsedParams[0]);
                for (int i = 1; i < parsedParams.Count; ++i)
                {
                    requiredSituations |= ParseSituation(module, parsedParams[i]);
                }
                return new VesselSituationMatch(module, requiredSituations);
            }

            private VesselSituationMatch(PartModule module, Vessel.Situations requiredSituations)
            {
                this.module = module;
                this.requiredSituations = (int)requiredSituations;
            }

            private static Vessel.Situations ParseSituation(PartModule module, string param)
            {
                for (int i = 0; i < ALL_SITUATIONS.Length; ++i)
                {
                    if (ALL_SITUATIONS[i].ToString().Equals(param))
                    {
                        return ALL_SITUATIONS[i];
                    }
                }
                // Invalid situation specified!
                throw new ArgumentException(
                    "Invalid Vessel.Situations value '" + param + "' for " + TYPE_NAME
                    + "() on " + module.ClassName + " of " + module.part.GetTitle()
                    + " (valid values are: " + ALL_SITUATIONS_LIST + ")");
            }

            private static string BuildList(Vessel.Situations[] situations)
            {
                StringBuilder builder = new StringBuilder(situations[0].ToString());
                for (int i = 1; i < situations.Length; ++i)
                {
                    builder.Append(", ").Append(situations[i]);
                }
                return builder.ToStringAndRelease();
            }
        }
        #endregion


        #region VesselControlLevelMatch
        /// <summary>
        /// Returns true if the vessel's current situation matches any of the specified flags.
        /// </summary>
        private class VesselControlLevelMatch : IToggle
        {
            private const string TYPE_NAME = "controlLevel";
            private static readonly Vessel.ControlLevel[] ALL_LEVELS = (Vessel.ControlLevel[])Enum.GetValues(typeof(Vessel.ControlLevel));
            private static readonly string ALL_LEVELS_LIST = BuildList(ALL_LEVELS);
            private readonly PartModule module;
            private readonly HashSet<Vessel.ControlLevel> requiredLevels;

            public bool ToggleStatus
            {
                get
                {
                    if (module.part == null) return false;
                    if (module.part.vessel == null) return false;
                    Vessel.ControlLevel currentControlLevel = module.part.vessel.CurrentControlLevel;
                    return requiredLevels.Contains(currentControlLevel);
                }
            }

            /// <summary>
            /// Try to get a "vessel control level" matcher from a ParsedParameters. The expected format is:
            ///
            /// controlLevel(level1, level2, ...)
            ///
            /// Must have at least one level. The allowed control level values are the enum constants
            /// in Vessel.ControlLevel.
            /// </summary>
            /// <param name="module"></param>
            /// <param name="parsedParams"></param>
            /// <returns></returns>
            public static IToggle TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (!TYPE_NAME.Equals(parsedParams.Identifier)) return null;
                parsedParams.RequireCount(module, 1, -1);
                HashSet<Vessel.ControlLevel> requiredLevels = new HashSet<Vessel.ControlLevel>();
                for (int i = 0; i < parsedParams.Count; ++i)
                {
                    Vessel.ControlLevel additionalControlLevel = ParseLevel(module, parsedParams[i]);
                    if (!requiredLevels.Add(additionalControlLevel))
                    {
                        throw new ArgumentException("Duplicate specification of '" + parsedParams[i] + "' for " + TYPE_NAME
                            + "() on " + module.ClassName + " of " + module.part.GetTitle());
                    }
                }
                return new VesselControlLevelMatch(module, requiredLevels);
            }

            private VesselControlLevelMatch(PartModule module, HashSet<Vessel.ControlLevel> requiredLevels)
            {
                this.module = module;
                this.requiredLevels = requiredLevels;
            }

            private static Vessel.ControlLevel ParseLevel(PartModule module, string param)
            {
                for (int i = 0; i < ALL_LEVELS.Length; ++i)
                {
                    if (ALL_LEVELS[i].ToString().Equals(param))
                    {
                        return ALL_LEVELS[i];
                    }
                }
                // Invalid situation specified!
                throw new ArgumentException(
                    "Invalid Vessel.ControlLevel value '" + param + "' for " + TYPE_NAME
                    + "() on " + module.ClassName + " of " + module.part.GetTitle()
                    + " (valid values are: " + ALL_LEVELS_LIST + ")");
            }

            private static string BuildList(Vessel.ControlLevel[] levels)
            {
                StringBuilder builder = new StringBuilder(levels[0].ToString());
                for (int i = 1; i < levels.Length; ++i)
                {
                    builder.Append(", ").Append(levels[i]);
                }
                return builder.ToStringAndRelease();
            }
        }
        #endregion


        #region CrewEffectMatch
        /// <summary>
        /// Returns true if a crew slot has the desired effect.
        /// </summary>
        private class CrewEffectMatch : IToggle
        {
            private const string TYPE_NAME = "hasCrewEffect";
            private static readonly TimeSpan UPDATE_INTERVAL = TimeSpan.FromMilliseconds(250);
            private readonly PartModule module;
            private readonly string effectName;
            private readonly int slot;
            private readonly int minLevel;
            private bool cachedValue = false;
            private DateTime nextUpdate = DateTime.MinValue;


            private CrewEffectMatch(PartModule module, string effectName, int slot, int minLevel)
            {
                this.module = module;
                this.effectName = effectName;
                this.slot = slot;
                this.minLevel = minLevel;
            }

            /// <summary>
            /// Try to get a "crew effect" matcher from a ParsedParameters. The expected format is:
            ///
            /// hasCrewEffect(effectName, slot)
            /// hasCrewEffect(effectName, slot, minLevel)
            ///
            /// ...where effectName is the name of the effect to check for (e.g. "ScienceSkill" or whatever;
            /// these are generally the names of ExperienceEffect classes in the Experience.Effects namespace),
            /// slot is the crew slot to evaluate (-1 for "any"), and minLevel is the minimum experience level
            /// needed for the ExperienceEffect to apply ("any level" if omitted).
            /// </summary>
            public static IToggle TryParse(PartModule module, ParsedParameters parsedParams)
            {
                if (parsedParams == null) return null;
                if (!TYPE_NAME.Equals(parsedParams.Identifier)) return null;
                if (module == null) return null;
                parsedParams.RequireCount(module, 2, 3);
                string effectName = parsedParams[0];
                if (!Kerbals.Effects.Contains(effectName))
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (string validName in Kerbals.Effects)
                    {
                        builder.Append(" ").Append(validName);
                    }
                    throw new ArgumentException(
                        "Invalid effect name '" + effectName + "'. Valid values are:"
                        + builder.ToString());
                }
                int slot = (int)Statics.Parse(module, parsedParams[1]);
                int minLevel = (parsedParams.Count > 2) ? (int)Statics.Parse(module, parsedParams[2]) : -1;
                return new CrewEffectMatch(module, effectName, slot, minLevel);
            }

            /// <summary>
            /// IToggle implementation. Returns true if the crew trait requirement is met.
            /// </summary>
            public bool ToggleStatus
            {
                get
                {
                    DateTime now = DateTime.Now;
                    if (now > nextUpdate)
                    {
                        nextUpdate = now + UPDATE_INTERVAL;
                        cachedValue = CalculateStatus();
                    }
                    return cachedValue;
                }
            }

            private bool CalculateStatus()
            {
                if (module.part == null) return false;
                List<ProtoCrewMember> crew = module.part.protoModuleCrew;
                if (crew == null) return false;
                if (slot >= crew.Count) return false;

                // If they specified a particular slot, then the specific slot has to match.
                if (slot >= 0) return Matches(crew[slot]);

                // If no slot is specified, then it's true if *any* slot matches.
                for (int i = 0; i < crew.Count; ++i)
                {
                    if (Matches(crew[i])) return true;
                }

                // There's no matching slot.
                return false;
            }

            /// <summary>
            /// Returns true if the specified crew member matches the trait we're looking for.
            /// </summary>
            /// <param name="crew"></param>
            /// <returns></returns>
            private bool Matches(ProtoCrewMember crew)
            {
                ExperienceTrait trait = crew.experienceTrait;
                if (trait == null) return false;
                List<ExperienceEffect> effects = trait.Effects;
                if (effects == null) return false;
                for (int i = 0; i < effects.Count; ++i)
                {
                    ExperienceEffect effect = effects[i];
                    if (effectName == effect.Name)
                    {
                        return trait.CrewMemberExperienceLevel() >= minLevel;
                    }
                }
                return false;
            }
        }
        #endregion


        #region NamedField
        /// <summary>
        /// This syntax is for a named boolean field of an arbitrary module.
        /// </summary>
        private class NamedField : IToggle
        {
            private Identifiers.IFieldEvaluator field;

            public NamedField(Identifiers.IFieldEvaluator field)
            {
                this.field = field;
            }

            public bool ToggleStatus
            {
                get
                {
                    return (bool)field.Value;
                }
            }
        }
        #endregion // NamedField
    }
}
