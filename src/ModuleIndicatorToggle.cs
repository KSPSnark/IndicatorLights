namespace IndicatorLights
{
    /// <summary>
    /// A simple module that doesn't actually do anything, other than "own" a particular toggle state.
    /// </summary>
    public class ModuleIndicatorToggle : PartModule
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
            status = !status;
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

        /// <summary>
        /// Tries to find a ModuleIndicatorToggle with the specified name. If the specified name
        /// is null, will just return the first ModuleIndicatorToggle found. If not found, returns null.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="toggleName"></param>
        /// <returns></returns>
        public static ModuleIndicatorToggle Find(Part part, string toggleName)
        {
            if (part == null) return null;
            bool findFirst = string.IsNullOrEmpty(toggleName);
            for (int i = 0; i < part.Modules.Count; ++i)
            {
                ModuleIndicatorToggle candidate = part.Modules[i] as ModuleIndicatorToggle;
                if (candidate == null) continue;
                if (findFirst) return candidate;
                if (toggleName.Equals(candidate.toggleName)) return candidate;
            }
            return null;
        }
    }
}
