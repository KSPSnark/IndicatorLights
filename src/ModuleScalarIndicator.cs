using System;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// A scalar controller whose output is controlled by other scalars.
    /// </summary>
    class ModuleScalarIndicator : ModuleEmissiveController, IScalar
    {
        /// <summary>
        /// Required. Specifies the input scalar that controls this module. Could be a simple
        /// identifier (such as a module or controller name), or it could be parseable scalar
        /// syntax such as "min(scalar1, scalar2)".
        /// </summary>
        [KSPField]
        [ScalarIDField]
        public string input = string.Empty;

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
        [StaticField]
        public double highThreshold = 0.7;

        /// <summary>
        /// If input scalar is below this value, display using the "low" color.
        /// </summary>
        [KSPField]
        [StaticField]
        public double lowThreshold = 0.3;

        /// <summary>
        /// If input scalar is below this value, display using the "critical" color.
        /// </summary>
        [KSPField]
        [StaticField]
        public double criticalThreshold = 0.03;

        /// <summary>
        /// Defines "effective zero". Values at or below this value will display using the "absent" color.
        /// </summary>
        [KSPField]
        [StaticField]
        public double epsilon = 0.0;

        private bool isValid = false;
        private IScalar inputScalar = null;
        private IColorSource highSource = null;
        private IColorSource mediumSource = null;
        private IColorSource lowSource = null;
        private IColorSource criticalSource = null;
        private IColorSource absentSource = null;

        public override void ParseIDs()
        {
            base.ParseIDs();
            try
            {
                inputScalar = FindScalar(input);
                isValid = true;
            }
            catch (ArgumentException e)
            {
                Logging.Warn("Invalid input for " + Identifier + " on " + part.GetTitle() + ": " + e.Message);
            }

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
            get { return isValid ? inputScalar.ScalarValue : 0.0; }
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
