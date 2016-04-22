using System;
using UnityEngine;

namespace IndicatorLights
{
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
    }
}