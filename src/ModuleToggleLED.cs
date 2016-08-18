using System;
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
        [KSPField(guiName = "LED Status", isPersistant = true, guiActive = true, guiActiveEditor = true), UI_Toggle(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, enabledText = "On", disabledText = "Off")]
        public bool status = false;
        private BaseField StatusField { get { return Fields["status"]; } }

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

        private IColorSource inputActive = null;
        private IColorSource inputInactive = null;


        /// <summary>
        /// Action-group method for toggling blinkenlight.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Toggle Light")]
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

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            StatusField.uiControlEditor.onFieldChanged = OnEditorToggleChanged;

            inputActive = FindColorSource(activeColor);
            inputInactive = FindColorSource(inactiveColor);

            SetInputUIState();
        }


        public override Color OutputColor
        {
            get
            {
                IColorSource source = status ? inputActive : inputInactive;
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
