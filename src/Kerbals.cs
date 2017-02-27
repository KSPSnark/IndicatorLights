using Experience;
using System;
using System.Collections.Generic;

namespace IndicatorLights
{
    /// <summary>
    /// Gathers useful info about kerbal types in the game.
    /// </summary>
    static class Kerbals
    {
        public const string PilotClass = "Pilot";
        public const string EngineerClass = "Engineer";
        public const string ScientistClass = "Scientist";
        public const string TouristClass = "Tourist";
        public static readonly HashSet<string> Classes = FindKerbalClasses();
        public static readonly HashSet<string> Effects = FindKerbalEffects();

        /// <summary>
        /// Gets the class name of the kerbal, as a string.
        /// </summary>
        /// <param name="kerbal"></param>
        /// <returns></returns>
        public static string ClassOf(ProtoCrewMember kerbal)
        {
            if (kerbal == null) return null;
            if ((kerbal.type == ProtoCrewMember.KerbalType.Tourist)
                || (kerbal.experienceTrait == null))
            {
                return TouristClass;
            }
            else
            {
                return kerbal.experienceTrait.Config.Name;
            }
        }

        /// <summary>
        /// Get all the kerbal classes in the game. Doesn't include "tourist", since technically
        /// speaking that's not a class.
        /// </summary>
        /// <returns></returns>
        private static HashSet<string> FindKerbalClasses()
        {
            HashSet<string> kerbalClasses = new HashSet<string>();
            ExperienceSystemConfig systemConfig = new ExperienceSystemConfig();
            for (int traitIndex = 0; traitIndex < systemConfig.Categories.Count; ++traitIndex)
            {
                ExperienceTraitConfig traitConfig = systemConfig.Categories[traitIndex];
                Logging.Log("Adding kerbal class: " + traitConfig.Name);
                kerbalClasses.Add(traitConfig.Name);
            }
            kerbalClasses.Add(TouristClass); // since the game doesn't list "tourist" as an actual class
            return kerbalClasses;
        }

        /// <summary>
        /// Get all kerbal effect types.
        /// </summary>
        /// <param name="kerbalClasses"></param>
        /// <returns></returns>
        private static HashSet<string> FindKerbalEffects()
        {
            HashSet<string> kerbalEffects = new HashSet<string>();
            ExperienceSystemConfig systemConfig = new ExperienceSystemConfig();
            for (int traitIndex = 0; traitIndex < systemConfig.Categories.Count; ++traitIndex)
            {
                ExperienceTraitConfig traitConfig = systemConfig.Categories[traitIndex];
                if (traitConfig.Effects == null) continue;
                for (int effectIndex = 0; effectIndex < traitConfig.Effects.Count; ++effectIndex)
                {
                    ExperienceEffectConfig effectConfig = traitConfig.Effects[effectIndex];
                    if (!kerbalEffects.Contains(effectConfig.Name))
                    {
                        Logging.Log("Adding kerbal effect: " + effectConfig.Name);
                        kerbalEffects.Add(effectConfig.Name);
                    }
                }
            }
            return kerbalEffects;
        }
    }
}
