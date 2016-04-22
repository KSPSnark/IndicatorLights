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
    public class ModuleToggleLED : ModuleEmissiveController
    {
        private static readonly Color DEFAULT_ACTIVE_COLOR = Color.green;
        private static readonly Color DEFAULT_INACTIVE_COLOR = Color.black;

        internal override string EditorGuiDescription
        {
            get { return "Switchable Lamp"; }
        }

        [KSPField(guiName = "LED Status", isPersistant = true, guiActive = true, guiActiveEditor = true), UI_Toggle(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, enabledText = "On", disabledText = "Off")]
        public bool status = false;
        private BaseField StatusField { get { return Fields["status"]; } }

        [KSPField(guiName = "On: Red", isPersistant = true), UI_FloatRange(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, minValue = 0, maxValue = 1, stepIncrement = 0.01f)]
        public float redOn = DEFAULT_ACTIVE_COLOR.r;
        private BaseField RedOnField { get { return Fields["redOn"]; } }

        [KSPField(guiName = "On: Green", isPersistant = true), UI_FloatRange(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, minValue = 0, maxValue = 1, stepIncrement = 0.01f)]
        public float greenOn = DEFAULT_ACTIVE_COLOR.g;
        private BaseField GreenOnField { get { return Fields["greenOn"]; } }

        [KSPField(guiName = "On: Blue", isPersistant = true), UI_FloatRange(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, minValue = 0, maxValue = 1, stepIncrement = 0.01f)]
        public float blueOn = DEFAULT_ACTIVE_COLOR.b;
        private BaseField BlueOnField { get { return Fields["blueOn"]; } }

        [KSPField(guiName = "Off: Red", isPersistant = true), UI_FloatRange(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, minValue = 0, maxValue = 1, scene = UI_Scene.Editor, stepIncrement = 0.01f)]
        public float redOff = DEFAULT_INACTIVE_COLOR.r;
        private BaseField RedOffField { get { return Fields["redOff"]; } }

        [KSPField(guiName = "Off: Green", isPersistant = true), UI_FloatRange(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, minValue = 0, maxValue = 1, scene = UI_Scene.Editor, stepIncrement = 0.01f)]
        public float greenOff = DEFAULT_INACTIVE_COLOR.g;
        private BaseField GreenOffField { get { return Fields["greenOff"]; } }

        [KSPField(guiName = "Off: Blue", isPersistant = true), UI_FloatRange(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, minValue = 0, maxValue = 1, scene = UI_Scene.Editor, stepIncrement = 0.01f)]
        public float blueOff = DEFAULT_INACTIVE_COLOR.b;
        private BaseField BlueOffField { get { return Fields["blueOff"]; } }


        /// <summary>
        /// Action-group method for toggling blinkenlight.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Toggle Light")]
        public void ToggleAction(KSPActionParam actionParam)
        {
            status = !status;
            SetState();
        }

        /// <summary>
        /// Action-group method for turning blinkenlight on.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Turn Light On")]
        public void ActivateAction(KSPActionParam actionParam)
        {
            status = true;
            SetState();
        }

        /// <summary>
        /// Action-group method for turning blinkenlight off.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Turn Light Off")]
        public void DeactivateAction(KSPActionParam actionParam)
        {
            status = false;
            SetState();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            RedOnField.uiControlEditor.onFieldChanged = OnColorSliderChanged;
            GreenOnField.uiControlEditor.onFieldChanged = OnColorSliderChanged;
            BlueOnField.uiControlEditor.onFieldChanged = OnColorSliderChanged;
            RedOffField.uiControlEditor.onFieldChanged = OnColorSliderChanged;
            GreenOffField.uiControlEditor.onFieldChanged = OnColorSliderChanged;
            BlueOffField.uiControlEditor.onFieldChanged = OnColorSliderChanged;

            StatusField.uiControlEditor.onFieldChanged = OnEditorToggleChanged;
            StatusField.uiControlFlight.onFieldChanged = OnFlightToggleChanged;

            SetState();
        }

        public override void OnLoad(ConfigNode node)
        {
            // When the part is loaded, initialize the blinkenlight state based on active/inactive
            // state in this module.
            base.OnLoad(node);
            SetState();
        }

        /// <summary>
        /// Updates the state of the controller.
        /// </summary>
        private void SetState()
        {
            RedOnField.guiActiveEditor = status;
            GreenOnField.guiActiveEditor = status;
            BlueOnField.guiActiveEditor = status;
            RedOffField.guiActiveEditor = !status;
            GreenOffField.guiActiveEditor = !status;
            BlueOffField.guiActiveEditor = !status;

            Color = status ? ActiveColor : InactiveColor;
        }

        /// <summary>
        /// Updates the state of the controller, along with any symmetry counterparts.
        /// </summary>
        private void SetSymmetricState()
        {
            foreach (Part counterpart in part.symmetryCounterparts)
            {
                for (int i = 0; i < counterpart.Modules.Count; ++i)
                {
                    ModuleToggleLED controller = counterpart.Modules[i] as ModuleToggleLED;
                    if (controller != null) controller.SetState();
                }
            }
            SetState();
        }

        /// <summary>
        /// Here whenever a color slider changes.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        private void OnColorSliderChanged(BaseField field, object value)
        {
            // Adjust everything in the symmetry group, since this can only happen in the editor.
            SetSymmetricState();
        }

        /// <summary>
        /// Here when the toggle changes while we're in the editor.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        private void OnEditorToggleChanged(BaseField field, object value)
        {
            // By design, the editor toggles symmetry groups on/off together.
            SetSymmetricState();
        }

        /// <summary>
        /// Here when the toggle changes while we're in the flight scene.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        private void OnFlightToggleChanged(BaseField field, object value)
        {
            // In flight, we just toggle the individual part, not the symmetry group.
            SetState();
        }

        /// <summary>
        /// Gets the color in the "on" state.
        /// </summary>
        private Color ActiveColor
        {
            get
            {
                return new Color(redOn, greenOn, blueOn, 1);
            }
        }

        /// <summary>
        /// Gets the color in the "off" state.
        /// </summary>
        private Color InactiveColor
        {
            get
            {
                return new Color(redOff, greenOff, blueOff, 1);
            }
        }
    }
}
