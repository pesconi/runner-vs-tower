using System;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>Runs an Editor step in batchmode and exits with 0 (PASS) or 1 (FAIL).</summary>
    public static class BatchRunner
    {
        public static void Run(string label, Action action)
        {
            try
            {
                action();
                Debug.Log($"[{label}] PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError($"[{label}] FAIL");
                EditorApplication.Exit(1);
            }
        }

        public static string GetArgument(string name)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                if (arguments[i] == name)
                {
                    return arguments[i + 1];
                }
            }

            return null;
        }

        public static bool HasFlag(string name)
        {
            return Array.IndexOf(Environment.GetCommandLineArgs(), name) >= 0;
        }
    }
}
