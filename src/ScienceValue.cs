using System.Collections.Generic;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Provides access to previously retrieved science subjects, by ID.
    /// 
    /// The existence of this class is a hack workaround for a bug / feature in stock KSP.
    /// Really, the function ResearchAndDevelopment.GetSubjectByID does exactly what
    /// I want, and if I could just call that function, this class here would be completely
    /// unnecessary.  Unfortunately, GetSubjectByID has a side effect, that if you ask for
    /// a subject that isn't there (i.e. when it returns null), it logs an error message.
    /// That makes it unusable for purposes of this mod, since I need to call it on every
    /// update cycle, which would cause huge continuous log spam all the time.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class ScienceValue : MonoBehaviour
    {
        public enum Fraction
        {
            Low,
            Medium,
            High
        }

        public const float DEFAULT_LOW_SCIENCE_THRESHOLD = 0.15f;
        public const float DEFAULT_HIGH_SCIENCE_THRESHOLD = 0.7f;

        // Subjects that we've asked ResearchAndDevelopment for, and received null,
        // meaning that they're not available. We have to keep track of these because
        // asking ResearchAndDevelopment for them logs an error message, so we have
        // to avoid spamming it.
        private readonly HashSet<string> missingSubjects = new HashSet<string>();


        private static ScienceValue instance = null;

        /// <summary>
        /// Gets the subject with the specified ID, or null if none is available.
        /// </summary>
        /// <param name="subjectID"></param>
        /// <param name="lowThreshold"></param>
        /// <param name="highThreshold"></param>
        /// <returns></returns>
        public static Fraction Get(string subjectID, float lowThreshold, float highThreshold)
        {
            if ((instance == null) || (ResearchAndDevelopment.Instance == null)) return Fraction.High;
            if (instance.missingSubjects.Contains(subjectID))
            {
                // We've already asked ResearchAndDevelopment about this subject ID,
                // and were already told that it's not there, so it means no science
                // on the subject has been reported yet and therefore full value is
                // available.
                return Fraction.High;
            }

            // Okay, look it up.
            ScienceSubject subject = ResearchAndDevelopment.GetSubjectByID(subjectID);
            if (subject == null)
            {
                // Not available!  Remember that fact and return high value.
                instance.missingSubjects.Add(subjectID);
                return Fraction.High;
            }

            // Okay, science for this subject was previously reported.  Return
            // the appropriate fraction.
            float value = ResearchAndDevelopment.GetSubjectValue(subject.science, subject);
            return FractionOf(value, lowThreshold, highThreshold);
        }

        public void Awake()
        {
            GameEvents.OnScienceRecieved.Add(OnScienceReceived);
        }

        public void Start()
        {
            instance = this;
            missingSubjects.Clear();
        }

        public void OnDestroy()
        {
            GameEvents.OnScienceRecieved.Remove(OnScienceReceived);
        }

        private void OnScienceReceived(
            float scienceAmount,
            ScienceSubject subject,
            ProtoVessel vessel,
            bool someFlagThatIHaveNoIdeaWhatItDoes)
        {
            // We just got science coming in.  If we previously asked for it and were told
            // that it's not available, we should forget that, since it's no longer the case.
            missingSubjects.Remove(subject.id);
        }

        private static Fraction FractionOf(float fraction, float lowThreshold, float highThreshold)
        {
            if (fraction >= highThreshold) return Fraction.High;
            if (fraction < lowThreshold) return Fraction.Low;
            return Fraction.Medium;
        }
    }
}
