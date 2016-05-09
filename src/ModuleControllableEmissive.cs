using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        private static readonly Regex TARGET_PATTERN = new Regex("^(.+):(\\d+)$");
        private static readonly int emissiveColorId = Shader.PropertyToID("_EmissiveColor");
        private static readonly Material[] NO_MATERIALS = new Material[0];

        private Material[] materials;
        private Color cachedColor = new Color(0, 0, 0, 0);

        /// <summary>
        /// This identifies the target renderer within the part whose material's emissive color
        /// will be adjusted.
        /// </summary>
        [KSPField]
        public string target = null;

        /// <summary>
        /// This is an identifier used by controllers to find the emissive to control.
        /// </summary>
        [KSPField]
        public string emissiveName = null;

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
                if (!cachedColor.Equals(value))
                {
                    cachedColor = value;
                    for (int i = 0; i < Materials.Length; ++i)
                    {
                        Materials[i].SetColor(emissiveColorId, value);
                    }
                }
            }
        }

        /// <summary>
        /// Tries to find all the emissives with the specified name, or null if not found.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="emissiveName"></param>
        /// <returns></returns>
        public static List<ModuleControllableEmissive> Find(Part part, string emissiveName)
        {
            if ((emissiveName == null) || string.Empty.Equals(emissiveName)) return null;
            List<ModuleControllableEmissive> emissives = new List<ModuleControllableEmissive>();
            for (int i = 0; i < part.Modules.Count; ++i)
            {
                ModuleControllableEmissive candidate = part.Modules[i] as ModuleControllableEmissive;
                if (candidate == null) continue;
                if (emissiveName.Equals(candidate.emissiveName))
                {
                    // got a match!
                    emissives.Add(candidate);
                }
            }
            if (emissives.Count > 0) return emissives;
            Logging.Warn("No controllable emissive '" + emissiveName + "' found for " + part.GetTitle());
            return null;
        }

        /// <summary>
        /// Searches the part for all emissive materials that match the specified target for this module.
        /// Special case is when the target ends with a colon and a number, in which case it only tries
        /// to get the Nth such instance.
        /// </summary>
        /// <returns></returns>
        private Material[] GetEmissiveMaterials()
        {
            if (part == null) return NO_MATERIALS;
            if (part.transform == null) return NO_MATERIALS;
            if (target == null)
            {
                Logging.Warn("No emissive target identified for " + part.GetTitle());
                return null;
            }
            MeshRenderer[] renderers = part.transform.GetComponentsInChildren<MeshRenderer>();
            if (renderers == null) return null;

            // Special case: if target ends with a colon and an integer, only get the Nth emissive material
            // of that name.
            Match match = TARGET_PATTERN.Match(target);
            int targetIndex = -1;
            if (match.Success)
            {
                target = match.Groups[1].Value;
                targetIndex = int.Parse(match.Groups[2].Value);
            }

            int count = 0;

            // If a model is added via ModuleManager config, it looks like it gets "(Clone)" added
            // to the end of the renderer name.
            string CLONE_TAG = "(Clone)";
            string cloneTarget = target + CLONE_TAG;

            for (int rendererIndex = 0; rendererIndex < renderers.Length; ++rendererIndex)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null) continue;
                if (!target.Equals(renderer.name) && !cloneTarget.Equals(renderer.name)) continue;
                ++count;
            }
            if (count < 1)
            {
                Logging.Warn("No emissive materials named '" + target + "' could be identified for " + part.GetTitle());
                for (int rendererIndex = 0; rendererIndex < renderers.Length; ++rendererIndex)
                {
                    string rendererName = renderers[rendererIndex].name;
                    if (rendererName.EndsWith(CLONE_TAG)) rendererName = rendererName.Substring(rendererName.Length - CLONE_TAG.Length);
                    Logging.Warn("Did you mean this?  " + rendererName);
                }
                return NO_MATERIALS;
            }
            if (targetIndex >= count) targetIndex = -1;
            Material[] emissiveMaterials = (targetIndex < 0) ? new Material[count] : new Material[1];
            count = 0;
            for (int rendererIndex = 0; rendererIndex < renderers.Length; ++rendererIndex)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null) continue;
                if (!target.Equals(renderer.name) && !cloneTarget.Equals(renderer.name)) continue;
                if (targetIndex < 0)
                {
                    emissiveMaterials[count] = renderer.material;
                }
                else
                {
                    if (count == targetIndex)
                    {
                        emissiveMaterials[0] = renderer.material;
                        break;
                    }
                }
                ++count;
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
