using System;

namespace IndicatorLights
{
    /// <summary>
    /// Used for designating controllers that are considered "experimental": i.e. subject to change
    /// or even deletion without notice, so nobody should be building code or config that depends
    /// on it if they care what happens to it.
    ///
    /// The purpose is to allow "pre-releasing" early rough drafts of new ideas, to give people
    /// a chance to play around with it and provide feedback, while making clear that it's
    /// subject to breaking changes without warning.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExperimentalController : Attribute
    {
        /// <summary>
        /// Gets whether the specified controller is of an experimental type.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static bool Is(ModuleEmissiveControllerBase controller)
        {
            return controller.GetType().IsDefined(typeof(ExperimentalController), true);
        }
    }
}
