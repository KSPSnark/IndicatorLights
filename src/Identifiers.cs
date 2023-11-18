using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IndicatorLights
{
    public static class Identifiers
    {
        /// <summary>
        /// Used as a special identifier meaning "the PartModule evaluating this"
        /// </summary>
        public const string THIS = "this";

        private const string IDENTIFIER_CHAR_PATTERN = "[A-Za-z0-9_]+";
        /// <summary>
        /// Pattern for strings used as identifiers.
        /// </summary>
        private static readonly Regex IDENTIFIER_PATTERN = new Regex("^" + IDENTIFIER_CHAR_PATTERN + "$");

        /// <summary>
        /// Pattern for field-on-module identifiers.
        /// </summary>
        private static readonly Regex FIELD_ON_MODULE_PATTERN = new Regex("^(" + IDENTIFIER_CHAR_PATTERN + ")\\s*@\\s*(" + IDENTIFIER_CHAR_PATTERN + ")$");

        public interface IFieldEvaluator {
            object Value { get; }
            Type FieldType { get; }
        }

        /// <summary>
        /// Interface for objects that can be found via identifiers.
        /// </summary>
        public interface IIdentifiable
        {
            /// <summary>
            /// Gets the identifier string used to locate this object.
            /// </summary>
            string Identifier { get; }
        }

        /// <summary>
        /// Gets whether the given string has the right format to be a simple identifier.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static bool IsSimpleIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier)) return false;
            return IDENTIFIER_PATTERN.Match(identifier).Success;
        }



        /// <summary>
        /// Tries to find the module or interface of the specified type that has the specified identifier.
        /// Returns null if not found.
        /// </summary>
        /// <typeparam name="T">The type to search for. Typically this will either be a PartModule, or some interface type such as IScalar or IToggle.</typeparam>
        /// <param name="part">The part to examine.</param>
        /// <param name="identifier">The identifier to look for. If null or empty, the first module of the specified type will be returned.</param>
        /// <returns></returns>
        public static T FindFirst<T>(Part part, string identifier) where T : class
        {
            if (part == null) return null;
            identifier = identifier.Trim();
            bool findFirst = string.IsNullOrEmpty(identifier);
            for (int i = 0; i < part.Modules.Count; ++i)
            {
                T candidate = part.Modules[i] as T;
                if (candidate == null) continue;
                if (findFirst) return candidate;
                if (identifier.Equals(identifierOf(candidate))) return candidate;
            }
            return null;
        }

        /// <summary>
        /// Try to find a field on a part module, if it's of the form fieldname@moduleidentifier.
        /// Returns null if not found (e.g. if it's not in that syntax).  Throws ArgumentException
        /// if it *is* in that syntactic form, but there's a problem with it.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static IFieldEvaluator FindKSPField(Part part, string identifier)
        {
            // First, try to parse it into a field name and a module identifier.
            identifier = identifier.Trim();
            Match match = FIELD_ON_MODULE_PATTERN.Match(identifier);
            if (!match.Success) return null; // it's not of the form "field@module"
            string fieldName = match.Groups[1].Value;
            string moduleIdentifier = match.Groups[2].Value;

            // Try to actually find a module that matches.
            PartModule module = FindFirst<PartModule>(part, moduleIdentifier);
            if (module == null)
            {
                Logging.Warn("Can't find " + identifier + " (no PartModule found on " + part.GetTitle() + " that matches '" + moduleIdentifier + "')");
                return null;
            }

            // Does it have a public field with that name?
            FieldInfo field = module.GetType().GetField(
                            fieldName,
                            BindingFlags.Public | BindingFlags.Instance);
            if (object.ReferenceEquals(field, null))
            {
                throw new ArgumentException("Can't find " + identifier + " (" + module.GetType().Name + " has no public field '" + fieldName + "')");
            }

            // Is the field a KSP field?
            if (!field.IsDefined(typeof(KSPField), true))
            {
                throw new ArgumentException("Can't reference " + identifier + " (" + module.GetType().Name + "." + fieldName + " isn't a KSPField)");
            }

            return new FieldEvaluator(module, field);
        }

        private class FieldEvaluator : IFieldEvaluator
        {
            private readonly PartModule module;
            private readonly FieldInfo field;

            public FieldEvaluator(PartModule module, FieldInfo field)
            {
                this.module = module;
                this.field = field;
            }

            public object Value
            {
                get
                {
                    return field.GetValue(module);
                }
            }

            public Type FieldType
            {
                get { return field.FieldType; }
            }
        }

        /// <summary>
        /// Tries to find all the modules of the specified type that have the specified identifier.
        /// Returns null if not found.
        /// </summary>
        /// <typeparam name="T">The module type to search for.</typeparam>
        /// <param name="part">The part to examine.</param>
        /// <param name="identifier">The identifier to look for. If null or empty, all modules of the specified type will be found.</param>
        /// <returns></returns>
        public static List<T> FindAll<T>(Part part, string identifier) where T : class
        {
            if (part == null) return null;
            bool findAll = string.IsNullOrEmpty(identifier);
            List<T> items = new List<T>();
            for (int i = 0; i < part.Modules.Count; ++i)
            {
                T candidate = part.Modules[i] as T;
                if (candidate == null) continue;
                IIdentifiable identifiable = candidate as IIdentifiable;
                if (identifiable == null) continue;
                if (findAll || identifier.Equals(identifiable.Identifier))
                {
                    // got a match!
                    items.Add(candidate);
                }
            }
            if (items.Count > 0) return items;
            if (findAll)
            {
                Logging.Warn("No " + typeof(T).Name + " found for " + part.GetTitle());
            }
            else
            {
                Logging.Warn("No " + typeof(T).Name + " named '" + identifier + "' found for " + part.GetTitle());
            }
            return null;
        }

        private static string identifierOf(object thing)
        {
            IIdentifiable identifiable = thing as IIdentifiable;
            return (identifiable == null) ? thing.GetType().Name : identifiable.Identifier;
        }
    }
}
