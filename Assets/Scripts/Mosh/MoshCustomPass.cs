using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Mosh
{
    public class MoshCustomPass : ScriptableRenderPass
    {
        private static readonly int s_BlockSize = Shader.PropertyToID("_BlockSize");
        private static readonly int s_WorkTex = Shader.PropertyToID("_WorkTex");
        private static readonly int s_DispTex = Shader.PropertyToID("_DispTex");
        private Material m_MoshMat;

        private RenderTexture m_SourceTexture;
        private RenderTargetIdentifier currentTarget;

        private RenderTexture m_WorkBuffer;
        private RenderTexture m_DispBuffer;
        private RenderTexture PassTexture;

        private int m_BlockSize = 4;
        private int m_LastFrame = 250;

        public MoshCustomPass(MoshRendererFeature.MoshSettings settings)
        {
            this.m_MoshMat = settings.m_MoshMat;
            this.m_SourceTexture = settings.m_SourceTexture;
        }

        public void Setup(ScriptableRenderer renderer)
        {
            this.currentTarget = renderer.cameraColorTarget;
        }

        private void ReleaseBuffer(RenderTexture buffer)
        {
            if (buffer != null) RenderTexture.ReleaseTemporary(buffer);
        }

        private RenderTexture NewWorkBuffer(RenderTexture source)
        {
            return RenderTexture.GetTemporary(source.width, source.height);
        }

        private RenderTexture NewDispBuffer(RenderTexture source)
        {
            var rt = RenderTexture.GetTemporary(
                source.width / m_BlockSize,
                source.height / m_BlockSize,
                //In linear color space, set GL.sRGBWrite before using Blit, to make sure the sRGB-to-linear color conversion is what you expect.
                0, RenderTextureFormat.ARGBHalf
            );
            rt.filterMode = FilterMode.Point;
            return rt;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_MoshMat == null)
                return;
            
            if(!Application.isPlaying)
                return;
            
            var camera = renderingData.cameraData.camera;
            if (camera.cameraType != CameraType.Game)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("MoshEffect");

            if (MoshController.MoshSequenceIndex == 0)
            {
                // Initialize and update the working buffer with the current frame.
                ReleaseBuffer(m_WorkBuffer);
                m_WorkBuffer = NewWorkBuffer(m_SourceTexture);
                //If you don't provide mat while blitting, Unity uses a default material.
                cmd.Blit(currentTarget, m_WorkBuffer);
                cmd.Blit(currentTarget,currentTarget);
                m_LastFrame = 0;
                    
            }
            else if (MoshController.MoshSequenceIndex == 1)
            {
                // Initialize the displacement buffer.
                ReleaseBuffer(m_DispBuffer);
                m_DispBuffer = NewDispBuffer(m_SourceTexture);

                //If the value is -1, Unity draws all the passes in mat. Otherwise, Unity draws only the pass you set pass to, The default value is -1.
                //Initializes the displacement buffer with the first pass of the mosh material. Which does nothing
                cmd.Blit(null, m_DispBuffer, m_MoshMat, 0);

                // Simply blit the working buffer because motion vectors might not be ready (because of sudden camera pos change) TODO
                cmd.Blit(m_WorkBuffer, currentTarget);
                MoshController.MoshSequenceIndex++;
            }
            else
            {
                // Final step: apply effect cont.
                if (m_LastFrame > 1)
                {
                    // Update the displacement buffer with the adding the second pass of the mosh material.
                    var newDisp = NewDispBuffer(m_SourceTexture);
                    cmd.Blit(m_DispBuffer, newDisp, m_MoshMat, 1);
                    ReleaseBuffer(m_DispBuffer);
                    m_DispBuffer = newDisp;

                    // Moshing!
                    m_MoshMat.SetTexture(s_WorkTex, m_WorkBuffer);
                    m_MoshMat.SetTexture(s_DispTex, m_DispBuffer);

                    //Get the current view texture, and blit it with itself by adding the mosh mat with the third pass (which has previous frame 
                    //texture, and displaced texture.
                    var newWork = NewWorkBuffer(m_SourceTexture);
                    cmd.Blit(currentTarget, newWork, m_MoshMat, 2);

                    // Update the working buffer with the result.
                    ReleaseBuffer(m_WorkBuffer);
                    m_WorkBuffer = newWork;
                }

                m_LastFrame++;

                // Blit the result to the cam fully
                cmd.Blit(m_WorkBuffer, currentTarget);
            }

            m_BlockSize = (int)m_MoshMat.GetFloat(s_BlockSize) == 0 ? 2 : (int)m_MoshMat.GetFloat(s_BlockSize);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}