using System.Collections.Generic;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Base class for controller modules that work with emissives.
    ///
    /// Note that there may be multiple controllers targeting the same emissive.
    /// </summary>
    public abstract class ModuleEmissiveControllerBase : PartModule
    {
        private List<ModuleControllableEmissive> controlledEmissives = null;

        /// <summary>
        /// This identifies the ModuleControllableEmissive within the part whose material's emissive color
        /// will be adjusted.
        /// </summary>
        [KSPField]
        public string emissiveName = null;

        /// <summary>
        /// Indicates whether the module's UI (if any) is enabled on the part. This base
        /// class has no UI, but subclasses might.  If they do, they should override
        /// the OnUiEnabled method.
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool isUiEnabled = true;

        /// <summary>
        /// Called on every frame. Note that this implements Unity's Update method, rather
        /// than overriding the OnUpdate method of PartModule, because this needs to get
        /// called regardless of whether the part is active or not.
        /// </summary>
        void Update()
        {
            if (controlledEmissives == null) return;
            if (HighLogic.LoadedSceneIsFlight && PhysicsGlobals.ThermalColorsDebug) return;
            if (GlobalSettings.IsEnabled)
            {
                SetColors();
            }
            else
            {
                for (int i = 0; i < controlledEmissives.Count; ++i)
                {
                    controlledEmissives[i].Color = Color.black;
                }
            }
        }

        /// <summary>
        /// Runs when the module starts up. Looks for a corresponding controllable emissive on
        /// the same part that has the same name.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (ExperimentalController.Is(this))
            {
                // Log an error so that it loudly announces itself, in case someone didn't
                // get the memo "hey, this is experimental!"
                Logging.Error("Warning! " + ClassName + " (on " + part.GetTitle()
                    + ") is an experimental controller and subject to change without notice.");
            }

            controlledEmissives = HasEmissive ? Identifiers.FindAll<ModuleControllableEmissive>(part, emissiveName) : null;
            SetUiEnabled(isUiEnabled);
        }

        /// <summary>
        /// Get a description of the controller's state, suitable for display in
        /// the debug console. Default behavior is to return null, meaning no
        /// special information present.
        /// </summary>
        /// <returns></returns>
        public virtual string DebugDescription
        {
            get { return null; }
        }

        /// <summary>
        /// Subclasses should call this to enable or disable UI.
        /// </summary>
        /// <param name="enabled"></param>
        internal void SetUiEnabled(bool enabled)
        {
            OnUiEnabled(enabled);
            isUiEnabled = enabled;
        }

        /// <summary>
        /// Called on every frame when it's time to set colors on the controllable emissives.
        /// </summary>
        protected abstract void SetColors();

        internal List<ModuleControllableEmissive> Emissives
        {
            get { return controlledEmissives; }
        }

        /// <summary>
        /// This is called whenever UI is toggled for the module. Subclasses should
        /// do whatever is appropriate to activate/deactivate their UI.
        /// </summary>
        /// <param name="enabled"></param>
        protected virtual void OnUiEnabled(bool enabled)
        {
            // Base class has no UI, so default behavior is to do nothing.
        }

        /// <summary>
        /// Try to find a PartModule of the specified class; null if none.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T FindFirst<T>() where T : PartModule
        {
            if (part == null) return null;
            for (int i = 0; i < part.Modules.Count; ++i)
            {
                T candidate = part.Modules[i] as T;
                if (candidate != null) return candidate;
            }
            return null;
        }

        /// <summary>
        /// Find the specified color source.
        /// </summary>
        /// <param name="sourceID"></param>
        /// <returns></returns>
        protected IColorSource FindColorSource(string sourceID)
        {
            return ColorSources.Find(this, sourceID);
        }

        /// <summary>
        /// Find the specified toggle. Throws ArgumentException if there's a problem.
        /// </summary>
        /// <param name="toggleID"></param>
        /// <returns></returns>
        protected IToggle FindToggle(string toggleID)
        {
            return Toggles.Parse(this, toggleID);
        }

        private bool HasEmissive
        {
            get { return !string.IsNullOrEmpty(emissiveName); }
        }
    }
}