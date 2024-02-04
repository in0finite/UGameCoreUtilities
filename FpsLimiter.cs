using UnityEngine;

namespace UGameCore.Utilities
{
    public class FpsLimiter : MonoBehaviour, IConfigVarRegistrator
    {
        public ushort fpsLimitEditMode = 60;

        public IntConfigVar fpsLimitConfigVar;


        FpsLimiter()
        {
            EditorApplicationEvents.Register(this);
        }

        void Start()
        {
            SetEditModeLimit(this.fpsLimitEditMode);
        }

        void OnValidate()
        {
            SetEditModeLimit(this.fpsLimitEditMode);
        }

        static void SetEditModeLimit(ushort value)
        {
            FpsLimiterForEditMode.IsEnabled = true;
            FpsLimiterForEditMode.TargetFps = value;
        }

        static void SetPlayModeLimit(int value, RuntimePlatform? onlyOnPlatform, RuntimePlatform? excludePlatform)
        {
            if (onlyOnPlatform.HasValue && Application.platform != onlyOnPlatform.Value)
                return;

            if (excludePlatform.HasValue && Application.platform == excludePlatform.Value)
                return;

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = value.BetweenExclusive(0, 5) ? 5 : value;
        }

        void IConfigVarRegistrator.Register(IConfigVarRegistrator.Context context)
        {
            // Note for Web platform:
            // WaitForTargetFPS() takes a lot of time even when below target FPS, so for best performance,
            // we remove FPS limit.
            // But, with value of 0, note that Browser will lock FPS to screen refresh rate.
            // The only way to go above screen refresh rate, is to set FPS limiter to a higher value.

            int defaultFps = Application.platform == RuntimePlatform.WebGLPlayer
                ? 0
                : 60;

            this.fpsLimitConfigVar = new IntConfigVar
            {
                SerializationName = "fps_max",
                MinValue = -1,
                MaxValue = 1000,
                DefaultValueInt = defaultFps,
                ApplyDefaultValueWhenNotPresentInConfig = true,
                SetValueCallbackInt = (val) => SetPlayModeLimit(val, null, null),
            };

            context.ConfigVars.Add(this.fpsLimitConfigVar);
        }
    }
}
