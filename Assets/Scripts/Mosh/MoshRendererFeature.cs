using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mosh
{
    public class MoshRendererFeature : ScriptableRendererFeature
    {
        public MoshSettings settings = new MoshSettings();
        private MoshCustomPass moshPass;
        
        public static bool moshEnabled = false;

        public override void Create()
        {
            moshPass = new MoshCustomPass(settings);
            moshPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(moshEnabled == false)
                return;
            
            
            if (settings.m_MoshMat != null)
            {
                moshPass.ConfigureInput(ScriptableRenderPassInput.Motion);
                renderer.EnqueuePass(moshPass);
            }
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (!moshEnabled)
            {
                return;
            }
            
            
            base.SetupRenderPasses(renderer, in renderingData);
            if (moshPass != null)
            {
                moshPass.Setup(renderer);
            }
        }

        [System.Serializable]
        public class MoshSettings
        {
            public Material m_MoshMat;
            public RenderTexture m_SourceTexture;
        }
    }
}