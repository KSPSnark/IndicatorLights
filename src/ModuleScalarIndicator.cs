using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace IndicatorLights
{
    class ModuleScalarIndicator : ModuleEmissiveController, IScalar
    {
        /// <summary>
        /// Determines how to combine input values to get an output value.
        /// </summary>
        [KSPField]
        public Operation operation = Operation.minimum;

        /// <summary>
        /// Required. A comma-delimited list of input scalar modules that are used to
        /// determine the scalar value of this module.
        /// </summary>
        [KSPField]
        public string inputs = string.Empty;

        /// <summary>
        /// The color to display when the input scalar is high.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string highColor = Colors.ToString(DefaultColor.HighResource);

        /// <summary>
        /// The color to display when the input scalar is medium.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string mediumColor = Colors.ToString(DefaultColor.MediumResource);

        /// <summary>
        /// The color to display when the input scalar is low.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string lowColor = Colors.ToString(DefaultColor.LowResource);

        /// <summary>
        /// The color to display when the input scalar is very close to zero.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string criticalColor = "lowColor";

        /// <summary>
        /// The color to display when the input scalar is zero.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string absentColor = Colors.ToString(DefaultColor.Off);

        /// <summary>
        /// If input scalar is above this value, display using the "high" color.
        /// </summary>
        [KSPField]
        public double highThreshold = 0.7;

        /// <summary>
        /// If input scalar is below this value, display using the "low" color.
        /// </summary>
        [KSPField]
        public double lowThreshold = 0.3;

        /// <summary>
        /// If input scalar is below this value, display using the "critical" color.
        /// </summary>
        [KSPField]
        public double criticalThreshold = 0.03;

        /// <summary>
        /// Defines "effective zero". Values at or below this value will display using the "absent" color.
        /// </summary>
        [KSPField]
        public double epsilon = 0.0;

        /// <summary>
        /// Used for deciding how to display a value when there are multiple inputs.
        /// </summary>
        public enum Operation
        {
            /// <summary>
            /// Use the input with the highest value.
            /// </summary>
            maximum,

            /// <summary>
            /// Use the average of the inputs.
            /// </summary>
            average,

            /// <summary>
            /// Use the input with the lowest value.
            /// </summary>
            minimum
        }

        private bool isValid = false;
        private IScalar[] inputScalars = null;
        private IColorSource highSource = null;
        private IColorSource mediumSource = null;
        private IColorSource lowSource = null;
        private IColorSource criticalSource = null;
        private IColorSource absentSource = null;

        /// <summary>
        /// Called when the module is starting up.
        /// </summary>
        /// <param name="state"></param>
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            string[] inputTokens = ParsedParameters.Tokenize(inputs, ',');
            List<IScalar> inputList = new List<IScalar>();
            for (int i = 0; i < inputTokens.Length; ++i)
            {
                inputList.AddRange(Identifiers.FindAll<IScalar>(part, inputTokens[i]));
            }
            if (inputList.Count == 0)
            {
                isValid = false;
                Logging.Warn("ModuleScalarIndicator (" + Logging.GetTitle(part) + "): No inputs found");
                return;
            }
            isValid = true;
            inputScalars = inputList.ToArray();

            highSource = FindColorSource(highColor);
            mediumSource = FindColorSource(mediumColor);
            lowSource = FindColorSource(lowColor);
            criticalSource = FindColorSource(criticalColor);
            absentSource = FindColorSource(absentColor);
        }

        public override bool HasColor
        {
            get
            {
                return base.HasColor && CurrentSource.HasColor;
            }
        }

        public override Color OutputColor
        {
            get { return CurrentSource.OutputColor; }
        }

        public double ScalarValue
        {
            get
            {
                if (isValid)
                {
                    switch (operation)
                    {
                        case Operation.maximum:
                            double max = double.NegativeInfinity;
                            for (int i = 0; i < inputScalars.Length; ++i)
                            {
                                double value = inputScalars[i].ScalarValue;
                                if (value > max) max = value;
                            }
                            return max;
                        case Operation.minimum:
                            double min = double.PositiveInfinity;
                            for (int i = 0; i < inputScalars.Length; ++i)
                            {
                                double value = inputScalars[i].ScalarValue;
                                if (value < min) min = value;
                            }
                            return min;
                        case Operation.average:
                            double sum = 0.0;
                            for (int i = 0; i < inputScalars.Length; ++i)
                            {
                                sum += inputScalars[i].ScalarValue;
                            }
                            return sum / (double)inputScalars.Length;
                    }
                }

                return 0.0;
            }
        }

        public override string DebugDescription
        {
            get
            {
                if (!isValid) return "Invalid state. No inputs found for: \"" + inputs + "\"";
                StringBuilder builder = new StringBuilder().AppendFormat(
                    "Level: {0} of {1} inputs is {2}: ",
                    operation.ToString(),
                    inputScalars.Length,
                    ScalarValue);
                for (int i = 0; i < inputScalars.Length; ++i)
                {
                    if (i > 0) builder.Append(", ");
                    builder.AppendFormat(
                        "{0} ({1})",
                        Logging.GetIdentifier(inputScalars[i]),
                        inputScalars[i].ScalarValue);
                }
                return builder.ToString();
            }
        }

        private IColorSource CurrentSource
        {
            get
            {
                if (!isValid) return ColorSources.ERROR;
                double value = ScalarValue;
                if (value <= epsilon) return absentSource;
                if (value <= criticalThreshold) return criticalSource;
                if (value <= lowThreshold) return lowSource;
                if (value >= highThreshold) return highSource;
                return mediumSource;
            }
        }
    }
}
