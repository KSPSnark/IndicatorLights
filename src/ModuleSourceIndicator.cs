namespace IndicatorLights
{
    /// <summary>
    /// Base class for modules indicator controllers that depend on some other
    /// PartModule (e.g. a stock one) for their color.
    /// </summary>
    public abstract class ModuleSourceIndicator<T> : ModuleEmissiveController where T : PartModule
    {
        private T sourceModule = null;

        /// <summary>
        /// Called when the module is starting up.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            sourceModule = ChooseSource();
            if (sourceModule == null)
            {
                Logging.Warn("No " + typeof(T).Name + " found for " + part.GetTitle());
            }
        }

        public override bool HasColor
        {
            get { return sourceModule != null; }
        }

        protected T SourceModule
        {
            get { return sourceModule; }
        }

        /// <summary>
        /// Choose the source module to use, or null if no suitable source module could be found.
        /// Subclasses can override to customize the choice. Default behavior is simply to pick
        /// the first one found.
        /// </summary>
        /// <returns></returns>
        private T ChooseSource()
        {
            if (part == null) return null;
            for (int i = 0; i < part.Modules.Count; ++i)
            {
                T candidate = part.Modules[i] as T;
                if ((candidate != null) && IsSource(candidate)) return candidate;
            }
            return null;
        }

        /// <summary>
        /// Determine whether the specified module is usable as a source. Default implementation
        /// is to always return true, so the first one found will be chosen.
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>
        protected virtual bool IsSource(T candidate)
        {
            return true;
        }
    }
}
