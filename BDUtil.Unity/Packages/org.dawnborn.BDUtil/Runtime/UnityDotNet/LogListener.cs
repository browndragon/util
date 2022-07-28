using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace BDEase
{
    /// Bridge between unity & dotnet logging systems.
    public class LogListener : System.Diagnostics.TraceListener
    {
        static bool isInstalled = false;
        public void Initialize()
        {
            if (isInstalled) return;
            isInstalled = true;
            System.Diagnostics.Trace.Listeners.Add(new LogListener());
        }
        public override void Write(String s) => UnityEngine.Debug.Log(s);
        public override void WriteLine(String s) => UnityEngine.Debug.Log(s);
    }
}