using System;

namespace IndicatorLights.Console
{
    class LightsEnabledCommand : DebugConsole.DebugConsoleCommand
    {
        private const string COMMAND = "enabled";
        private const string STATUS_ON = "on";
        private const string STATUS_OFF = "off";
        private const string HELP = "Global switch for all lights in the mod";
        private const string USAGE = "{" + STATUS_ON + " | " + STATUS_OFF + "}";

        public LightsEnabledCommand() : base(COMMAND, HELP, USAGE) {}


        public override void Call(string[] arguments)
        {
            if (arguments.Length != 1)
            {
                Logging.Log(HelpCommand.HelpStringOf(this));
                return;
            }

            switch (arguments[0])
            {
                case STATUS_ON:
                    if (GlobalSettings.IsEnabled)
                    {
                        Logging.Log("IndicatorLights is already enabled.");
                    }
                    else
                    {
                        GlobalSettings.IsEnabled = true;
                        Logging.Log("Enabled IndicatorLights.");
                    }
                    break;
                case STATUS_OFF:
                    if (GlobalSettings.IsEnabled)
                    {
                        GlobalSettings.IsEnabled = false;
                        Logging.Log("Disabled IndicatorLights.");
                    }
                    else
                    {
                        Logging.Log("IndicatorLights is already disabled.");
                    }
                    break;
                default:
                    Logging.Log(HelpCommand.HelpStringOf(this));
                    break;
            }
        }
    }
}
