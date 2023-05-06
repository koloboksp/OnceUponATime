using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Core
{
    [ExecuteInEditMode]
    public class LevelLighting : MonoBehaviour
    {
        [SerializeField] private LevelLightingSettings _mainLighting;
        [SerializeField] private LevelLightingSettings _transitionLighting;
        private float _transition = 0.0f;
        private Color _fakeLightingDarknessColor;
        private Color _ambientLight;
        private float _maxEnvironmentLightIntensity;

        public Color FakeLightingDarknessColor => _fakeLightingDarknessColor;
        public Color AmbientLight => _ambientLight;

        void Start()
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

        void UpdateLighting()
        {
            if (_transitionLighting != null)
            {
                _fakeLightingDarknessColor = Color.Lerp(_mainLighting.FakeLightingDarknessColor, _transitionLighting.FakeLightingDarknessColor, _transition);
                _ambientLight = Color.Lerp(_mainLighting.AmbientLight, _transitionLighting.AmbientLight, _transition);
                _maxEnvironmentLightIntensity = Mathf.Lerp(_mainLighting.EnvironmentLightIntencity, _transitionLighting.EnvironmentLightIntencity, _transition);
            }
            else
            {
                _fakeLightingDarknessColor = _mainLighting.FakeLightingDarknessColor;
                _ambientLight = _mainLighting.AmbientLight;
                _maxEnvironmentLightIntensity = _mainLighting.EnvironmentLightIntencity;
            }
            
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = AmbientLight;
            
            Shader.SetGlobalFloat("_maxEnvironmentLightIntensity", _maxEnvironmentLightIntensity);
        }
    }
}