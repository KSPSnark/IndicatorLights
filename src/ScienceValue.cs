namespace IndicatorLights
{
    /// <summary>
    /// Classifies science value as low/medium/high.
    /// </summary>
    static class ScienceValue
    {
        public enum Fraction
        {
            Low,
            Medium,
            High
        }

        public const float DEFAULT_LOW_SCIENCE_THRESHOLD = 0.15f;
        public const float DEFAULT_HIGH_SCIENCE_THRESHOLD = 0.7f;

        /// <summary>
        /// Gets the subject with the specified ID as a fraction category.
        /// </summary>
        /// <param name="subjectID"></param>
        /// <param name="lowThreshold"></param>
        /// <param name="highThreshold"></param>
        /// <returns></returns>
        public static Fraction Get(string subjectID, float lowThreshold, float highThreshold)
        {
            return FractionOf(Get(subjectID), lowThreshold, highThreshold);
        }

        /// <summary>
        /// Gets the subject with the specified ID as a floating-point fraction.
        /// </summary>
        /// <param name="subjectID"></param>
        /// <returns></returns>
        public static float Get(string subjectID)
        {
            if (ResearchAndDevelopment.Instance == null) return 1; // it's a sandbox game
            ScienceSubject subject = ResearchAndDevelopment.GetSubjectByID(subjectID);
            // subject will be null if we've never retrieved this science result before
            return (subject == null) ? 1 : ResearchAndDevelopment.GetSubjectValue(subject.science, subject);
        }

        public static Fraction FractionOf(float fraction, float lowThreshold, float highThreshold)
        {
            if (fraction >= highThreshold) return Fraction.High;
            if (fraction < lowThreshold) return Fraction.Low;
            return Fraction.Medium;
        }
    }
}
