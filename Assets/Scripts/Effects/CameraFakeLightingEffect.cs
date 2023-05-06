using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

namespace Assets.Scripts.Effects
{
    public class CameraFakeLightingEffect : MonoBehaviour
    {
        private Camera _effectCamera;
        private RenderTexture _effectRenderTexture;
        private Color _darknessColor = Color.black;

        [FormerlySerializedAs("EffectMaterial")] [SerializeField] private Material _effectMaterial;
        
        public Color DarknessColor
        {
            get => _darknessColor;
            set
            {
                _darknessColor = value;
                if (_effectCamera != null)
                {
                    _effectCamera.backgroundColor = _darknessColor;
                }
            }
        }

        private void OnEnable()
        {
            var srcCamera = gameObject.GetComponent<Camera>();
   
            _effectRenderTexture = new RenderTexture(Screen.width / 16, Screen.height / 16, 0, GraphicsFormat.R8G8B8A8_UNorm);
            _effectRenderTexture.name = "FakeLighting";
            _effectRenderTexture.autoGenerateMips = false;
            
            var cameraObj = new GameObject("FakeLightEffectCamera");
            cameraObj.transform.SetParent(transform);
            cameraObj.transform.localPosition = Vector3.zero;
            cameraObj.transform.localRotation = Quaternion.identity;
            _effectCamera = cameraObj.AddComponent<Camera>();
            _effectCamera.fieldOfView = srcCamera.fieldOfView;
            _effectCamera.nearClipPlane = srcCamera.nearClipPlane;
            _effectCamera.farClipPlane = srcCamera.farClipPlane;
            _effectCamera.cullingMask = 1 << 26;
            _effectCamera.clearFlags = CameraClearFlags.Color;
            _effectCamera.backgroundColor = _darknessColor;
            _effectCamera.targetTexture = _effectRenderTexture;

            _effectMaterial.SetTexture("_LightingTex", _effectRenderTexture);
        }

        private void OnDisable()
        {
            Destroy(_effectCamera.gameObject);
            Destroy(_effectRenderTexture);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, _effectMaterial);
        }  
    }
}