using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

/*This class demonstrates how to create a scriptable renderer feature and implement the methods that let you configure and inject 
 ScriptableRenderPass instances into the scriptable Renderer. It must inherit from ScriptableRendererFeature and implement a number
of methods.*/
public class LensFlareRendererFeature : ScriptableRendererFeature 
{


    /*Using this class, we create a scriptable Render Pass and enqueue its instance into the scriptable Renderer.*/
    class LensFlarePass : ScriptableRenderPass
    {
        private Material mat;
        private Mesh mesh;

        public LensFlarePass(Material mat, Mesh mesh)
        {
            this.mat = mat;
            this.mesh = mesh;
        }


        //This execute method is run every frame. Here is where we can implement our custom rendering functionality
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            //The CommandBufferPool.Get method gets the new command buffer and assigns a name to it
            CommandBuffer cmd = CommandBufferPool.Get(name: "LensFlarePass");

            //Get the camera data from the renderingData argument
            Camera camera = renderingData.cameraData.camera;
            //Set the projection matrix so that unity draws the quad in screen space
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            //Add the scale variable. Use the camera aspect ratio for the y coordinate
            Vector3 scale = new Vector3(1, camera.aspect, 1);
            //Draw a quad for each light at the screen space position of the light
            foreach (VisibleLight visibleLight in renderingData.lightData.visibleLights) {
                Light light = visibleLight.light;
                //convert the position of each light from world to viewport coordinates
                Vector3 pos = camera.WorldToViewportPoint(light.transform.position) * 2 - Vector3.one;
                //Set the z coordinate of each quad to zero so they're drawn on the same plane
                pos.z = 0;
                //the drawMesh method of the cmd object takes a mesh and material as arguments
                cmd.DrawMesh(mesh, Matrix4x4.TRS(pos, Quaternion.identity, scale), mat, 0, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
    public Material mat;
    public Mesh mesh;
    private LensFlarePass lensFlarePass; 
    /*The Create method is called on the following events:
        - when the Renderer Feature loads for the first time
        - when you enable or disable the renderer feature 
        - when you change a property in the inspector of the Renderer Feature
    */
    public override void Create()
    {
        lensFlarePass = new LensFlarePass(mat, mesh);
        //Draw the lens flare after the skybox
        lensFlarePass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }
    /*Unity calls this method every frame, once for each camera. This method lets you inject ScriptableRenderPass instances
     into the scriptable Renderer.
    We use the EnqueuePass method of the renderer object to enqueue lensFlarePass in the rendering queue*/
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (mat != null && mesh != null) 
        {
            renderer.EnqueuePass(lensFlarePass);
        }
        
    }

}

