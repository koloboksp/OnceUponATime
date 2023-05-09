using Assets.Scripts.Effects;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core
{
    public class LevelLightingSettings : MonoBehaviour
    {
        [FormerlySerializedAs("AmbientLight")] [SerializeField] private Color _ambientLight = Color.gray;
        [FormerlySerializedAs("FakeLightingDarknessColor")] [SerializeField] private Color _fakeLightingDarknessColor = Color.black; 
        [FormerlySerializedAs("EnvironmentLightIntencity")] [SerializeField] private float _environmentLightIntensity = 10000.0f;
        [SerializeField] private float _mainLightIntensity = 1.0f;
        [SerializeField] private Color _clearColor = Color.black;

        public Color AmbientLight => _ambientLight;
        public Color FakeLightingDarknessColor => _fakeLightingDarknessColor;
        public float EnvironmentLightIntensity => _environmentLightIntensity;
        public float MainLightIntensity => _mainLightIntensity;
        public Color ClearColor => _clearColor;
    }
}