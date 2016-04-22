using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Base class for controller modules that work with emissives.
    /// 
    /// Note that there may be multiple controllers targeting the same emissive.
    /// </summary>
    public abstract class ModuleEmissiveController : PartModule
    {
        private ModuleControllableEmissive controlledEmissive = null;

        /// <summary>
        /// This identifies the ModuleControllableEmissive within the part whose material's emissive color
        /// will be adjusted.
        /// </summary>
        [KSPField]
        public string emissiveName = null;

        /// <summary>
        /// Indicates whether this module is "active".  When inactive, it will do nothing to set
        /// the color.  Normally, you'd want this to always be true, if there's just a single
        /// controller module per target material. However, if there are multiple controller modules
        /// targeting the same material, only one should be active at a time so that they don't
        /// end up arm-wrestling over it. Thus, this field is available for a manager module to
        /// use to set the behavior.
        /// </summary>
        [KSPField]
        public bool isControlActive = true;

        /// <summary>
        /// This is used as a "mode" description in the editor. It's displayed when there are multiple
        /// controllers on the part, and the user needs to pick one-- e.g. "do I want this to act like
        /// a toggle on/off light, or a resource indicator, or what".
        /// </summary>
        internal abstract string EditorGuiDescription { get; }

        /// <summary>
        /// This is used to communicate to a controller manager in the editor whether it's possible for this
        /// controller to be used in its current situation or not. Defaults to just return true always.
        /// Should return false if not applicable, e.g. if it's a resource-based controller and it's on a
        /// part with no resource capacity.
        /// </summary>
        internal virtual bool CanControl
        {
            get { return true; }
        }

        /// <summary>
        /// This fires when the module updates in such a way that a controller manager might need to
        /// update (e.g. if the display named changed, or the state of CanControl changed, etc.)
        /// </summary>
        internal Callback<ModuleEmissiveController> ControllerUpdated;

        /// <summary>
        /// Use this to fire the ControllerUpdated event.
        /// </summary>
        protected void AnnounceUpdate()
        {
            if (ControllerUpdated != null)
            {
                ControllerUpdated(this);
            }
        }

        /// <summary>
        /// This is called any time anything changes on the ship.
        /// </summary>
        internal virtual void OnEditorShipModified(ShipConstruct construct)
        {
            // Default behavior is to do nothing
        }

        /// <summary>
        /// This is called any time any part on the ship receives a relevant event.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="affectedPart"></param>
        internal virtual void OnEditorPartEvent(ConstructionEventType eventType, Part affectedPart)
        {
            // Default behavior is to do nothing.
        }

        /// <summary>
        /// Runs when the module starts up. Looks for a corresponding controllable emissive on
        /// the same part that has the same name.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (emissiveName == null)
            {
                Logging.Warn("No controllable emissive specified for " + part.GetTitle());
                return;
            }
            for (int i = 0; i < part.Modules.Count; ++i)
            {
                ModuleControllableEmissive candidate = part.Modules[i] as ModuleControllableEmissive;
                if (candidate == null) continue;
                if (emissiveName.Equals(candidate.emissiveName))
                {
                    // got a match!
                    controlledEmissive = candidate;
                    return;
                }
                Logging.Warn("No controllable emissive '" + emissiveName + "' found for " + part.GetTitle());
            }
        }

        /// <summary>
        /// Gets or sets the color of the controlled emissive.
        /// </summary>
        protected Color Color
        {
            get
            {
                return (controlledEmissive == null) ? Color.black : controlledEmissive.Color;
            }
            set
            {
                if (isControlActive && (controlledEmissive != null)) controlledEmissive.Color = value;
            }
        }
    }
}