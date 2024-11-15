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
        public int callbackOrder => -500;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL)
                return;

            if (RenderPipelineIdDetector.GetCurrentPipeline() != RenderPipelineId.URP)
                return;

            var instance = WebPreProcessorForwardRendererSwitcherScriptableSingleton.instance;

            const string logMsg = "switching to Forward renderer on Web platform";

            if (!instance.SwitchToForwardRendererOnWeb)
            {
                Debug.Log($"Skipped {logMsg}");
                return;
            }

            Debug.Log(logMsg);

            URPUtils.SetForwardRenderer();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Done {logMsg}");
        }
    }
}
#endif
