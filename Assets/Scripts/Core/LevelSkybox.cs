using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core
{
    [ExecuteInEditMode]
    public class LevelSkybox : MonoBehaviour
    {
        [FormerlySerializedAs("SkyBoxMaterial")] [SerializeField] private Material _skyBoxMaterial;

        private void Start()
        {
            RenderSettings.skybox = _skyBoxMaterial;
        }
    }
}