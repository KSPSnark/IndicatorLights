using Expansions.Serenity;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// A module that sets the display based on the state of a robotic controller. The toggle
    /// implementation returns true if the sequence is playing.  The scalar implementation returns
    /// a number in the range [0, 1] indicating the current sequence position while playing.
    /// </summary>
    class ModuleRoboticControllerIndicator : ModuleSourceIndicator<ModuleRoboticController>, IToggle, IScalar
    {
        public override Color OutputColor => CurrentSource.OutputColor;
        public bool ToggleStatus => (SourceModule == null) ? false :  SourceModule.SequenceIsPlaying;
        public double ScalarValue => (SourceModule == null) ? 0 : (SourceModule.SequencePosition / SourceModule.SequenceLength);

        private static readonly IColorSource DEFAULT_PLAYING_FORWARD_SOURCE = ColorSources.Blink(
            ColorSources.Constant(DefaultColor.ToggleLED), 850,
            ColorSources.Constant(DefaultColor.Off), 150);

        private static readonly IColorSource DEFAULT_PLAYING_BACKWARD_SOURCE = ColorSources.Blink(
            ColorSources.Constant(DefaultColor.Warning), 850,
            ColorSources.Constant(DefaultColor.Off), 150);

        /// <summary>
        /// The color to display when the controller is playing forward.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string forwardColor = DEFAULT_PLAYING_FORWARD_SOURCE.ColorSourceID;

        /// <summary>
        /// The color to display when the controller is playing backward.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string backwardColor = DEFAULT_PLAYING_BACKWARD_SOURCE.ColorSourceID;

        /// <summary>
        /// The color to display when the controller is inactive (i.e. not playing).
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string inactiveColor = Colors.ToString(DefaultColor.Off);

        private IColorSource forwardSource;
        private IColorSource backwardSource;
        private IColorSource inactiveSource;

        public override bool HasColor
        {
            get
            {
                return base.HasColor && CurrentSource.HasColor;
            }
        }

        public override void ParseIDs()
        {
            base.ParseIDs();
            forwardSource = FindColorSource(forwardColor);
            backwardSource = FindColorSource(backwardColor);
            inactiveSource = FindColorSource(inactiveColor);
        }

        /// <summary>
        /// Gets the currently displayed source.
        /// </summary>
        private IColorSource CurrentSource
        {
            get
            {
                if (SourceModule == null) return ColorSources.BLACK;
                if (SourceModule.SequenceIsPlaying)
                {
                    switch (SourceModule.SequenceDirection)
                    {
                        case ModuleRoboticController.SequenceDirectionOptions.Forward:
                            return forwardSource;
                        case ModuleRoboticController.SequenceDirectionOptions.Reverse:
                            return backwardSource;
                        default:
                            return ColorSources.ERROR;
                    }
                }
                else
                {
                    return inactiveSource;
                }
            }
        }
    }
}