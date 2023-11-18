using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Module that shows when an antenna is transmitting data.
    /// </summary>
    class ModuleDataTransmitterIndicator : ModuleSourceIndicator<ModuleDataTransmitter>, IToggle
    {
        private IColorSource busySource;
        private IColorSource inactiveSource;
        private ModuleDeployableAntenna deployable = null;

        /// <summary>
        /// The data rate of the transmitter, in mits/second.
        /// </summary>
        [StaticField]
        private double dataRate;

        /*
        To put things in perspective, here are the data rates of the stock KSP antennas, as of KSP 1.2:

        RA-2:             2.86
        Communotron 16:   3.33
        Communotron 16-S: 3.33
        DTS-M1:           5.71
        HG-5:             5.71
        RA-15:            5.71
        RA-100:          11.43
        88-88:           20.00
        HG-55:           20.00
        */

        /// <summary>
        /// The color to display when the transmitter is busy.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string busyColor = ColorSources.Random(
            ColorSources.Constant(DefaultColor.ToggleLED),
            ColorSources.Constant(DefaultColor.Off),
            100,
            0.5)
            .ColorSourceID;

        /// <summary>
        /// The color to display when the transmitter is idle.
        /// </summary>
        [KSPField]
        [ColorSourceIDField]
        public string inactiveColor = ColorSources.Constant(DefaultColor.Off).ColorSourceID;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            deployable = part.FindModuleImplementing<ModuleDeployableAntenna>(); // may be null
        }

        public override void ParseIDs()
        {
            base.ParseIDs();
            dataRate = (SourceModule == null) ? 0.0 : SourceModule.DataRate;
            busySource = FindColorSource(busyColor);
            inactiveSource = FindColorSource(inactiveColor);
        }

        public override Color OutputColor
        {
            get
            {
                return (ToggleStatus ? busySource : inactiveSource).OutputColor;
            }
        }

        public bool ToggleStatus
        {
            get
            {
                return ((deployable == null) || (deployable.deployState == ModuleDeployablePart.DeployState.EXTENDED))
                    && (SourceModule != null) && SourceModule.IsBusy();
            }
        }
    }
}
