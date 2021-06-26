using System.Collections.Generic;
using UnityEngine;

namespace IndicatorLights
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    class SplashScreen : MonoBehaviour
    {
        private static float MAX_TIP_TIME = 4; // seconds

        private static readonly string[] NEW_TIPS =
        {
            "Checking Indicator Lights...",
            "Gazing At Blinkenlights..."
        };

        /// <summary>
        /// Snark's sneaky little way of thanking various users of this mod for helpful contributions.
        /// </summary>
        private static readonly string[] THANK_USERS =
        {
            "Beetlecat",       // for suggesting random-blinking lights
            "DStaal",          // for USI construction-port support, which nudged me into field@module syntax
            "Enceos",          // feature suggestions: tint shaders, alpha colors, parent-part indicators
            "Fwiffo",          // LOTS of helpful moddability suggestions and detailed feedback
            "Jiraiyah",        // Impossible Innovations support
            "Kerbart",         // lots of helpful suggestions, especially in v0.9.1
            "Kerbas_ad_astra", // putting in lots of work for providing Ven's Stock Revamp support
            "mikerl",          // suggestion for crew indicators to support other kerbal types
            "NathanKell",      // just 'coz he's awesome :-)
            "NecroBones",      // lots and lots of patient hand-holding, teaching me how to model
            "SchwinnTropius",  // Patches for Mk1CabinHatch, Porkjet's PartOverhauls
            "Sharpy",          // help with TweakScale support
            "smokytehbear",    // MKS support; use case helped flesh out crew indicator behavior
            "steedcrugeon",    // suggestion for vessel-situation feature; innovatively using IL in mod
            "VintageXP",       // lots of patient help with Unity and Blender
            "Wcmille",         // suggestion for crew indicators to flash if crew report is available
            "cakepie",         // Community Trait Icons integration
            "Tonka Crash",     // fix for missing Blinkenlights agent bug
            "linuxgurugamer",  // helpful PR to fix Unity initialization bug
            "Rodger",          // suggestion for vessel control level syntax
            "Dominiquini"      // helpfully supplying config for Mk2 inline cockpit
        };

        internal void Awake()
        {
            LoadingScreen.LoadingScreenState state = FindLoadingScreenState();
            if (state != null)
            {
                InsertTips(state);
                if (state.tipTime > MAX_TIP_TIME) state.tipTime = MAX_TIP_TIME;
            }
        }

        /// <summary>
        /// Finds the loading screen where we want to tinker with the tips,
        /// or null if there's no suitable candidate.
        /// </summary>
        /// <returns></returns>
        private static LoadingScreen.LoadingScreenState FindLoadingScreenState()
        {
            if (LoadingScreen.Instance == null) return null;
            List<LoadingScreen.LoadingScreenState> screens = LoadingScreen.Instance.Screens;
            if (screens == null) return null;
            for (int i = 0; i < screens.Count; ++i)
            {
                LoadingScreen.LoadingScreenState state = screens[i];
                if ((state != null) && (state.tips != null) && (state.tips.Length > 1)) return state;
            }
            return null;
        }

        /// <summary>
        /// Insert our list of tips into the specified loading screen state.
        /// </summary>
        /// <param name="state"></param>
        private static void InsertTips(LoadingScreen.LoadingScreenState state)
        {
            List<string> tipsList = new List<string>();
            tipsList.AddRange(state.tips);
            tipsList.AddRange(NEW_TIPS);
            int numThanks = 1 + (int)Mathf.Sqrt(THANK_USERS.Length);
            System.Random random = new System.Random(System.DateTime.UtcNow.Second);
            for (int i = 0; i < numThanks; ++i)
            {
                tipsList.Add(string.Format("Thanking {0}...", THANK_USERS[random.Next(THANK_USERS.Length)]));
            }
            state.tips = tipsList.ToArray();
        }
    }
}