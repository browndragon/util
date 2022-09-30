using System;
using UnityEngine;

namespace BDUtil
{
    public static class Debugs
    {
        static Debugs() => Initialize();
        public static void Initialize() => LogListener.Initialize();

        public static string IDStr(this GameObject thiz)
        => $"{thiz?.ToString() ?? "nil"}#{thiz?.GetInstanceID() ?? 0}";
        public static string IDStr(this Component thiz)
        => $"{thiz?.gameObject.IDStr()}[{thiz}]";
        public static string IDStr(this ScriptableObject thiz)
        => $"{thiz?.ToString() ?? "nil"}#{thiz?.GetInstanceID() ?? 0}";
        public static string IDStr(this UnityEngine.Object thiz)
        => thiz switch
        {
            null => $"nil#0",
            GameObject go => go.IDStr(),
            Component co => co.IDStr(),
            ScriptableObject so => so.IDStr(),
            _ => $"{thiz?.ToString() ?? "nil"}#{thiz?.GetInstanceID() ?? 0}",
        };

        /// Bridge between unity & dotnet logging systems.
        public class LogListener : System.Diagnostics.TraceListener
        {
            static bool isInstalled = false;
            public static void Initialize()
            {
                if (isInstalled) return;
                isInstalled = true;
                System.Diagnostics.Trace.Listeners.Add(new LogListener());
            }
            public override void Write(string s) => UnityEngine.Debug.Log(s ?? "");
            public override void WriteLine(string s) => UnityEngine.Debug.Log(s ?? "");
        }
    }
}