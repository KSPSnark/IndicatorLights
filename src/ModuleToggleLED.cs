using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// This controller makes the emissive act as a simple on/off LED "lamp".
    /// 
    /// The user can toggle the lamp on/off, either in the editor or in flight. Toggling,
    /// activating, and deactivating are also all available via action groups, so (for example)
    /// it's possible to add the lamp to the Lights group if desired.
    /// 
    /// By default, it's lit bright green when "on", totally extinguished when "off". Both the
    /// "on" and the "off" colors can be arbitrarily adjusted via sliders in the editor.
    /// </summary>
    public class ModuleToggleLED : ModuleEmissiveController, IToggle
    {
        private const float MIN_AXIS = 0f;
        private const float MAX_AXIS = 1f;
        private const float MID_AXIS = 0.5f * (MIN_AXIS + MAX_AXIS);
        private const float AXIS_THRESHOLD = 1.01f * MID_AXIS;

        /// <summary>
        /// Indicates whether the light is on or off.
        /// </summary>
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true), UI_Toggle(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, enabledText = "On", disabledText = "Off")]
        public bool status = false;
        private BaseField StatusField { get { return Fields["status"]; } }

        /// <summary>
        /// The text to display in the UI for the status field.
        /// </summary>
        [KSPField]
        public string statusText = "LED Status";

        /// <summary>
        /// This specifies the color of the toggle when it's in the "on" state. This might be a literal
        /// color string, or it might be the controllerName of another controller on the part.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string activeColor = Colors.ToString(DefaultColor.ToggleLED);

        /// <summary>
        /// This specifies the color of the toggle when it's in the "off" state. This might be a literal
        /// color string, or it might be the controllerName of another controller on the part.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string inactiveColor = Colors.ToString(DefaultColor.Off);

        /// <summary>
        /// Brightness of the emitted color, on a scale from 0 to 1.
        /// </summary>
        [KSPAxisField(
            guiName = "Activation",
            guiActive = false,
            guiActiveEditor = false,
            axisMode = KSPAxisMode.Absolute,
            minValue = MIN_AXIS,
            maxValue = MAX_AXIS,
            incrementalSpeed = 1f)]
        public float axisGroup = MID_AXIS;
        private BaseAxisField axisGroupField = null;

        private IColorSource inputActive = null;
        private IColorSource inputInactive = null;


        /// <summary>
        /// Action-group method for toggling blinkenlight.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Toggle Light", KSPActionGroup.REPLACEWITHDEFAULT)]
        public void ToggleAction(KSPActionParam actionParam)
        {
            status = actionParam.type != KSPActionType.Deactivate;
        }

        /// <summary>
        /// Action-group method for turning blinkenlight on.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Turn Light On")]
        public void ActivateAction(KSPActionParam actionParam)
        {
            status = true;
        }

        /// <summary>
        /// Action-group method for turning blinkenlight off.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Turn Light Off")]
        public void DeactivateAction(KSPActionParam actionParam)
        {
            status = false;
        }

        [KSPField]
        public KSPActionGroup defaultActionGroup;

        public override void OnAwake()
        {
            base.OnAwake();
            BaseAction toggleAction = base.Actions["ToggleAction"];
            if (toggleAction.actionGroup == KSPActionGroup.REPLACEWITHDEFAULT)
            {
                toggleAction.actionGroup = this.defaultActionGroup;
            }
            if (toggleAction.defaultActionGroup == KSPActionGroup.REPLACEWITHDEFAULT)
            {
                toggleAction.defaultActionGroup = this.defaultActionGroup;
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            axisGroupField = (BaseAxisField)Fields["axisGroup"];

            StatusField.uiControlEditor.onFieldChanged = OnEditorToggleChanged;
            StatusField.guiName = statusText;

            SetInputUIState();
        }

        public override void ParseIDs()
        {
            base.ParseIDs();
            inputActive = FindColorSource(activeColor);
            inputInactive = FindColorSource(inactiveColor);
        }


        public override Color OutputColor
        {
            get
            {
                bool effectiveStatus = status && ((axisGroupField.axisGroup == KSPAxisGroup.None) || (axisGroup > AXIS_THRESHOLD));
                
                IColorSource source = effectiveStatus ? inputActive : inputInactive;
                return source.HasColor ? source.OutputColor : Color.black;
            }
        }

        /// <summary>
        /// IToggle implementation.
        /// </summary>
        public bool ToggleStatus
        {
            get { return status; }
        }

        /// <summary>
        /// Updates the state of the controller.
        /// </summary>
        private void SetInputUIState()
        {
            ModuleEmissiveController activeController = inputActive as ModuleEmissiveController;
            if (activeController != null) activeController.SetUiEnabled(status);

            ModuleEmissiveController inactiveController = inputInactive as ModuleEmissiveController;
            if (inactiveController != null) inactiveController.SetUiEnabled(!status);
        }

        /// <summary>
        /// Here when the toggle changes while we're in the editor.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        private void OnEditorToggleChanged(BaseField field, object value)
        {
            // By design, the editor toggles symmetry groups on/off together.
            foreach (Part counterpart in part.symmetryCounterparts)
            {
                for (int i = 0; i < counterpart.Modules.Count; ++i)
                {
                    ModuleToggleLED controller = counterpart.Modules[i] as ModuleToggleLED;
                    if (controller != null) controller.SetInputUIState();
                }
            }
            SetInputUIState();
        }
    }
}
