using System.Collections.Generic;

namespace IndicatorLights
{
    public static class Identifiers
    {
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
        /// Tries to find the module of the specified type that has the specified identifier.
        /// Returns null if not found.
        /// </summary>
        /// <typeparam name="T">The module type to search for.</typeparam>
        /// <param name="part">The part to examine.</param>
        /// <param name="identifier">The identifier to look for. If null or empty, the first module of the specified type will be returned.</param>
        /// <returns></returns>
        public static T FindFirst<T>(Part part, string identifier) where T : class
        {
            if (part == null) return null;
            bool findFirst = string.IsNullOrEmpty(identifier);
            for (int i = 0; i < part.Modules.Count; ++i)
            {
                T candidate = part.Modules[i] as T;
                if (candidate == null) continue;
                IIdentifiable identifiable = candidate as IIdentifiable;
                if (identifiable == null) return null;
                if (findFirst) return candidate;
                if (identifier.Equals(identifiable.Identifier)) return candidate;
            }
            return null;
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
            Logging.Warn("No " + typeof(T).Name + " named '" + identifier + "' found for " + part.GetTitle());
            return null;
        }
    }
}
