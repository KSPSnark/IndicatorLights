using System;
using UnityEngine;

namespace IndicatorLights
{
    /// <summary>
    /// Utility wrapper for logging messages.
    /// </summary>
    static class Logging
    {
        public static void Log(object message)
        {
            Debug.Log("[IndicatorLights] " + message);
        }

        public static void Warn(object message)
        {
            Debug.LogWarning("[IndicatorLights] " + message);
        }

        public static void Error(object message)
        {
            Debug.LogError("[IndicatorLights] " + message);
        }

        public static void Exception(string message, Exception e)
        {
            Error(message + " (" + e.GetType().Name + ") " + e.Message + ": " + e.StackTrace);
        }

        public static void Exception(Exception e)
        {
            Error("(" + e.GetType().Name + ") " + e.Message + ": " + e.StackTrace);
        }

        public static string GetTitle(this Part part)
        {
            return (part == null) ? "<null part>" : ((part.partInfo == null) ? part.partName : part.partInfo.title);
        }

        public static string GetIdentifier(object obj)
        {
            Identifiers.IIdentifiable identifiable = obj as Identifiers.IIdentifiable;
            return (identifiable == null) ? obj.GetType().Name : identifiable.Identifier;
        }

        /// <summary>
        /// Useful for debugging per-update-frame events without spamming the log to uselessness.
        /// </summary>
        public class Throttled
        {
            private readonly string label;
            private readonly RateLimiter cooldown;

            public Throttled(string label, long milliseconds)
            {
                this.label = label;
                this.cooldown = new RateLimiter(TimeSpan.FromMilliseconds(milliseconds));
            }

            public bool Log(object message)
            {
                DateTime now = DateTime.Now;
                if (!cooldown.Check()) return false;
                if (string.IsNullOrEmpty(label))
                {
                    Logging.Log(message);
                }
                else
                {
                    Logging.Log(label + ": " + message);
                }
                return true;
            }
        }
    }
}