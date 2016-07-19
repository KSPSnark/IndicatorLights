using System.Collections.Generic;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Base class for controller modules that work with emissives.
    /// 
    /// Note that there may be multiple controllers targeting the same emissive.
    /// </summary>
    public abstract class ModuleEmissiveController : PartModule, IColorSource
    {
        private List<ModuleControllableEmissive> controlledEmissives = null;

        /// <summary>
        /// This identifies the ModuleControllableEmissive within the part whose material's emissive color
        /// will be adjusted.
        /// </summary>
        [KSPField]
        public string emissiveName = null;

        /// <summary>
        /// This is used to uniquely identify a particular controller. Use the ControllerName
        /// property to access at run time. If not specified, ControllerName will simply
        /// return the name of the class.
        /// 
        /// The use case for this property is if you want to have a controller that specifies
        /// an output color, but doesn't actually point at any emissiveName that it controls.
        /// This can be useful if you want to have "compound controllers", where the output
        /// from one controller serves as an input for another.
        /// </summary>
        [KSPField]
        public string controllerName = null;

        /// <summary>
        /// Indicates whether the module's UI (if any) is enabled on the part. This base
        /// class has no UI, but subclasses might.  If they do, they should override
        /// the OnUiEnabled property.
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool isUiEnabled = true;

        /// <summary>
        /// Call this at runtime to identify the controller.
        /// </summary>
        public string ColorSourceID
        {
            get
            {
                return string.IsNullOrEmpty(controllerName) ? GetType().Name : controllerName;
            }
        }

        /// <summary>
        /// Gets whether this controller has an output color available. Default implementation
        /// is to return true. The contract is that if this returns false, should not try to
        /// get the OutputColor.
        /// </summary>
        public virtual bool HasColor
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the output color of this controller. Won't be called unless HasColor
        /// returns true.
        /// </summary>
        public abstract Color OutputColor { get; }

        /// <summary>
        /// Called on every frame. Note that this implements Unity's Update method, rather
        /// than overriding the OnUpdate method of PartModule, because this needs to get
        /// called regardless of whether the part is active or not.
        /// </summary>
        void Update()
        {
            if ((controlledEmissives != null) && HasColor)
            {
                for (int i = 0; i < controlledEmissives.Count; ++i)
                {
                    controlledEmissives[i].Color = OutputColor;
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

            controlledEmissives = HasEmissive ? Identifiers.FindAll<ModuleControllableEmissive>(part, emissiveName) : null;
            SetUiEnabled(isUiEnabled);
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

        protected IColorSource FindColorSource(string sourceID)
        {
            return ColorSources.Find(this, sourceID);
        }

        private bool HasEmissive
        {
            get { return !string.IsNullOrEmpty(emissiveName); }
        }
    }
}