namespace IndicatorLights
{
    /// <summary>
    /// A specialized toggle for turning crew indicators on/off.
    /// </summary>
    public class ModuleCrewIndicatorToggle : PartModule, Identifiers.IIdentifiable, IToggle
    {
        [KSPField(guiName = "Crew LEDs", isPersistant = true, guiActive = true, guiActiveEditor = true),
         UI_Toggle(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, enabledText = "On", disabledText = "Off")]
        public bool status = Configuration.crewIndicatorDefaultStatus;

        [KSPField]
        public string toggleName = null;

        /// <summary>
        /// Action-group method for toggling status.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Toggle Crew LEDs")]
        public void OnToggleAction(KSPActionParam actionParam)
        {
            status = actionParam.type != KSPActionType.Deactivate;
        }
        private BaseAction ToggleAction { get { return Actions["OnToggleAction"]; } }

        /// <summary>
        /// Action-group method for setting status to true.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Activate Crew LEDs")]
        public void OnActivateAction(KSPActionParam actionParam)
        {
            status = true;
        }
        private BaseAction ActivateAction { get { return Actions["OnActivateAction"]; } }

        /// <summary>
        /// Action-group method for setting status to false.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Deactivate Crew LEDs")]
        public void OnDeactivateAction(KSPActionParam actionParam)
        {
            status = false;
        }
        private BaseAction DeactivateAction { get { return Actions["OnDeactivateAction"]; } }

        public string Identifier
        {
            get { return toggleName; }
        }

        public bool ToggleStatus
        {
            get { return status; }
        }
    }
}
