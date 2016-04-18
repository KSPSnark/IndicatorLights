using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// This module keeps track of emissive materials on a part corresponding to a given
    /// target name (this is the name of the Unity renderer). It has a gettable/settable
    /// emissive color that will change the renderer's property dynamically.
    /// 
    /// Typically there will be one instance of this module on a part for each emissive
    /// renderer present.
    /// </summary>
    public class ModuleControllableEmissive : PartModule
    {
        private static readonly int emissiveColorId = Shader.PropertyToID("_EmissiveColor");
        private static readonly Material[] NO_MATERIALS = new Material[0];

        private Material[] materials;

        /// <summary>
        /// This identifies the target renderer within the part whose material's emissive color
        /// will be adjusted.
        /// </summary>
        [KSPField]
        public string target = null;

        /// <summary>
        /// Gets or sets the emissive color of the module.
        /// </summary>
        public Color Color
        {
            get
            {
                return (Materials.Length > 0) ? Materials[0].GetColor(emissiveColorId) : Color.black;
            }
            set
            {
                for (int i = 0; i < Materials.Length; ++i)
                {
                    Materials[i].SetColor(emissiveColorId, value);
                }
            }
        }

        private Material[] GetEmissiveMaterials()
        {
            if (part == null) return NO_MATERIALS;
            if (part.transform == null) return NO_MATERIALS;
            if (target == null)
            {
                Logging.Warn("No emissive target identified for " + Logging.ToString(part));
                return null;
            }
            MeshRenderer[] renderers = part.transform.GetComponentsInChildren<MeshRenderer>();
            if (renderers == null) return null;
            int count = 0;
            for (int rendererIndex = 0; rendererIndex < renderers.Length; ++rendererIndex)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null) continue;
                if (!target.Equals(renderer.name)) continue;
                ++count;
            }
            if (count < 1) return NO_MATERIALS;
            Material[] emissiveMaterials = new Material[count];
            count = 0;
            for (int rendererIndex = 0; rendererIndex < renderers.Length; ++rendererIndex)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null) continue;
                if (!target.Equals(renderer.name)) continue;
                emissiveMaterials[count++] = renderer.material;
            }

            return emissiveMaterials;
        }

        private Material[] Materials
        {
            get
            {
                if (materials == null)
                {
                    materials = GetEmissiveMaterials();
                }
                return materials;
            }
        }
    }
}
