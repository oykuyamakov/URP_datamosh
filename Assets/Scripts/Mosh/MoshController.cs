using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Mosh
{
    public class MoshController : MonoBehaviour
    {
        #region Mosh Manipulation Variables

        [SerializeField] [Tooltip("Size of Macroblock.")]
        public int m_BlockSize = 16;
        
        public int BlockSize
        {
            get => Mathf.Clamp(m_BlockSize,0, 4);
            set => m_BlockSize = value;
        }
        
        [SerializeField, Range(0, 2)] [Tooltip("Scale factor for velocity vectors.")]
        private float m_VelocityVectorScale = 0.8f;

        public float VelocityVectorScale
        {
            get => m_VelocityVectorScale;
            set => m_VelocityVectorScale = value;
        }

        [SerializeField, Range(0, 1)] [Tooltip("The larger value makes the stronger noise.")]
        private float m_Entropy = 0.5f;

        public float Entropy
        {
            get => m_Entropy;
            set => m_Entropy = value;
        }
        
        [SerializeField, Range(0.5f, 4.0f)]
        private float m_NoiseContrast = 1;
        
        /// Contrast of stripe-shaped noise.
        public float NoiseContrast
        {
            get => m_NoiseContrast;
            set => m_NoiseContrast = value;
        }

        [SerializeField, Range(0, 2)] 
        float m_Diffusion = 0.4f;
        
        /// Amount of random displacement.
        public float Diffusion
        {
            get => m_Diffusion;
            set => m_Diffusion = value;
        }

        #endregion

        [SerializeField]
        private Material m_Material;
        
        [SerializeField]
        private bool m_PulseGlitch;
        
        [SerializeField]
        private float m_Interval = 3.0f;
        private float Interval => Mathf.Max(1.0f / 30, m_Interval);
        
        private CommandBuffer commandBuffer;
        
        private float m_Timer;
        
        public static int MoshSequenceIndex;
        
        void OnEnable()
        {
            m_Material.hideFlags = HideFlags.DontSaveInEditor;

            // TODO
            GetComponent<Camera>().depthTextureMode |=
                DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
            
            GetComponent<Camera>().AddCommandBuffer(CameraEvent.AfterImageEffects, commandBuffer);

            MoshSequenceIndex = 0;
        }

        private IEnumerator Start()
        {
            commandBuffer = new CommandBuffer { name = "Capture Camera View" };
            
            while (true)
            {
                yield return new WaitForSeconds(Interval);

                if (m_PulseGlitch)
                {
                    Glitch();
                }
            }
            // ReSharper disable once IteratorNeverReturns
        }
        
        private void Update()
        {
            m_Material.SetFloat("_BlockSize", m_BlockSize);
            m_Material.SetFloat("_Quality", 1 - m_Entropy);
            m_Material.SetFloat("_Contrast", m_NoiseContrast);
            m_Material.SetFloat("_Velocity", m_VelocityVectorScale);
            m_Material.SetFloat("_Diffusion", m_Diffusion);
        }
        
        /// Start glitching.
        [ContextMenu("Glitch")]
        private void Glitch()
        {
            MoshSequenceIndex = 1;
        }

        /// Stop glitching.
        public void Reset()
        {
            m_PulseGlitch = false;
            MoshSequenceIndex = 0;
        }
    }
}


