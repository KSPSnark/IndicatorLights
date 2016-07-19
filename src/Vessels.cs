namespace IndicatorLights
{
    static class Vessels
    {
        /// <summary>
        /// Get the current biome of the ship. Returns null if none.
        /// </summary>
        /// <param name="vessel"></param>
        /// <returns></returns>
        public static string GetCurrentBiome(this Vessel vessel)
        {
            if (vessel == null) return null;
            double lat = ResourceUtilities.Deg2Rad((vessel.latitude + 180.0 + 90.0) % 180.0 - 90.0);
            double lon = ResourceUtilities.Deg2Rad((vessel.longitude + 360.0 + 180.0) % 360.0 - 180.0);
            CBAttributeMapSO.MapAttribute biome = ResourceUtilities.GetBiome(lat, lon, vessel.mainBody);
            return (biome == null) ? null : biome.name;
        }
    }
}
