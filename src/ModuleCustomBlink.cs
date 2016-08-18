using System;
using UnityEngine;

namespace IndicatorLights
{
    class ModuleCustomBlink : ModuleEmissiveController, IToggle
    {
        [KSPField(guiName = "Mode", isPersistant = true, guiActive = true, guiActiveEditor = true), UI_Toggle(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, enabledText = "Blinking", disabledText = "Continuous")]
        public bool blinkEnabled = false;
        private BaseField BlinkEnabledField { get { return Fields["blinkEnabled"]; } }

        [KSPField(guiName = "Milliseconds On", isPersistant = true), UI_FloatRange(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, minValue = 50, maxValue = 1000, stepIncrement = 50)]
        public float onMillis = 500;
        private BaseField OnMillisField { get { return Fields["onMillis"]; } }

        [KSPField(guiName = "Milliseconds Off", isPersistant = true), UI_FloatRange(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, minValue = 50, maxValue = 1000, stepIncrement = 50)]
        public float offMillis = 500;
        private BaseField OffMillisField { get { return Fields["offMillis"]; } }

        [KSPField(guiName = "Phase", isPersistant = true), UI_FloatRange(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, minValue = 0, maxValue = 1, stepIncrement = 0.01F)]
        public float phase = 0;
        private BaseField PhaseField {  get { return Fields["phase"]; } }

        /// <summary>
        /// This specifies the color of the toggle when it's in the "on" state. This might be a literal
        /// color string, or it might be the controllerName of another controller on the part.
        /// </summary>
        [KSPField]
        public string onColor = null;

        /// <summary>
        /// This specifies the color of the toggle when it's in the "off" state. This might be a literal
        /// color string, or it might be the controllerName of another controller on the part.
        /// </summary>
        [KSPField]
        public string offColor = null;

        private IColorSource sourceOn = null;
        private IColorSource sourceOff = null;
        private Animations.Blink blink = null;

        /// <summary>
        /// Action-group method for toggling blinkenlight.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Toggle Blinking")]
        public void ToggleAction(KSPActionParam actionParam)
        {
            blinkEnabled = actionParam.type != KSPActionType.Deactivate;
        }

        /// <summary>
        /// Action-group method for turning blinkenlight on.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Start Blinking")]
        public void ActivateAction(KSPActionParam actionParam)
        {
            blinkEnabled = true;
        }

        /// <summary>
        /// Action-group method for turning blinkenlight off.
        /// </summary>
        /// <param name="actionParam"></param>
        [KSPAction("Stop Blinking")]
        public void DeactivateAction(KSPActionParam actionParam)
        {
            blinkEnabled = false;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            sourceOn = FindColorSource(onColor);
            sourceOff = FindColorSource(offColor);
            blink = Animations.Blink.of((long)onMillis, (long)offMillis, phase);

            BlinkEnabledField.uiControlEditor.onFieldChanged = OnBlinkEnabledChanged;
            OnMillisField.uiControlEditor.onFieldChanged = OnMillisChanged;
            OffMillisField.uiControlEditor.onFieldChanged = OnMillisChanged;
            PhaseField.uiControlEditor.onFieldChanged = OnMillisChanged;

            SetUiState();
        }

        public override bool HasColor
        {
            get
            {
                return CurrentSource.HasColor;
            }
        }

        public override Color OutputColor
        {
            get
            {
                return CurrentSource.OutputColor;
            }
        }

        protected override void OnUiEnabled(bool enabled)
        {
            base.OnUiEnabled(enabled);

            BlinkEnabledField.guiActiveEditor = enabled;
            OnMillisField.guiActiveEditor = enabled && blinkEnabled;
            OffMillisField.guiActiveEditor = enabled && blinkEnabled;
            PhaseField.guiActiveEditor = enabled && blinkEnabled;

            ModuleEmissiveController onController = sourceOn as ModuleEmissiveController;
            if (onController != null) onController.SetUiEnabled(enabled);

            ModuleEmissiveController offController = sourceOff as ModuleEmissiveController;
            if (offController != null) offController.SetUiEnabled(enabled);
        }

        private IColorSource CurrentSource
        {
            get
            {
                return (!blinkEnabled || blink.State) ? sourceOn : sourceOff;
            }
        }

        /// <summary>
        /// IToggle implementation.
        /// </summary>
        public bool ToggleStatus
        {
            get
            {
                return blinkEnabled;
            }
        }

        /// <summary>
        /// Here when the "blink enabled" field is changed in the editor.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        private void OnBlinkEnabledChanged(BaseField field, object value)
        {
            SetUiState();
            foreach (Part counterpart in part.symmetryCounterparts)
            {
                for (int i = 0; i < counterpart.Modules.Count; ++i)
                {
                    ModuleCustomBlink controller = counterpart.Modules[i] as ModuleCustomBlink;
                    if (controller != null) controller.SetUiState();
                }
            }
        }

        /// <summary>
        /// Here when one of the period sliders has changed its value in the editor.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        private void OnMillisChanged(BaseField field, object value)
        {
            UpdateBlinkPeriods();
            foreach (Part counterpart in part.symmetryCounterparts)
            {
                for (int i = 0; i < counterpart.Modules.Count; ++i)
                {
                    ModuleCustomBlink controller = counterpart.Modules[i] as ModuleCustomBlink;
                    if (controller != null) controller.UpdateBlinkPeriods();
                }
            }
        }

        private void SetUiState()
        {
            OnMillisField.guiActiveEditor = isUiEnabled && blinkEnabled;
            OffMillisField.guiActiveEditor = isUiEnabled && blinkEnabled;
            PhaseField.guiActiveEditor = isUiEnabled && blinkEnabled;
        }

        private void UpdateBlinkPeriods()
        {
            blink.OnMillis = (long)onMillis;
            blink.OffMillis = (long)offMillis;
            blink.Phase = phase;
        }
    }
}
