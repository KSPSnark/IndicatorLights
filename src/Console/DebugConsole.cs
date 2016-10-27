using KSP.UI.Screens.DebugToolbar;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace IndicatorLights.Console
{
    /// <summary>
    /// Adds custom console commands for IndicatorLights.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class DebugConsole : MonoBehaviour
    {
        internal const string COMMAND = "il";
        private const string HELP = "Work with the IndicatorLights mod";
        private static readonly Regex CONSECUTIVE_SPACE = new Regex(@"\s+");
        private static bool areCommandsInitialized = false;

        internal static readonly DebugConsoleCommand[] COMMANDS =
        {
            new HelpCommand(),
            new LightsEnabledCommand(),
            new PartsCommand(),
            new PartCommand()
        };

        private void Start()
        {
            if (areCommandsInitialized) return;
            DebugScreenConsole.AddConsoleCommand(COMMAND, OnCommand, HELP);
            areCommandsInitialized = true;
        }

        private void AddDebugConsoleCommand()
        {

        }

        private void OnCommand(string argumentString)
        {
            string[] arguments = ParseArguments(argumentString);
            if (arguments.Length == 0) arguments = new string[] { HelpCommand.COMMAND };

            string command = arguments[0];
            string[] commandArgs = new string[arguments.Length - 1];
            for (int i = 0; i < commandArgs.Length; ++i)
            {
                commandArgs[i] = arguments[i + 1];
            }

            for (int i = 0; i < COMMANDS.Length; ++i)
            {
                if (COMMANDS[i].Command == command)
                {
                    if ((commandArgs.Length == 1) && (commandArgs[0] == HelpCommand.COMMAND))
                    {
                        Logging.Log(HelpCommand.HelpStringOf(COMMANDS[i]));
                        return;
                    }
                    try
                    {
                        COMMANDS[i].Call(commandArgs);
                    }
                    catch (DebugCommandException e)
                    {
                        Logging.Error("/" + COMMAND + " " + command + ": " + e.Message);
                    }
                    return;
                }
            }
            Logging.Error("Unknown command: /" + COMMAND + " " + command);
        }


        /// <summary>
        /// Parses an argument string into an array of zero or more individual arguments.
        /// </summary>
        /// <param name="argString"></param>
        /// <returns></returns>
        private static string[] ParseArguments(string argString)
        {
            if (argString == null) return new string[0];

            argString = argString.Trim();
            if (string.Empty == argString) return new string[0];
            return CONSECUTIVE_SPACE.Replace(argString, (m) => " ").Split(' ');
        }


        /// <summary>
        /// Base class for console commands.
        /// </summary>
        internal abstract class DebugConsoleCommand
        {
            private readonly string command;
            private readonly string help;
            private readonly string usage;

            protected DebugConsoleCommand(string command, string help, string usage = null)
            {
                this.command = command;
                this.help = help;
                this.usage = usage;
            }

            public string Command
            {
                get { return command; }
            }

            public string Help
            {
                get { return help; }
            }

            public string Usage
            {
                get { return usage; }
            }
            
            public abstract void Call(string[] arguments);

            /// <summary>
            /// Gets all parts on the current vessel.
            /// </summary>
            /// <returns></returns>
            internal List<Part> GetVesselParts()
            {
                if (HighLogic.LoadedSceneIsEditor)
                {
                    return EditorLogic.fetch.ship.Parts;
                }
                else if (HighLogic.LoadedSceneIsFlight)
                {
                    return FlightGlobals.ActiveVessel.Parts;
                }
                else
                {
                    throw GetException("Only valid in editor or flight");
                }
            }

            protected DebugCommandException GetException(string message)
            {
                return new DebugCommandException(command, message);
            }
        }

        /// <summary>
        /// Thrown when a command receives invalid syntax.
        /// </summary>
        internal class DebugCommandException : Exception
        {
            private readonly string command;

            public DebugCommandException(string command, string message) : base(message)
            {
                this.command = command;
            }

            public DebugCommandException(string command, string message, Exception cause) : base(message, cause)
            {
                this.command = command;
            }

            public string Command
            {
                get { return command; }
            }
        }
    }
}
