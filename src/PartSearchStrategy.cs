namespace IndicatorLights
{
    /// <summary>
    /// Determines how IndicatorLights modules search the ship's parts when
    /// looking for target modules to interoperate with.
    /// </summary>
    public enum PartSearchStrategy
    {
        /// <summary>
        /// Search the host part only (i.e. the part where the module lives).
        /// </summary>
        host,

        /// <summary>
        /// Search the parent part only (i.e. the direct parent of the part where the module lives).
        /// </summary>
        parent
    }

    /// <summary>
    /// Extension methods for PartSearchStrategy.
    /// </summary>
    internal static class PartSearchStrategies
    {
        /// <summary>
        /// The search strategy to use for everything, unless specified otherwise.
        /// </summary>
        public const PartSearchStrategy Default = PartSearchStrategy.host;

        /// <summary>
        /// Choose the part to use. Returns null if not found.
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public static Part ChoosePart(this PartSearchStrategy strategy, PartModule module)
        {
            if (module == null) return null;
            switch (strategy)
            {
                case PartSearchStrategy.host:
                    return module.part;

                case PartSearchStrategy.parent:
                    return (module.part == null) ? null : module.part.parent;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets whether the specified candidate part is the chosen part for the specified module.
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="module"></param>
        /// <param name="candidatePart"></param>
        /// <returns></returns>
        public static bool IsChoice(this PartSearchStrategy strategy, PartModule module, Part candidatePart)
        {
            if (module == null) return candidatePart == null;
            switch (strategy)
            {
                case PartSearchStrategy.host:
                    return false;
                case PartSearchStrategy.parent:
                    return object.ReferenceEquals(candidatePart, ChoosePart(strategy, module));
                default:
                    return false;
            }
        }
    }
}
