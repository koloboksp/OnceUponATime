using UnityEngine;

namespace Assets.Scripts.Core
{
    [ExecuteInEditMode]
    public class LevelSkybox : MonoBehaviour
    {
        public Material SkyBoxMaterial;
        
        void Start()
        {
            RenderSettings.skybox = SkyBoxMaterial;
        }
    }
}