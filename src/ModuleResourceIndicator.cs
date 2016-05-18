namespace IndicatorLights
{
    /// <summary>
    /// Base class for modules that work with resources.
    /// </summary>
    abstract class ModuleResourceIndicator : ModuleEmissiveController
    {
        private PartResource resource = null;

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
            if (resource == null)
            {
                Logging.Warn("ModuleResourceIndicator is inactive");
                return;
            }
        }

        public override bool HasColor
        {
            get { return resource != null; }
        }

        protected PartResource Resource
        {
            get { return resource; }
        }

        /// <summary>
        /// Picks a resource to track.
        /// </summary>
        /// <returns></returns>
        private PartResource FindResource()
        {
            if (part == null) return null;
            if ((part.Resources == null) || (part.Resources.Count == 0))
            {
                Logging.Warn(part.GetTitle() + " has no resources, can't track");
                return null;
            }
            if ((resourceName == null) || (resourceName.Length == 0))
            {
                if (part.Resources.Count > 1)
                {
                    Logging.Log(part.GetTitle() + " has multiple resources; indicator is defaulting to " + part.Resources[0].resourceName);
                }
                return part.Resources[0];
            }
            for (int i = 0; i < part.Resources.Count; ++i)
            {
                PartResource resource = part.Resources[i];
                if (resourceName.Equals(resource.resourceName) && (resource.maxAmount > 0))
                {
                    return resource;
                }
            }
            Logging.Warn("No resource '" + resourceName + "' found in " + part.GetTitle() + ", can't track");
            return null;
        }
    }
}
