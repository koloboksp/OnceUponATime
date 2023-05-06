using Assets.Scripts.Effects;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Core
{
    public class LevelLightingSettings : MonoBehaviour
    {
        public Color AmbientLight = Color.gray;
        public Color FakeLightingDarknessColor = Color.black;
        public float EnvironmentLightIntencity = 10000.0f;
    }
}