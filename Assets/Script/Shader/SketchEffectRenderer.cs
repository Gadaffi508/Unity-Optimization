using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SketchEffectRenderer : ScriptableRendererFeature
{
    class SketchPass : ScriptableRenderPass
    {
        private Material sketchMaterial;
        private RTHandle tempTexture;
        private RTHandle cameraColorTargetHandle;

        public SketchPass(Material material)
        {
            this.sketchMaterial = material;
            this.renderPassEvent = RenderPassEvent.AfterRendering;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_TemporaryColorTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("SketchEffect");

            // 1. Render sahneyi geçici RT'ye aktar
            Blit(cmd, cameraColorTargetHandle, tempTexture, sketchMaterial);
            // 2. Ardýndan geri kopyala
            Blit(cmd, tempTexture, cameraColorTargetHandle);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // RTHandle artýk otomatik temizleniyor, gerek yok
        }
    }

    public Material sketchMaterial;
    private SketchPass sketchPass;

    public override void Create()
    {
        sketchPass = new SketchPass(sketchMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (sketchMaterial != null)
        {
            renderer.EnqueuePass(sketchPass);
        }
    }
}
