using System.Collections.Generic;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Base class for "simple" emissive controller modules that have only a single
    /// output color.
    /// 
    /// Note that there may be multiple controllers targeting the same emissive.
    /// </summary>
    public abstract class ModuleEmissiveController : ModuleEmissiveControllerBase, IColorSource, Identifiers.IIdentifiable
    {
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
        /// Called on every frame when it's time to set colors on the controllable emissives.
        /// </summary>
        protected override void SetColors()
        {
            if (HasColor)
            {
                List<ModuleControllableEmissive> controlledEmissives = Emissives;
                for (int i = 0; i < controlledEmissives.Count; ++i)
                {
                    controlledEmissives[i].Color = OutputColor;
                }
            }
        }

        public string Identifier
        {
            get
            {
                return ColorSourceID;
            }
        }
    }
}