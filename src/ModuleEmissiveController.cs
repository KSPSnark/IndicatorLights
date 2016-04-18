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
        /// <summary>
        /// This identifies the target renderer within the part whose material's emissive color
        /// will be adjusted.
        /// </summary>
        [KSPField]
        public string target = null;

        private ModuleControllableEmissive emissive = null;

        /// <summary>
        /// Runs when the module starts up. Looks for a corresponding controllable emissive on
        /// the same part that has the same name.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (target == null) return;
            for (int i = 0; i < part.Modules.Count; ++i)
            {
                ModuleControllableEmissive candidate = part.Modules[i] as ModuleControllableEmissive;
                if (candidate == null) continue;
                if (target.Equals(candidate.target))
                {
                    // got a match!
                    emissive = candidate;
                    return;
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the controlled emissive.
        /// </summary>
        protected Color Color
        {
            get
            {
                return (emissive == null) ? Color.black : emissive.Color;
            }
            set
            {
                if (emissive != null) emissive.Color = value;
            }
        }
    }
}