using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Diagnostics;

namespace UGameCore.Utilities
{
    public static class FpsLimiterForEditMode
    {
        static readonly Stopwatch s_stopwatch = Stopwatch.StartNew();

        public static bool IsEnabled { get; set; } = false;
        public static ushort TargetFps { get; set; } = 60;


#if UNITY_EDITOR
        static FpsLimiterForEditMode()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        static void OnEditorUpdate()
        {
            if (!IsEnabled)
                return;

            if (TargetFps <= 0)
                return;

            if (TargetFps < 5)
                TargetFps = 5;

            if (Application.isPlaying && !EditorApplication.isPaused)
                return;

            float elapsedMs = (float)s_stopwatch.Elapsed.TotalMilliseconds;
            s_stopwatch.Restart();
            float desiredMs = 1000.0f / TargetFps;

            if (elapsedMs >= desiredMs) // FPS is lower than target FPS, no need to sleep
                return;

            int sleepMs = Mathf.CeilToInt(desiredMs - elapsedMs);
            
            System.Threading.Thread.Sleep(sleepMs);
            s_stopwatch.Restart();
        }
#endif
    }
}
