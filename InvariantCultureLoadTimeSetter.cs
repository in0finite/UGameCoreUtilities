using System.Globalization;

namespace UGameCore.Utilities
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    static class InvariantCultureLoadTimeSetter
    {
        static InvariantCultureLoadTimeSetter() => SetInvariantCulture();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
#endif
        [InitializeOnLoadMethodRuntimeAndEditor]
        static void Init() => SetInvariantCulture();

        static void SetInvariantCulture()
        {
            UnityEngine.Debug.Log("setting Invariant culture");

            // set culture to invariant to avoid localization problems
            // need to do it on load, before any other thread is created

            var culture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
