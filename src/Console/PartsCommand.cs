using System.Collections.Generic;
using System.Text;

namespace IndicatorLights.Console
{
    class PartsCommand : DebugConsole.DebugConsoleCommand
    {
        private const string COMMAND = "parts";
        private const string HELP = "List all IndicatorLights parts on the current vessel";

        public PartsCommand() : base(COMMAND, HELP) {}

        public override void Call(string[] arguments)
        {
            List<Part> parts = GetIndicatorLightsParts(this);
            if (parts.Count == 0)
            {
                Logging.Log("No IndicatorLights parts are present on this vessel.");
                return;
            }
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0} IndicatorLights parts found:", parts.Count);
            for (int i = 0; i < parts.Count; ++i)
            {
                builder.AppendFormat("\n{0}: {1}", i, Logging.GetTitle(parts[i]));
            }
            Logging.Log(builder.ToString());
        }

        internal static List<Part> GetIndicatorLightsParts(DebugConsole.DebugConsoleCommand command)
        {
            List<Part> allParts = command.GetVesselParts();
            List<Part> filteredParts = new List<Part>();
            for (int i = 0; i < allParts.Count; ++i)
            {
                Part part = allParts[i];
                if (IsIndicatorLightsPart(part))
                {
                    filteredParts.Add(part);
                }
            }
            return filteredParts;
        }

        internal static bool IsIndicatorLightsPart(Part part)
        {
            for (int i = 0; i < part.Modules.Count; ++i)
            {
                PartModule module = part.Modules[i];
                if (module is ModuleControllableEmissive) return true;
                if (module is ModuleEmissiveController) return true;
            }
            return false;
        }
    }
}
