using System.Runtime.CompilerServices;
using BDUtil.Clone;
using UnityEngine;

namespace BDUtil.Fluent
{
    public static class Logs
    {
        static Logs() => Initialize();
        public static void Initialize() => LogListener.Initialize();

        public static string IDStr(this GameObject thiz)
        => $"{thiz?.ToString() ?? "nil"}#{thiz?.GetInstanceID() ?? 0}";
        public static string IDStr(this Postfab thiz)
        => $"{thiz?.ToString() ?? "nil"}#{thiz?.GetInstanceID() ?? 0}[{thiz?.FabType ?? Postfab.FabTypes.Unknown}]";
        public static string IDStr(this Component thiz)
        => $"{thiz?.gameObject.IDStr()}[{thiz}]";
        public static string IDStr(this ScriptableObject thiz)
        => $"{thiz?.ToString() ?? "nil"}#{thiz?.GetInstanceID() ?? 0}";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string IDStr(this UnityEngine.Object thiz)
        => thiz switch
        {
            null => $"nil#0",
            GameObject go => go.IDStr(),
            Component co => co.IDStr(),
            ScriptableObject so => so.IDStr(),
            _ => $"{thiz?.ToString() ?? "nil"}#{thiz?.GetInstanceID() ?? 0}",
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Logging<T>(this T thiz, string message)
        {
            Debug.Log(message, thiz as UnityEngine.Object);
            return thiz;
        }

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