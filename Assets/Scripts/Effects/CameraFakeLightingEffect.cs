using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Assets.Scripts.Effects
{
    public class CameraFakeLightingEffect : MonoBehaviour
    {
        Camera mEffectCamera;
        RenderTexture mEffectRenderTexture;
        public Material EffectMaterial;
        Color mDarknessColor = Color.black;

        public Color DarknessColor
        {
            get => mDarknessColor;
            set
            {
                mDarknessColor = value;
                if (mEffectCamera != null)
                {
                    mEffectCamera.backgroundColor = mDarknessColor;
                }
            }
        }

        void OnEnable()
        {
            var srcCamera = gameObject.GetComponent<Camera>();
   
            mEffectRenderTexture = new RenderTexture(Screen.width / 16, Screen.height / 16, 0, GraphicsFormat.R8G8B8A8_UNorm);
            mEffectRenderTexture.name = "FakeLighting";
            mEffectRenderTexture.autoGenerateMips = false;
            
            var cameraObj = new GameObject("FakeLightEffectCamera");
            cameraObj.transform.SetParent(transform);
            cameraObj.transform.localPosition = Vector3.zero;
            cameraObj.transform.localRotation = Quaternion.identity;
            mEffectCamera = cameraObj.AddComponent<Camera>();
            mEffectCamera.fieldOfView = srcCamera.fieldOfView;
            mEffectCamera.nearClipPlane = srcCamera.nearClipPlane;
            mEffectCamera.farClipPlane = srcCamera.farClipPlane;
            mEffectCamera.cullingMask = 1 << 26;
            mEffectCamera.clearFlags = CameraClearFlags.Color;
            mEffectCamera.backgroundColor = mDarknessColor;
            mEffectCamera.targetTexture = mEffectRenderTexture;

            EffectMaterial.SetTexture("_LightingTex", mEffectRenderTexture);
        }

        void OnDisable()
        {
            Destroy(mEffectCamera.gameObject);
            Destroy(mEffectRenderTexture);
        }
      
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, EffectMaterial);
        }  
    }
}