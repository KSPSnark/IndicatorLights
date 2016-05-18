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

            sourceModule = FindFirst<T>();
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
    }
}
