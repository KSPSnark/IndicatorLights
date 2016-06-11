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

        /// <summary>
        /// Useful for debugging per-update-frame events without spamming the log to uselessness.
        /// </summary>
        public class Throttled
        {
            private readonly string label;
            private readonly TimeSpan cooldown;
            private DateTime nextLog;

            public Throttled(string label, long milliseconds)
            {
                this.label = label;
                this.cooldown = TimeSpan.FromMilliseconds(milliseconds);
                nextLog = DateTime.MinValue;
            }

            public bool Log(object message)
            {
                DateTime now = DateTime.Now;
                if (now < nextLog) return false;
                nextLog = now + cooldown;
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