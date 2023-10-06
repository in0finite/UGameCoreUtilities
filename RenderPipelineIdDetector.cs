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

        public const string URPAssetTypeName = "UniversalRenderPipelineAsset";
        public const string HDRPAssetTypeName = "HDRenderPipelineAsset";


        public static RenderPipelineId GetCurrentPipeline()
        {
            var rp = GraphicsSettings.currentRenderPipeline;
            if (rp == null)
                return RenderPipelineId.BiRP;

            // here we could use "RenderPipelineAsset.renderPipelineShaderTag", however it doesn't work in a build

            string typeName = rp.GetType().Name;

            if (typeName.Equals(HDRPAssetTypeName, System.StringComparison.Ordinal))
                return RenderPipelineId.HDRP;

            if (typeName.Equals(URPAssetTypeName, System.StringComparison.Ordinal))
                return RenderPipelineId.URP;

            return RenderPipelineId.Unknown;
        }
    }
}
