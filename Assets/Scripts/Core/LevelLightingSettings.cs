using Assets.Scripts.Effects;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Core
{
    [ExecuteInEditMode]
    public class LevelLightingSettings : MonoBehaviour
    {
        public Color AmbientLight = Color.gray;
        public Color FakeLightingDarknessColor = Color.black;

        public Material SkyBoxMaterial;

        void Start()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = AmbientLight;

            RenderSettings.skybox = SkyBoxMaterial;
        }
    }
}