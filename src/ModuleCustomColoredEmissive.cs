using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// A simple controller that just emits one color. UI sliders allow customizing the color
    /// in the vehicle editor.
    /// </summary>
    class ModuleCustomColoredEmissive : ModuleEmissiveController
    {
        private static readonly Color DEFAULT_COLOR = Color.black;
        private Color currentColor = DEFAULT_COLOR;
        private StartState startState = StartState.None;

        [KSPField]
        public string label = string.Empty;

        [KSPField(isPersistant = true)]
        public string color = Colors.ToString(DEFAULT_COLOR);

        [KSPField(guiName = "Red", isPersistant = true), UI_FloatRange(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, minValue = 0, maxValue = 1, stepIncrement = 0.01f)]
        public float red = DEFAULT_COLOR.r;
        private BaseField RedField { get { return Fields["red"]; } }

        [KSPField(guiName = "Green", isPersistant = true), UI_FloatRange(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, minValue = 0, maxValue = 1, stepIncrement = 0.01f)]
        public float green = DEFAULT_COLOR.g;
        private BaseField GreenField { get { return Fields["green"]; } }

        [KSPField(guiName = "Blue", isPersistant = true), UI_FloatRange(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, minValue = 0, maxValue = 1, stepIncrement = 0.01f)]
        public float blue = DEFAULT_COLOR.b;
        private BaseField BlueField { get { return Fields["blue"]; } }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            startState = state;
            if (!string.IsNullOrEmpty(label))
            {
                RedField.guiName = label + ": Red";
                GreenField.guiName = label + ": Green";
                BlueField.guiName = label + ": Blue";
            }

            if (!string.IsNullOrEmpty(color))
            {
                currentColor = Colors.Parse(color, DEFAULT_COLOR);
                red = roundHundredths(currentColor.r);
                green = roundHundredths(currentColor.g);
                blue = roundHundredths(currentColor.b);
            }

            RedField.uiControlEditor.onFieldChanged = OnColorSliderChanged;
            GreenField.uiControlEditor.onFieldChanged = OnColorSliderChanged;
            BlueField.uiControlEditor.onFieldChanged = OnColorSliderChanged;
        }

        protected override void OnUiEnabled(bool enabled)
        {
            base.OnUiEnabled(enabled);
            RedField.guiActiveEditor = enabled;
            GreenField.guiActiveEditor = enabled;
            BlueField.guiActiveEditor = enabled;
        }

        public override Color OutputColor
        {
            get { return currentColor; }
        }

        /// <summary>
        /// Here whenever a color slider changes.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        private void OnColorSliderChanged(BaseField field, object oldValue)
        {
            // Adjust everything in the symmetry group, since this can only happen in the editor.
            SetSymmetricState();
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
                    ModuleCustomColoredEmissive controller = counterpart.Modules[i] as ModuleCustomColoredEmissive;
                    if ((controller != null) && object.Equals(ColorSourceID, controller.ColorSourceID))
                    {
                        controller.red = red;
                        controller.green = green;
                        controller.blue = blue;
                        controller.SetCurrentColor();
                    }
                }
            }
            SetCurrentColor();
        }

        /// <summary>
        /// Updates the state of the controller.
        /// </summary>
        private void SetCurrentColor()
        {
            currentColor.r = red;
            currentColor.g = green;
            currentColor.b = blue;
            color = Colors.ToString(currentColor);
        }

        private static float roundHundredths(float value)
        {
            return Mathf.Floor(value * 100f + 0.5f) * 0.01f;
        }
    }
}
