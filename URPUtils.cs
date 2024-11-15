using UnityEngine.Rendering;

namespace UGameCore.Utilities
{
    public static class URPUtils
    {
        public static void EnableShadows(bool enableShadows)
        {
#if UGAMECORE_URP
            UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset pipelineAsset = GetURPAsset();

            if (null == pipelineAsset)
                return;

            var type = pipelineAsset.GetType();

            var mainLightField = type.GetField(
                "m_MainLightShadowsSupported", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var additionalLightField = type.GetField(
                "m_AdditionalLightShadowsSupported", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            mainLightField.SetValue(pipelineAsset, enableShadows);
            additionalLightField.SetValue(pipelineAsset, enableShadows);

#endif
        }

#if UGAMECORE_URP
        public static UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset GetURPAsset()
        {
            return GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
        }
#endif

        public static UnityEngine.Object GetURPAssetAsObject()
        {
#if UGAMECORE_URP
            return GetURPAsset();
#else
            return null;
#endif
        }

        public static void SetForwardRenderer()
        {
#if UGAMECORE_URP
            SetRenderer(UnityEngine.Rendering.Universal.RenderingMode.Forward);
#endif
        }

#if UGAMECORE_URP
        public static void SetRenderer(UnityEngine.Rendering.Universal.RenderingMode renderingMode)
        {
            UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset pipelineAsset = GetURPAsset();

            if (null == pipelineAsset)
                return;

            foreach (UnityEngine.Rendering.Universal.ScriptableRendererData r in pipelineAsset.rendererDataList)
            {
                if (r is UnityEngine.Rendering.Universal.UniversalRendererData u)
                {
                    if (u.renderingMode != renderingMode)
                    {
                        u.renderingMode = renderingMode;
                        EditorUtilityEx.MarkObjectAsDirty(u);
                    }
                }
            }
        }
#endif
    }
}
