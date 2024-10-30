using UnityEngine;

namespace UGameCore.Utilities
{
    public class FpsLimiter : MonoBehaviour, IConfigVarRegistrator
    {
        public ushort fpsLimitEditMode = 60;

        // Note for Web platform:
        // WaitForTargetFPS() takes a lot of time even when below target FPS, so for best performance,
        // we remove FPS limit.
        // But, with value of 0, note that Browser will lock FPS to screen refresh rate.
        // The only way to go above screen refresh rate, is to set FPS limiter to a higher value.

        // Edit: seems that certain values (such are: 15, 20, 30, 60) cause the Unity's FPS limiter on Web to
        // misbehave: limiting the app to different FPS, taking some time from frame, preventing app from running in
        // background.
        // But other values seem to be fine (eg. 61), so we choose to limit to 61 by default, rather than having max
        // CPU/GPU usage with unlocked FPS.

        public readonly IntConfigVar fpsLimitConfigVar = new IntConfigVar
        {
            SerializationName = "fps_max",
            Description = "Maximum FPS",
            MinValue = -1,
            MaxValue = 1000,
            DefaultValueInt = F.IsOnWebPlatform ? 61 : 60,
            ApplyDefaultValueWhenNotPresentInConfig = true,
            SetValueCallbackInt = (val) => SetPlayModeLimit(val, null, null),
        };


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
    }
}
