using System;

namespace BDUtil
{
    public static class Debugs
    {
        static Debugs() => Initialize();
        public static void Initialize() => LogListener.Initialize();

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
            public override void Write(String s) => UnityEngine.Debug.Log(s);
            public override void WriteLine(String s) => UnityEngine.Debug.Log(s);
        }
    }
}