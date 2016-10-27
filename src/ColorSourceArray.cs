namespace IndicatorLights
{
    /// <summary>
    /// Lazy-loading placeholder for an array of color sources, useful for emissive
    /// controllers that work with arrays of meshes.
    /// </summary>
    internal class ColorSourceArray
    {
        private readonly string colorSourceID;
        private IColorSource[] sources = null;
        private bool isValid = false;
        private bool isInitialized = false;

        private ColorSourceArray(string colorSourceID)
        {
            this.colorSourceID = colorSourceID;
        }

        /// <summary>
        /// Get a ColorSourceArray for the specified ID.
        /// </summary>
        /// <param name="colorSourceID"></param>
        /// <returns></returns>
        public static ColorSourceArray of(string colorSourceID)
        {
            return new ColorSourceArray(colorSourceID);
        }

        /// <summary>
        /// Try to set the colors of the meshes on the specified controller.
        /// </summary>
        /// <param name="controller"></param>
        public void SetColors(ModuleEmissiveControllerBase controller)
        {
            if (!TryInitialize(controller)) return;

            int sourceIndex = 0;
            for (int emissiveIndex = 0; emissiveIndex < controller.Emissives.Count; ++emissiveIndex)
            {
                ModuleControllableEmissive emissive = controller.Emissives[emissiveIndex];
                for (int meshIndex = 0; meshIndex < emissive.Count; ++meshIndex)
                {
                    if (sourceIndex >= sources.Length) return;
                    IColorSource source = sources[sourceIndex++];
                    if (source.HasColor) emissive.SetColorAt(source.OutputColor, meshIndex);
                }
            }
        }

        private bool TryInitialize(ModuleEmissiveControllerBase controller)
        {
            if (isInitialized) return isValid;
            if ((controller == null) || (controller.Emissives == null) || (controller.Emissives.Count == 0)) return DoneInitializing(false);
            int total = 0;
            for (int emissiveIndex = 0; emissiveIndex < controller.Emissives.Count; ++emissiveIndex)
            {
                int count = controller.Emissives[emissiveIndex].Count;
                if (count < 0) return false; // not yet initialized, we'll try again later
                total += count;
            }
            if (total < 1) return DoneInitializing(false);

            sources = ColorSources.FindWithIndex(controller, colorSourceID, total);
            return DoneInitializing(true);
        }

        private bool DoneInitializing(bool success)
        {
            isValid = success;
            isInitialized = true;
            return isValid;
        }
    }
}
