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
        internal const string CLONE_TAG = "(Clone)";
        private static readonly Regex TARGET_PATTERN = new Regex("^(.+):([\\d,]+)$");
        private static readonly int EMISSIVE_COLOR_ID = Shader.PropertyToID("_EmissiveColor");
        private static readonly MeshRenderer[] NO_RENDERERS = new MeshRenderer[0];

        private bool isValid = false;
        private MaterialPropertyBlock materialPropertyBlock;
        private MeshRenderer[] renderers;
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
        /// Sets the emissive color of the module.
        /// </summary>
        public Color Color
        {
            set
            {
                if (isValid)
                {
                    materialPropertyBlock.SetColor(EMISSIVE_COLOR_ID, value);
                    for (int i = 0; i < renderers.Length; ++i)
                    {
                        renderers[i].SetPropertyBlock(materialPropertyBlock);
                    }
                }
            }
        }

        public void Start()
        {
            materialPropertyBlock = new MaterialPropertyBlock();
            renderers = GetControlledRenderers();
            isValid = renderers.Length > 0;
        }

        /// <summary>
        /// Searches the part for all emissive materials that match the specified target for this module.
        /// Special case is when the target ends with a colon and a number (or set of comma-delimited
        /// numbers), in which case it only tries to get the instance(s) matching the indices.
        /// </summary>
        /// <returns></returns>
        private MeshRenderer[] GetControlledRenderers()
        {
            if (part == null) return NO_RENDERERS;
            if (part.transform == null) return NO_RENDERERS;
            if (string.IsNullOrEmpty(target))
            {
                Logging.Warn("No emissive target identified for " + part.GetTitle());
                return NO_RENDERERS;
            }
            MeshRenderer[] allRenderers = GetMeshes(part);
            if (allRenderers == null) return NO_RENDERERS;

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
            string cloneTarget = target + CLONE_TAG;

            for (int rendererIndex = 0; rendererIndex < allRenderers.Length; ++rendererIndex)
            {
                Renderer renderer = allRenderers[rendererIndex];
                if (renderer == null) continue;
                if (!target.Equals(renderer.name) && !cloneTarget.Equals(renderer.name)) continue;
                ++count;
            }
            if (count < 1)
            {
                Logging.Warn("No emissive materials named '" + target + "' could be identified for " + part.GetTitle());
                for (int rendererIndex = 0; rendererIndex < allRenderers.Length; ++rendererIndex)
                {
                    string rendererName = allRenderers[rendererIndex].name;
                    if (rendererName.EndsWith(CLONE_TAG)) rendererName = rendererName.Substring(rendererName.Length - CLONE_TAG.Length);
                    Logging.Warn("Did you mean this?  " + rendererName);
                }
                return NO_RENDERERS;
            }
            PruneIndices(targetIndices, count);
            if (targetIndices.Count == 0)
            {
                // just include them all
                for (int i = 0; i < count; ++i) targetIndices.Add(i);
            }
            MeshRenderer[] controlledRenderers = new MeshRenderer[targetIndices.Count];
            int matchingRendererIndex = 0;
            int emissiveMaterialIndex = 0;
            for (int rendererIndex = 0; rendererIndex < allRenderers.Length; ++rendererIndex)
            {
                MeshRenderer renderer = allRenderers[rendererIndex];
                if (renderer == null) continue;
                if (!target.Equals(renderer.name) && !cloneTarget.Equals(renderer.name)) continue;
                if (targetIndices.Contains(matchingRendererIndex))
                {
                    controlledRenderers[emissiveMaterialIndex++] = renderer;
                }
                ++matchingRendererIndex;
            }

            return controlledRenderers;
        }

        internal static MeshRenderer[] GetMeshes(Part part)
        {
            return part.FindModelComponents<MeshRenderer>().ToArray();
        }

        private static HashSet<int> ParseIndices(string listText)
        {
            string[] tokens = ParsedParameters.Tokenize(listText, ',');
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

        public string Identifier
        {
            get
            {
                return emissiveName;
            }
        }
    }
}
