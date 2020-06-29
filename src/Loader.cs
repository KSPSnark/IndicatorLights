using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Runs once when the game hits the main menu on startup. Loads custom IndicatorLights
    /// config used in various places.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class Loader : MonoBehaviour
    {
        private const string MASTER_NODE_NAME = "IndicatorLights";

        private delegate void ConfigLoader(ConfigNode node);

        /// <summary>
        /// Here when the script starts up.
        /// </summary>
        public void Start()
        {
            Configuration.Configuration_Init();
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs(MASTER_NODE_NAME);
            if (configs.Length < 1)
            {
                Logging.Error("Can't find main " + MASTER_NODE_NAME + " config node! Some features will be inoperable.");
                return;
            }
            ConfigNode masterNode = configs[0].config;
            ProcessMasterNode(masterNode);

            // Community Trait Icons integration. If initCTIWrapper returns true,
            // it means that Community Trait Icons has been identified as being
            // loaded, and therefore we can load its colors.
            if (Compatibility.CTIWrapper.initCTIWrapper())
            {
                StartCoroutine(ModuleCrewIndicator.LoadCommunityTraitIconColors());
            }
        }

        /// <summary>
        /// Process the main IndicatorLights config node.
        /// </summary>
        /// <param name="masterNode"></param>
        private static void ProcessMasterNode(ConfigNode masterNode)
        {
            TryProcessChildNode(masterNode, ModuleCrewIndicator.CONFIG_NODE_NAME, ModuleCrewIndicator.LoadConfig);
        }

        /// <summary>
        /// Looks for a child with the specified name, and delegates to it if found.
        /// </summary>
        /// <param name="masterNode"></param>
        /// <param name="childName"></param>
        /// <param name="loader"></param>
        private static void TryProcessChildNode(ConfigNode masterNode, string childName, ConfigLoader loader)
        {
            ConfigNode child = masterNode.nodes.GetNode(childName);
            if (child == null)
            {
                Logging.Warn("Child node " + childName + " of master config node " + MASTER_NODE_NAME + " not found, skipping");
            }
            else
            {
                Logging.Log("Loading " + masterNode.name + " config: " + child.name);
                loader(child);
            }
        }
    }
}