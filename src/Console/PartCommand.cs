using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace IndicatorLights.Console
{
    /// <summary>
    /// Shows info about a specific part.
    /// </summary>
    class PartCommand : DebugConsole.DebugConsoleCommand
    {
        private const string COMMAND = "part";
        private const string HELP = "Shows information about the specified part";
        private const string MESHES = "meshes";
        private const string EMISSIVES = "emissives";
        private const string CONTROLLERS = "controllers";
        private const string CONTROLLER = "controller";
        private const string SET = "set";
        private const string USAGE = "<index> { " + MESHES + " | " + EMISSIVES + " | " + CONTROLLERS + " | " + CONTROLLER + " <index> }";
        private string[] originalArguments;

        public PartCommand() : base(COMMAND, HELP, USAGE) { }

        public override void Call(string[] arguments)
        {
            originalArguments = arguments;
            if (arguments.Length == 0)
            {
                Logging.Log(HelpCommand.HelpStringOf(this));
                return;
            }
            List<Part> parts = PartsCommand.GetIndicatorLightsParts(this);
            Part part = parts[parseInteger(arguments[0], 0, parts.Count, "part index")];
            ShowPart(
                part,
                (arguments.Length > 1) ? arguments[1] : null,
                (arguments.Length > 2) ? arguments.SubArray(2) : new string[0]);
        }

        private int parseInteger(string argument, int lowerBoundInclusive, int upperBoundExclusive, string label)
        {
            int index;
            try
            {
                index = int.Parse(argument);
            }
            catch (FormatException)
            {
                throw GetException("Index must be an integer");
            }
            if ((index < lowerBoundInclusive) || (index >= upperBoundExclusive))
            {
                throw GetException(label + " out of range [" + lowerBoundInclusive + ", " + (upperBoundExclusive - 1) + "]");
            }
            return index;
        }

        private void ShowPart(Part part, string qualifier, string[] arguments)
        {
            if (qualifier == null)
            {
                Logging.Log(string.Format(
                    "Part '{0}' has {1} meshes, {2} emissives, {3} controllers.",
                    Logging.GetTitle(part),
                    ModuleControllableEmissive.GetMeshes(part).Length,
                    FindModules<ModuleControllableEmissive>(part).Count,
                    FindModules<ModuleEmissiveControllerBase>(part).Count));
                return;
            }

            if (qualifier == MESHES)
            {
                ShowMeshes(part);
                return;
            }

            if (qualifier == EMISSIVES)
            {
                ShowModules<ModuleControllableEmissive>(part, "emissives", DescribeEmissive);
                return;
            }

            if (qualifier == CONTROLLERS)
            {
                ShowModules<ModuleEmissiveControllerBase>(part, "controllers", DescribeController);
                return;
            }

            if (qualifier == CONTROLLER)
            {

                if (arguments.Length < 1) throw GetException("No controller index specified");
                List<ModuleEmissiveControllerBase> controllers = FindModules<ModuleEmissiveControllerBase>(part);
                int index = parseInteger(arguments[0], 0, controllers.Count, "controller index");
                ShowController(controllers[index], arguments.SubArray(1));
                return;
            }

            throw GetException("/" + DebugConsole.COMMAND + " " + COMMAND + ": unknown qualifier '" + qualifier + "'");
        }

        private void ShowMeshes(Part part)
        {
            MeshRenderer[] renderers = ModuleControllableEmissive.GetMeshes(part);
            if ((renderers == null) || (renderers.Length < 1))
            {
                throw GetException("Part has no meshes: " + Logging.GetTitle(part));
            }
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("Part '{0}' has {1} meshes:", Logging.GetTitle(part), renderers.Length);
            for (int i = 0; i < renderers.Length; ++i)
            {
                builder.AppendFormat("\n{0}: {1}", i, DescribeMesh(renderers[i]));
            }
            Logging.Log(builder.ToString());
        }

        /// <summary>
        /// Get a display description for a mesh.
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
        private string DescribeMesh(MeshRenderer renderer)
        {
            string name = renderer.name;
            if (name.EndsWith(ModuleControllableEmissive.CLONE_TAG))
            {
                name = name.Substring(0, name.Length - ModuleControllableEmissive.CLONE_TAG.Length);
            }
            return name;
        }

        /// <summary>
        /// Delegate for functions rendering a PartModule as a string.
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        private delegate string ModuleDescriptor<TModule>(TModule module) where TModule : PartModule;

        /// <summary>
        /// Display a list of all modules of a particular type on the specified part.
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <param name="part"></param>
        /// <param name="label"></param>
        /// <param name="descriptor"></param>
        private void ShowModules<TModule>(Part part, string label, ModuleDescriptor<TModule> descriptor) where TModule : PartModule
        {
            List<TModule> modules = FindModules<TModule>(part);
            if (modules.Count == 0)
            {
                throw GetException("Part has no " + label + ": " + Logging.GetTitle(part));
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Part '{0}' has {1} {2}:", Logging.GetTitle(part), modules.Count, label);
            for (int i = 0; i < modules.Count; ++i)
            {
                builder.AppendFormat("\n{0}: {1}", i, descriptor(modules[i]));
            }
            Logging.Log(builder.ToString());
        }


        private void ShowController(ModuleEmissiveControllerBase controller, string[] arguments)
        {
            string description = DescribeController(controller);
            List<BaseField> idFields = new List<BaseField>();
            for (int i = 0; i < controller.Fields.Count; ++i)
            {
                BaseField field = controller.Fields[i];
                if (ToggleIDField.Is(field) || ScalarIDField.Is(field) || ColorSourceIDField.Is(field)) idFields.Add(field);
            }
            StringBuilder builder = new StringBuilder()
                .AppendFormat("{0}'s {1}:", controller.part.GetTitle(), controller.ClassName);
            if (arguments.Length < 1)
            {
                // Just show the controller.
                if (idFields.Count > 0)
                {
                    builder.AppendFormat("\n{0} parseable ID fields:", idFields.Count);
                    for (int i = 0; i < idFields.Count; ++i)
                    {
                        builder.AppendFormat("\n{0}: {1}", idFields[i].name, idFields[i].GetValue<string>(controller));
                    }
                }
                builder.AppendFormat("\nTo temporarily override a field: /{0} {1} ", DebugConsole.COMMAND, COMMAND);
                for (int i = 0; i < originalArguments.Length; ++i)
                {
                    builder.AppendFormat(" {0}", originalArguments[i]);
                }
                builder.AppendFormat(" {0} <name> <value>", SET);
            }
            else
            {
                // See if they're setting a field.
                if (arguments[0] != SET)
                {
                    throw GetException("/" + DebugConsole.COMMAND + " " + COMMAND + ": unknown qualifier '" + arguments[0] + "'");
                }
                if (arguments.Length < 3)
                {
                    throw GetException("/" + DebugConsole.COMMAND + " " + COMMAND + ": '" + SET + "' command requires field name and value");
                }
                string fieldName = arguments[1];
                BaseField field = null;
                for (int i = 0; i < idFields.Count; ++i)
                {
                    if (idFields[i].name == fieldName)
                    {
                        field = idFields[i];
                        break;
                    }
                }
                if (field == null)
                {
                    throw GetException("/" + DebugConsole.COMMAND + " " + COMMAND + ": Unknown field '" + fieldName + "'");
                }
                string fieldValue = arguments.SubArray(2).Join(" ");
                builder.AppendFormat(" Setting {0} to: {1}", fieldName, fieldValue);
                field.SetValue(fieldValue, controller);
                controller.ParseIDs();
            }
            Logging.Log(builder.ToString());
        }

        /// <summary>
        /// Get a descriptive string for a ModuleControllableEmissive.
        /// </summary>
        /// <param name="emissive"></param>
        /// <returns></returns>
        private string DescribeEmissive(ModuleControllableEmissive emissive)
        {
            StringBuilder builder = new StringBuilder();
            builder
                .Append(string.IsNullOrEmpty(emissive.emissiveName) ? emissive.ClassName : emissive.emissiveName)
                .Append(": ");
            if (string.IsNullOrEmpty(emissive.target))
            {
                builder.Append("no target!");
            }
            else
            {
                builder.AppendFormat("target \"{0}\"", emissive.target);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Get a descriptive string for a ModuleEmissiveControllerBase.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        private string DescribeController(ModuleEmissiveControllerBase controller)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(controller.ClassName);
            ModuleEmissiveController simpleController = controller as ModuleEmissiveController;
            if ((simpleController != null) && !string.IsNullOrEmpty(simpleController.controllerName))
            {
                builder.AppendFormat(" \"{0}\"", simpleController.controllerName);
            }
            builder.Append(", ");
            if (string.IsNullOrEmpty(controller.emissiveName))
            {
                builder.Append("no emissive");
            }
            else
            {
                builder.AppendFormat("emissive \"{0}\"", controller.emissiveName);
            }
            string description = controller.DebugDescription;
            if (string.IsNullOrEmpty(description))
            {
                IToggle toggle = controller as IToggle;
                if (toggle != null)
                {
                    description = string.Format("status = {0}", toggle.ToggleStatus);
                }
            }
            if (!string.IsNullOrEmpty(description))
            {
                builder.AppendFormat(": {0}", description);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Get a list of all modules of a particular type on the specified part.
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <param name="part"></param>
        /// <returns></returns>
        private static List<TModule> FindModules<TModule>(Part part) where TModule : PartModule
        {
            List<TModule> modules = new List<TModule>();
            for (int i = 0; i < part.Modules.Count; ++i)
            {
                TModule module = part.Modules[i] as TModule;
                if (module != null)
                {
                    modules.Add(module);
                }
            }
            return modules;
        }

    }
}
