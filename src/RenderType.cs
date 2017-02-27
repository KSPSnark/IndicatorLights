using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// These are the valid values for the renderType property on ModuleControllableEmissive.
    /// </summary>
    public enum RenderType
    {
        /// <summary>
        /// Controls the emissive color of the mesh.
        /// </summary>
        emissive,

        /// <summary>
        /// Controls the tint color of the mesh.
        /// </summary>
        tint,

        /// <summary>
        /// Controls the "main" color of the mesh (e.g. the diffuse color, for many shaders).
        /// </summary>
        main
    }

    /// <summary>
    /// Extension methods for RenderType.
    /// </summary>
    internal static class RenderTypes
    {
        /// <summary>
        /// The render type to use for everything, unless specified otherwise.
        /// </summary>
        public const RenderType Default = RenderType.emissive;

        private static readonly int EMISSIVE_COLOR_ID = Shader.PropertyToID("_EmissiveColor");
        private static readonly int TINT_COLOR_ID = Shader.PropertyToID("_TintColor");
        private static readonly int MAIN_COLOR_ID = Shader.PropertyToID("_Color");

        /// <summary>
        /// Gets the Unity shader property ID for this render type.
        /// </summary>
        /// <param name="renderType"></param>
        /// <returns></returns>
        public static int GetShaderPropertyId(this RenderType renderType)
        {
            switch (renderType)
            {
                case RenderType.emissive:
                    return EMISSIVE_COLOR_ID;

                case RenderType.tint:
                    return TINT_COLOR_ID;

                case RenderType.main:
                    return MAIN_COLOR_ID;

                default:
                    // Should never happen-- this means there's a bug, i.e. a new RenderType was added
                    // without updating here.
                    Logging.Error("Unknown render type " + renderType + ", defaulting to " + Default);
                    return EMISSIVE_COLOR_ID;
            }
        }
    }
}
