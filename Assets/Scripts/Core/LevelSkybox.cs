using UnityEngine;

namespace Assets.Scripts.Core
{
    [ExecuteInEditMode]
    public class LevelSkybox : MonoBehaviour
    {
        public Material SkyBoxMaterial;

        private void Start()
        {
            RenderSettings.skybox = SkyBoxMaterial;
        }
    }
}