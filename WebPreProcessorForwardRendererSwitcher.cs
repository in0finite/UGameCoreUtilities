#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UGameCore.Utilities
{
    [FilePath(nameof(WebPreProcessorForwardRendererSwitcher) + ".asset", FilePathAttribute.Location.ProjectFolder)]
    public class WebPreProcessorForwardRendererSwitcherScriptableSingleton : ScriptableSingleton<WebPreProcessorForwardRendererSwitcherScriptableSingleton>
    {
        public bool SwitchToForwardRendererOnWeb = true;
    }

    public class WebPreProcessorForwardRendererSwitcher : IPreprocessBuildWithReport
    {
        public int callbackOrder => 40;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
                return;

            if (RenderPipelineIdDetector.GetCurrentPipeline() != RenderPipelineId.URP)
                return;

            var instance = WebPreProcessorForwardRendererSwitcherScriptableSingleton.instance;

            if (!instance.SwitchToForwardRendererOnWeb)
            {
                Debug.Log($"Skipped switching to Forward renderer on Web platform");
                return;
            }

            Debug.Log($"Switching to Forward renderer on Web platform");

            URPUtils.SetForwardRenderer();
        }
    }
}
#endif
