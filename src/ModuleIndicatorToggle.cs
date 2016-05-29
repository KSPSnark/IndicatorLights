namespace IndicatorLights
{
    /// <summary>
    /// A simple module that doesn't actually do anything, other than "own" a particular toggle state.
    /// </summary>
    public class ModuleIndicatorToggle : PartModule, Identifiers.IIdentifiable, IToggle
    {
        [KSPField(guiName = "Status", isPersistant = true, guiActive = true, guiActiveEditor = true), UI_Toggle(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, enabledText = "On", disabledText = "Off")]
        public bool status = false;
        private BaseField StatusField { get { return Fields["status"]; } }

        [KSPField]
        public bool guiActive = true;

        [KSPField]
        public bool guiActiveEditor = true;

        [KSPField]
        public string toggleName = null;

        [KSPField]
        public string statusLabel = "Status";

        [KSPField]
        public string statusOn = "On";

        [KSPField]
        public string statusOff = "Off";

        [KSPField]
        public string toggleAction = "Toggle";

        [KSPField]
        public string activateAction = "Activate";

        [KSPField]
        public string deactivateAction = "Deactivate";

        /// <summary>
        /// Action-group method for toggling status.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Toggle")]
        public void OnToggleAction(KSPActionParam actionParam)
        {
            status = actionParam.type != KSPActionType.Deactivate;
        }
        private BaseAction ToggleAction {  get { return Actions["OnToggleAction"]; } }

        /// <summary>
        /// Action-group method for setting status to true.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Activate")]
        public void OnActivateAction(KSPActionParam actionParam)
        {
            status = true;
        }
        private BaseAction ActivateAction { get { return Actions["OnActivateAction"]; } }

        /// <summary>
        /// Action-group method for setting status to false.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Deactivate")]
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

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            StatusField.guiActive = guiActive;
            StatusField.guiActiveEditor = guiActiveEditor;
            StatusField.guiName = statusLabel;
            UI_Toggle toggle = StatusField.uiControlEditor as UI_Toggle;
            if (toggle != null)
            {
                toggle.enabledText = statusOn;
                toggle.disabledText = statusOff;
            }
            toggle = StatusField.uiControlFlight as UI_Toggle;
            if (toggle != null)
            {
                toggle.enabledText = statusOn;
                toggle.disabledText = statusOff;
            }
            ToggleAction.guiName = toggleAction;
            ActivateAction.guiName = activateAction;
            DeactivateAction.guiName = deactivateAction;
        }
    }
}
