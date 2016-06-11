using System;
using System.Collections.Generic;

namespace IndicatorLights
{
    /// <summary>
    /// Keeps track of what science is present on a vessel. Does initialization work
    /// at constructor time that scans the vessel once; if the vessel is modified,
    /// construct a new VesselScienceContents.
    /// </summary>
    public class VesselScienceContents
    {
        private static readonly TimeSpan UPDATE_INTERVAL = new TimeSpan(0, 0, 0, 0, 500);

        // Keeps track of all science containers on the vessel.
        private readonly List<IScienceDataContainer> scienceContainers = new List<IScienceDataContainer>();

        // Keeps track of science subjects currently present on the current vessel.
        // Keys are subject IDs.
        private readonly Dictionary<string, CacheItem<bool>> hasScienceStatus = new Dictionary<string, CacheItem<bool>>();

        public VesselScienceContents(Vessel vessel)
        {
            for (int partIndex = 0; partIndex < vessel.parts.Count; ++partIndex)
            {
                Part part = vessel.parts[partIndex];
                for (int moduleIndex = 0; moduleIndex < part.Modules.Count; ++moduleIndex)
                {
                    PartModule module = part.Modules[moduleIndex];
                    IScienceDataContainer container = module as IScienceDataContainer;
                    if (container != null) scienceContainers.Add(container);
                } // for each module on the part
            } // for each part on the vessel
        }

        /// <summary>
        /// Gets whether science with the specified subject ID is present.
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        public bool this[string subjectId]
        {
            get
            {
                CacheItem<bool> status = null;
                if (!hasScienceStatus.TryGetValue(subjectId, out status))
                {
                    status = new CacheItem<bool>(FindScience(subjectId));
                    hasScienceStatus[subjectId] = status;
                }
                else if (DateTime.Now > status.Expiry)
                {
                    status.Update(FindScience(subjectId));
                }
                return status.Item;
            }
        }

        /// <summary>
        /// Search through all science experiments and containers to determine whether
        /// they contain the specified subject. Returns true if any do.
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        private bool FindScience(string subjectId)
        {
            for (int containerIndex = 0; containerIndex < scienceContainers.Count; ++containerIndex)
            {
                IScienceDataContainer container = scienceContainers[containerIndex];
                if (container.GetScienceCount() == 0)
                {
                    continue;
                }
                ScienceData[] data = container.GetData();
                for (int dataIndex = 0; dataIndex < data.Length; ++dataIndex)
                {
                    if (data[dataIndex].subjectID == subjectId) return true;
                }
            }
            return false;
        }

        private class CacheItem<T>
        {
            private T item;
            private DateTime expiry;

            public CacheItem(T item)
            {
                Update(item);
            }

            public T Item
            {
                get { return item; }
            }

            public DateTime Expiry
            {
                get { return expiry; }
            }

            public void Update(T newItem)
            {
                item = newItem;
                expiry = DateTime.Now + UPDATE_INTERVAL;
            }
        }
    }
}
