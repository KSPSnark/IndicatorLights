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
        private MeshRenderer[] renderers = null;
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
        /// Sets the emissive color of all controlled meshes on the module.
        /// </summary>
        public Color Color
        {
            set
            {
                if (isValid)
                {
                    materialPropertyBlock.SetColor(EMISSIVE_COLOR_ID, value);
                    bool needCleanup = false;
                    for (int i = 0; i < renderers.Length; ++i)
                    {
                        if (renderers[i] == null)
                        {
                            // This can happen if a breakable part breaks, and there was an emissive mesh
                            // on the broken part; suddenly we end up with a null renderer in our array.
                            needCleanup = true;
                        }
                        else
                        {
                            renderers[i].SetPropertyBlock(materialPropertyBlock);
                        }
                    }
                    if (needCleanup) CleanupRenderers();
                }
            }
        }

        /// <summary>
        /// Sets the emissive color of the specified controlled mesh on the module.
        /// This is typically called only from "array" style controllers that don't
        /// use the Color property to set all meshes at once.
        /// </summary>
        /// <param name="index">The index of the mesh whose color to set. Must be between 0 and Count-1.</param>
        public void SetColorAt(Color color, int index)
        {
            if ((index < 0) || (index >= renderers.Length)) return;
            materialPropertyBlock.SetColor(EMISSIVE_COLOR_ID, color);
            renderers[index].SetPropertyBlock(materialPropertyBlock);
        }

        /// <summary>
        /// Gets the number of meshes that this module controls. This is only available
        /// after Start() time, and will return -1 if not yet initialized.
        /// </summary>
        public int Count
        {
            get { return (renderers == null) ? -1 : renderers.Length; }
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
            if (string.IsNullOrEmpty(target))
            {
                Logging.Warn("No emissive target identified for " + part.GetTitle());
                return NO_RENDERERS;
            }

            // Get all the meshes on the part (regardless of whether we're doing anything with them or not).
            MeshRenderer[] allRenderers = GetMeshes(part);
            if ((allRenderers == null) || (allRenderers.Length < 1)) return NO_RENDERERS;

            // If they've ended with a colon and a list of indices, use that.
            Match match = TARGET_PATTERN.Match(target);
            List<int> targetIndices = null;
            if (match.Success)
            {
                target = match.Groups[1].Value;
                targetIndices = ParseIndices(match.Groups[2].Value);
            }

            // Filter our list of meshes down to just the ones that match our target.
            MeshRenderer[] candidates = FilterMeshes(allRenderers, target);
            if (candidates.Length < 1)
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

            // Were specific indices specified?
            if ((targetIndices == null) || (targetIndices.Count < 1))
            {
                // No indices specified, just return the full list of candidates.
                return candidates;
            }

            // Okay, a specific list of indices was specified. Assemble our results based on that.
            List<MeshRenderer> results = new List<MeshRenderer>(candidates.Length);
            for (int i = 0; i < targetIndices.Count; ++i)
            {
                int targetIndex = targetIndices[i];
                if ((targetIndex < 0) || (targetIndex >= candidates.Length)) continue;
                results.Add(candidates[targetIndex]);
            }
            return results.ToArray();
        }

        /// <summary>
        /// Get an array of all renderers for the specified part.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        internal static MeshRenderer[] GetMeshes(Part part)
        {
            return part.FindModelComponents<MeshRenderer>().ToArray();
        }

        /// <summary>
        /// Go through the list of renderers and remove any that are null. This is called
        /// whenever we find a null renderer, which can happen when a breakable part breaks
        /// and an emissive mesh goes poof.
        /// </summary>
        private void CleanupRenderers()
        {
            int nullCount = 0;
            for (int i = 0; i < renderers.Length; ++i)
            {
                if (renderers[i] == null) ++nullCount;
            }
            if (nullCount < 1) return; // nope, no nulls found
            if (nullCount == renderers.Length) // they're ALL null!
            {
                renderers = NO_RENDERERS;
                isValid = false;
                return;
            }
            // Some but not all are null.  Need to compact.
            MeshRenderer[] newRenderers = new MeshRenderer[renderers.Length - nullCount];
            int newIndex = 0;
            for (int oldIndex = 0; oldIndex < renderers.Length; ++oldIndex)
            {
                if (renderers[oldIndex] == null) continue;
                newRenderers[newIndex++] = renderers[oldIndex];
            }
            renderers = newRenderers;
        }

        /// <summary>
        /// Given a comma-delimited list of integers, get a list of them in order.
        /// </summary>
        /// <param name="listText"></param>
        /// <returns></returns>
        private static List<int> ParseIndices(string listText)
        {
            string[] tokens = ParsedParameters.Tokenize(listText, ',');
            List<int> indices = new List<int>();
            for (int i = 0; i < tokens.Length; ++i)
            {
                try {
                    int index = int.Parse(tokens[i]);
                    if (!indices.Contains(index)) indices.Add(index);
                }
                catch
                {
                    // skip it
                }
            }
            return indices;
        }

        /// <summary>
        /// Given an array of renderers, filter them down to just the ones that match
        /// the target.
        /// </summary>
        /// <param name="renderers"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static MeshRenderer[] FilterMeshes(MeshRenderer[] renderers, string target)
        {
            // If a model is added via ModuleManager config, it looks like it gets "(Clone)" added
            // to the end of the renderer name.
            string cloneTarget = target + CLONE_TAG;

            List<MeshRenderer> candidates = new List<MeshRenderer>();
            for (int rendererIndex = 0; rendererIndex < renderers.Length; ++rendererIndex)
            {
                MeshRenderer renderer = renderers[rendererIndex];
                if (renderer == null) continue;
                if (!target.Equals(renderer.name) && !cloneTarget.Equals(renderer.name)) continue;
                candidates.Add(renderer);
            }
            return (candidates.Count > 0) ? candidates.ToArray() : NO_RENDERERS;
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
