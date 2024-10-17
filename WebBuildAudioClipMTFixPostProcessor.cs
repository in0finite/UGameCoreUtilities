#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UGameCore.Utilities
{
    public class WebBuildAudioClipMTFixPostProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 30;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != UnityEditor.BuildTarget.WebGL)
                return;

            if (!UnityEditor.PlayerSettings.WebGL.threadsSupport)
                return;

            Debug.Log($"Fixing code in framework.js");

            BuildFile[] buildFiles = report.GetFiles();
            Debug.Log($"Build files [{buildFiles.Length}]:\n{string.Join("\n", buildFiles.Select(f => f.path))}");

            //BuildFile frameworkJsFile = buildFiles.SingleOrDefault(f => f.path.EndsWith(".framework.js", System.StringComparison.OrdinalIgnoreCase));
            //if (frameworkJsFile.path == default)
            //    throw new System.InvalidOperationException($"Failed to find '.framework.js' file among build files");

            //PostprocessBuild(frameworkJsFile.path);

            //Debug.Log($"Successfully replaced code in framework.js");
        }

        public void PostprocessBuild(string frameworkJsFilePath)
        {
            string text = File.ReadAllText(frameworkJsFilePath);

            var toReplaceArray = new (string, string)[]
            {
                ("copyToChannel.apply(buffer,[GROWABLE_HEAP_F32().subarray(offs>>>0,offs+length>>>0),i,0])",
                "copyToChannel.apply(buffer,[new Float32Array(GROWABLE_HEAP_F32().subarray(offs>>>0,offs+length>>>0)),i,0])"),
            };

            foreach (var toReplace in toReplaceArray)
            {
                int oldLength = text.Length;
                text = text.Replace(toReplace.Item1, toReplace.Item2, System.StringComparison.Ordinal);
                if (oldLength == text.Length)
                    throw new System.InvalidOperationException($"Failed to replace string: {toReplace.Item1}");
            }

            File.WriteAllText(frameworkJsFilePath, text);
        }
    }
}
#endif
