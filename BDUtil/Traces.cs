using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BDUtil.Traces
{
    public static class Traces
    {
        public static T DoTrace<T>(
            this T thiz,
            string message = default,
            [CallerFilePath] string callerPath = default,
            [CallerMemberName] string callerName = default,
            [CallerLineNumber] int lineNumber = default
        )
        {
            Trace.WriteLine($"f:{callerPath}\tn:{callerName}\tl:{lineNumber}\t{thiz}{(message.IsEmpty() ? "" : ": ")}{message}");
            return thiz;
        }

        public static bool OrTrace(this bool thiz, string tmpl = default, params object[] args)
        {
            if (!thiz) Trace.WriteLine(string.Format(tmpl ?? "Unexpected False", args));
            return thiz;
        }
        public static T OrTrace<T>(this T thiz, string tmpl = default, params object[] args)
        where T : class
        {
            if (thiz == null) Trace.WriteLine(string.Format(tmpl ?? $"Unexpected Null", args));
            return thiz;
        }
        public static string OrTrace(this string thiz, string tmpl = default, params object[] args)
        {
            if (thiz.IsEmpty()) Trace.WriteLine(string.Format(tmpl ?? "Unexpected empty string", args));
            return thiz;
        }
        public static IEnumerable<T> OrTrace<T>(this IEnumerable<T> thiz, string tmpl = default, params object[] args)
        {
            if (thiz.IsEmpty()) Trace.WriteLine(string.Format(tmpl ?? "Unexpected empty enumerable", args));
            return thiz;
        }
    }
}
