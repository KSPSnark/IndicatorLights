using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// A bi-state indicator whose output is controlled by other toggle controllers.
    /// </summary>
    class ModuleBooleanIndicator : ModuleEmissiveController, IToggle
    {
        /// <summary>
        /// Determines how to combine input values to get an output value.
        /// </summary>
        [KSPField]
        public Operation operation = Operation.or;

        /// <summary>
        /// Required. A comma-delimited list of input toggle modules that are used to
        /// determine the toggle value of this module.
        /// </summary>
        [KSPField]
        public string inputs = string.Empty;

        /// <summary>
        /// Color used when in the "on" state.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string activeColor = Colors.ToString(DefaultColor.ToggleLED);

        /// <summary>
        /// Color used when in the "off" state.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string inactiveColor = Colors.ToString(DefaultColor.Off);

        /// <summary>
        /// Used for deciding how to display a value when there are multiple inputs.
        /// </summary>
        public enum Operation
        {
            /// <summary>
            /// Use the boolean AND of the inputs.
            /// </summary>
            and,

            /// <summary>
            /// Use the boolean OR of the inputs.
            /// </summary>
            or
        }

        bool isValid = false;
        private IToggle[] inputToggles = null;
        private IColorSource activeSource = null;
        private IColorSource inactiveSource = null;

        /// <summary>
        /// Here when we're starting up.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            string[] inputTokens = ParsedParameters.Tokenize(inputs, ',');
            List<IToggle> inputList = new List<IToggle>();
            for (int i = 0; i < inputTokens.Length; ++i)
            {
                inputList.AddRange(FindToggles(part, inputTokens[i]));
            }
            if (inputList.Count == 0)
            {
                isValid = false;
                Logging.Warn("ModuleBooleanIndicator (" + Logging.GetTitle(part) + "): No inputs found");
                return;
            }
            isValid = true;
            inputToggles = inputList.ToArray();

            activeSource = FindColorSource(activeColor);
            inactiveSource = FindColorSource(inactiveColor);
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

        /// <summary>
        /// IToggle implementation.
        /// </summary>
        public bool ToggleStatus
        {
            get
            {
                if (isValid)
                {
                    bool status = inputToggles[0].ToggleStatus;
                    switch (operation)
                    {
                        case Operation.and:
                            for (int i = 1; i < inputToggles.Length; ++i)
                            {
                                status &= inputToggles[i].ToggleStatus;
                            }
                            return status;
                        case Operation.or:
                            for (int i = 1; i < inputToggles.Length; ++i)
                            {
                                status |= inputToggles[i].ToggleStatus;
                            }
                            return status;
                    }
                }
                return false;
            }
        }

        public override string DebugDescription
        {
            get
            {
                if (!isValid) return "Invalid state. No inputs found for: \"" + inputs + "\"";
                StringBuilder builder = new StringBuilder().AppendFormat(
                    "Status: {0} of {1} inputs is {2}: ",
                    operation.ToString().ToUpper(),
                    inputToggles.Length,
                    ToggleStatus);
                for (int i = 0; i < inputToggles.Length; ++i)
                {
                    if (i > 0) builder.Append(", ");
                    builder.AppendFormat(
                        "{0} ({1})",
                        Logging.GetIdentifier(inputToggles[i]),
                        inputToggles[i].ToggleStatus);
                }
                return builder.ToString();
            }
        }

        private IColorSource CurrentSource
        {
            get
            {
                if (!isValid) return ColorSources.ERROR;
                return ToggleStatus ? activeSource : inactiveSource;
            }
        }

        /// <summary>
        /// Find all toggles matching the specified identifier.  If the identifier starts
        /// with a "!", invert them.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        private static List<IToggle> FindToggles(Part part, string identifier)
        {
            bool isInverted = false;
            if (identifier.StartsWith("!"))
            {
                isInverted = true;
                identifier = identifier.Substring(1).Trim();
            }
            List<IToggle> toggles = Identifiers.FindAll<IToggle>(part, identifier);
            if (isInverted)
            {
                for (int i = 0; i < toggles.Count; ++i)
                {
                    toggles[i] = new Inverter(toggles[i]);
                }
            }
            return toggles;
        }

        #region Inverter
        /// <summary>
        /// IToggle implementation that returns the logical NOT of another toggle.
        /// </summary>
        private class Inverter : IToggle
        {
            private readonly IToggle source;

            public Inverter(IToggle source)
            {
                this.source = source;
            }

            public bool ToggleStatus
            {
                get { return !source.ToggleStatus; }
            }
        }
        #endregion
    }
}
