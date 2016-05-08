namespace IndicatorLights
{
    /// <summary>
    /// These are the logical identifiers of default colors that the modules in this
    /// assembly will use if not told otherwise via config.
    /// 
    /// Each of these has a corresponding setting in Configuration to supply a value.
    /// </summary>
    enum DefaultColor
    {
        TOGGLE_LED,
        HIGH_RESOURCE,
        MEDIUM_RESOURCE,
        LOW_RESOURCE,
        REACTION_WHEEL_PROBLEM,
        REACTION_WHEEL_NORMAL,
        REACTION_WHEEL_PILOT_ONLY,
        REACTION_WHEEL_SAS_ONLY,
        RESOURCE_CONVERTER_ACTIVE,
        DOCKING_CROSSFEED_ON,
        DOCKING_CROSSFEED_OFF,
        OFF
    }
}
