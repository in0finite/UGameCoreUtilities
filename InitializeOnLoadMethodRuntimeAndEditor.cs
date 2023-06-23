namespace UGameCore.Utilities
{
    public class InitializeOnLoadMethodRuntimeAndEditor :
#if UNITY_EDITOR
        UnityEditor.InitializeOnLoadMethodAttribute
#else
        UnityEngine.RuntimeInitializeOnLoadMethodAttribute
#endif
    {
#if !UNITY_EDITOR
        public InitializeOnLoadMethodRuntimeAndEditor()
            : base(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)
        {
        }
#endif
    }
}
