namespace IndicatorLights
{
    /// <summary>
    /// Base class for modules that work with resources.
    /// </summary>
    abstract class ModuleResourceIndicator : ModuleEmissiveController
    {
        private PartResource resource = null;

        /// <summary>
        /// Determines which part is searched for the specified resource.
        /// </summary>
        [KSPField]
        public PartSearchStrategy searchStrategy = PartSearchStrategies.Default;

        /// <summary>
        /// The name of the resource which this controller tracks. If left null, the controller will
        /// pick the first resource it finds.
        /// </summary>
        [KSPField]
        public string resourceName = null;

        /// <summary>
        /// Called when the module is starting up.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            resource = FindResource();
            if ((resource == null) && (searchStrategy == PartSearchStrategy.host))
            {
                Logging.Warn("ModuleResourceIndicator is inactive");
                return;
            }
        }

        protected PartResource Resource
        {
            get
            {
                if (searchStrategy != PartSearchStrategy.host)
                {
                    Part sourcePart = (resource == null) ? null : resource.part;
                    if (!searchStrategy.IsChoice(this, sourcePart))
                    {
                        resource = FindResource();
                    }
                }
                return resource;
            }
        }

        /// <summary>
        /// Picks a resource to track.
        /// </summary>
        /// <returns></returns>
        private PartResource FindResource()
        {
            Part sourcePart = searchStrategy.ChoosePart(this);
            if (sourcePart == null) return null;
            if ((sourcePart.Resources == null) || (sourcePart.Resources.Count == 0))
            {
                Logging.Warn(sourcePart.GetTitle() + " has no resources, can't track");
                return null;
            }
            if ((resourceName == null) || (resourceName.Length == 0))
            {
                if (sourcePart.Resources.Count > 1)
                {
                    Logging.Log(sourcePart.GetTitle() + " has multiple resources; indicator is defaulting to " + sourcePart.Resources[0].resourceName);
                }
                return sourcePart.Resources[0];
            }
            for (int i = 0; i < sourcePart.Resources.Count; ++i)
            {
                PartResource resource = sourcePart.Resources[i];
                if (resourceName.Equals(resource.resourceName) && (resource.maxAmount > 0))
                {
                    return resource;
                }
            }
            Logging.Warn("No resource '" + resourceName + "' found in " + sourcePart.GetTitle() + ", can't track");
            return null;
        }
    }
}
