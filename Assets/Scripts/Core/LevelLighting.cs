using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Core
{
    [ExecuteInEditMode]
    public class LevelLighting : MonoBehaviour
    {
        private float _transition = 0.0f;
        private Color _fakeLightingDarknessColor;
        private Color _ambientLight;
        private float _maxEnvironmentLightIntensity;
        private float _mainLightIntensity;
        private Color _clearColor;

        [SerializeField] private LevelLightingSettings _mainLighting;
        [SerializeField] private LevelLightingSettings _transitionLighting;
        [SerializeField] private Light _mainLight;

        public Color FakeLightingDarknessColor => _fakeLightingDarknessColor;
        public Color AmbientLight => _ambientLight;

        private void Start()
        {
            UpdateLighting();
        }

        public void Blend(LevelLightingSettings from, LevelLightingSettings to, float transition)
        {
            _mainLighting = from;
            _transitionLighting = to;
            
            _transition = transition;
            if (_transition >= 1)
            {
                _mainLighting = to;
                _transitionLighting = null;
            }
            else  if (_transition <= 0)
            {
                _mainLighting = from;
                _transitionLighting = null;
            }
            
            UpdateLighting();
        }

        private void UpdateLighting()
        {
            if (_transitionLighting != null)
            {
                _fakeLightingDarknessColor = Color.Lerp(_mainLighting.FakeLightingDarknessColor, _transitionLighting.FakeLightingDarknessColor, _transition);
                _ambientLight = Color.Lerp(_mainLighting.AmbientLight, _transitionLighting.AmbientLight, _transition);
                _maxEnvironmentLightIntensity = Mathf.Lerp(_mainLighting.EnvironmentLightIntensity, _transitionLighting.EnvironmentLightIntensity, _transition);
                _mainLightIntensity = Mathf.Lerp(_mainLighting.MainLightIntensity, _transitionLighting.MainLightIntensity, _transition);
                _clearColor = Color.Lerp(_mainLighting.ClearColor, _transitionLighting.ClearColor, _transition);
            }
            else
            {
                _fakeLightingDarknessColor = _mainLighting.FakeLightingDarknessColor;
                _ambientLight = _mainLighting.AmbientLight;
                _maxEnvironmentLightIntensity = _mainLighting.EnvironmentLightIntensity;
                _mainLightIntensity = _mainLighting.MainLightIntensity;
                _clearColor = _mainLighting.ClearColor;
            }
            
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = AmbientLight;
            _mainLight.intensity = _mainLightIntensity;
            if(Camera.main != null)
                Camera.main.backgroundColor = _clearColor;
            
            Shader.SetGlobalFloat("_maxEnvironmentLightIntensity", _maxEnvironmentLightIntensity);
        }
    }
}