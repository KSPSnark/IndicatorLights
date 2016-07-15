using System;
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
    public class ModuleControllableEmissive : PartModule, Identifiers.IIdentifiable
    {
        private static readonly Regex TARGET_PATTERN = new Regex("^(.+):([\\d,]+)$");
        private static readonly int emissiveColorId = Shader.PropertyToID("_EmissiveColor");
        private static readonly Material[] NO_MATERIALS = new Material[0];

        private Material[] materials;
        private Color cachedColor = new Color(0, 0, 0, 0);
        private Guid lastActiveVessel = Guid.Empty;

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
                if (ActiveVesselChanged || !cachedColor.Equals(value))
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
        /// Searches the part for all emissive materials that match the specified target for this module.
        /// Special case is when the target ends with a colon and a number, in which case it only tries
        /// to get the Nth such instance.
        /// </summary>
        /// <returns></returns>
        private Material[] GetEmissiveMaterials()
        {
            if (part == null) return NO_MATERIALS;
            if (part.transform == null) return NO_MATERIALS;
            if (string.IsNullOrEmpty(target))
            {
                Logging.Warn("No emissive target identified for " + part.GetTitle());
                return NO_MATERIALS;
            }
            MeshRenderer[] renderers = part.transform.GetComponentsInChildren<MeshRenderer>();
            if (renderers == null) return NO_MATERIALS;

            // If they've ended with a colon and a list of indices, use that.
            Match match = TARGET_PATTERN.Match(target);
            HashSet<int> targetIndices = null;
            if (match.Success)
            {
                target = match.Groups[1].Value;
                targetIndices = ParseIndices(match.Groups[2].Value);
            }
            if (targetIndices == null) targetIndices = new HashSet<int>();

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
            PruneIndices(targetIndices, count);
            if (targetIndices.Count == 0)
            {
                // just include them all
                for (int i = 0; i < count; ++i) targetIndices.Add(i);
            }
            Material[] emissiveMaterials = new Material[targetIndices.Count];
            int matchingRendererIndex = 0;
            int emissiveMaterialIndex = 0;
            for (int rendererIndex = 0; rendererIndex < renderers.Length; ++rendererIndex)
            {
                Renderer renderer = renderers[rendererIndex];
                if (renderer == null) continue;
                if (!target.Equals(renderer.name) && !cloneTarget.Equals(renderer.name)) continue;
                if (targetIndices.Contains(matchingRendererIndex))
                {
                    emissiveMaterials[emissiveMaterialIndex++] = renderer.material;
                }
                ++matchingRendererIndex;
            }

            return emissiveMaterials;
        }

        private static HashSet<int> ParseIndices(string listText)
        {
            string[] tokens = listText.Split(',');
            HashSet<int> indices = new HashSet<int>();
            for (int i = 0; i < tokens.Length; ++i)
            {
                try {
                    indices.Add(int.Parse(tokens[i]));
                }
                catch
                {
                    // skip it
                }
            }
            return indices;
        }

        private static void PruneIndices(HashSet<int> indices, int limit)
        {
            List<int> overflows = new List<int>();
            foreach (int index in indices)
            {
                if (index >= limit) overflows.Add(index);
            }
            foreach (int overflow in overflows)
            {
                indices.Remove(overflow);
            }
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

        public string Identifier
        {
            get
            {
                return emissiveName;
            }
        }

        private bool ActiveVesselChanged
        {
            get
            {
                Vessel activeVessel = FlightGlobals.ActiveVessel;
                if (activeVessel == null) return true;
                if (activeVessel.id == lastActiveVessel) return false;
                lastActiveVessel = activeVessel.id;
                return true;
            }
        }
    }
}
