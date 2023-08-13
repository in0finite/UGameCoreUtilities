using UnityEngine.Rendering;

namespace UGameCore.Utilities
{
    public enum RenderPipelineId
    {
        Unknown = 0,
        BiRP,
        URP,
        HDRP,
    }

    public static class RenderPipelineIdDetector
    {
        public const string URPShaderTag = "UniversalPipeline";
        public const string HDRPShaderTag = "HDRenderPipeline";


        public static RenderPipelineId GetCurrentPipeline()
        {
            var rp = GraphicsSettings.currentRenderPipeline;
            if (rp == null)
                return RenderPipelineId.BiRP;

            string shaderTag = rp.renderPipelineShaderTag;

            if (URPShaderTag.Equals(shaderTag, System.StringComparison.OrdinalIgnoreCase))
                return RenderPipelineId.URP;

            if (HDRPShaderTag.Equals(shaderTag, System.StringComparison.OrdinalIgnoreCase))
                return RenderPipelineId.HDRP;

            return RenderPipelineId.Unknown;
        }
    }
}
