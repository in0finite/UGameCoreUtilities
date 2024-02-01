using UnityEngine;

namespace UGameCore.Utilities
{
    public class FpsLimiter : MonoBehaviour, IConfigVarRegistrator
    {
        public ushort fpsLimitEditMode = 60;

        public IntConfigVar fpsLimitConfigVar = new IntConfigVar
        {
            SerializationName = "fps_max",
            MinValue = 5,
            MaxValue = 1000,
            DefaultValueInt = 60,
            ApplyDefaultValueWhenNotPresentInConfig = true,
            SetValueCallbackInt = (val) => SetPlayModeLimit(val),
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

        static void SetPlayModeLimit(int value)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = value;
        }

        void IConfigVarRegistrator.Register(IConfigVarRegistrator.Context context)
        {
            context.ConfigVars.Add(this.fpsLimitConfigVar);
        }
    }
}
