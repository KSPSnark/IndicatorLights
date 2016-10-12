using System.Text;

namespace IndicatorLights.Console
{
    /// <summary>
    /// Show all the various IndicatorLights console commands.
    /// </summary>
    internal class HelpCommand : DebugConsole.DebugConsoleCommand
    {
        public const string COMMAND = "help";
        private const string HELP = "Shows this message";

        public HelpCommand() : base(COMMAND, HELP) {}

        public override void Call(string[] arguments)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Commands available via /{0}:", DebugConsole.COMMAND);
            for (int i = 0; i < DebugConsole.COMMANDS.Length; ++i)
            {
                builder.Append("\n").Append(HelpStringOf(DebugConsole.COMMANDS[i]));
            }
            Logging.Log(builder.ToString());
        }

        internal static string HelpStringOf(DebugConsole.DebugConsoleCommand command)
        {
            if (command.Usage == null)
            {
                return string.Format("/{0} {1}: {2}", DebugConsole.COMMAND, command.Command, command.Help);
            }
            else
            {
                return string.Format("/{0} {1} {2}: {3}", DebugConsole.COMMAND, command.Command, command.Usage, command.Help);
            }
        }
    }
}
