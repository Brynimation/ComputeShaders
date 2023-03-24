using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.XR.XRDisplaySubsystem;

/*CustomBlitRendererFeature is a custom renderer feature. It calls a custom render pass, ColourBlitPass, that blits the
 opaque texture to the camera colour target.
 Note that internal classes are only accessible from classes defined in the same assembly*/
internal class ColourBlitRendererFeature : ScriptableRendererFeature
{
    internal class ColourBlitPass : ScriptableRenderPass 
    {
        ProfilingSampler profilingSampler = new ProfilingSampler("ColourBlit");
        Material material;
        RenderTargetIdentifier cameraColourTarget;
        float intensity;

        public ColourBlitPass(Material material) 
        {
            this.material = material;
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }
        public void SetTarget(RenderTargetIdentifier colourHandle, float intensity) 
        {
            cameraColourTarget= colourHandle;
            this.intensity = intensity;
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(new RenderTargetIdentifier(cameraColourTarget, 0, CubemapFace.Unknown, 0));
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Camera cam = renderingData.cameraData.camera;
            if (cam.cameraType != CameraType.Game) 
            {
                return;
            }
            if (material == null) 
            {
                return;
            }
            CommandBuffer cmd = CommandBufferPool.Get("ColourBlitPass");
            using (new ProfilingScope(cmd, profilingSampler)) 
            {
                material.SetFloat("_Intensity", intensity);
                cmd.SetRenderTarget(new RenderTargetIdentifier(cameraColourTarget, 0, CubemapFace.Unknown, 0));
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }
    }
    public Shader shader;
    public float intensity;

    Material material;
    ColourBlitPass renderPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game) 
        {
            renderPass.ConfigureInput(ScriptableRenderPassInput.Color); //configures the input requirements for this render pass
            renderPass.SetTarget(renderer.cameraColorTarget, intensity);
            renderer.EnqueuePass(renderPass);
        }
    }
    public override void Create()
    {
        if (shader != null) 
        {
            material = new Material(shader);
        }
        renderPass = new ColourBlitPass(material);
    }
    protected override void Dispose(bool disposing) 
    {
        CoreUtils.Destroy(material);
    }


}
