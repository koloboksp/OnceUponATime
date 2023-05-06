using System;
using Assets.Scripts.Core.Mobs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core
{
    [CreateAssetMenu(fileName = nameof(LevelDefaultPrefabsLinker), menuName = "Level/" + nameof(LevelDefaultPrefabsLinker), order = 51)]
    public class LevelDefaultPrefabsLinker : ScriptableObject
    {
        [FormerlySerializedAs("UIPrefab")] [SerializeField] private GameObject _uiPrefab;
        [FormerlySerializedAs("HeroPrefab")] [SerializeField] private Hero _heroPrefab;
        [FormerlySerializedAs("CameraControlPrefab")] [SerializeField] private CameraControl _cameraControlPrefab;

        public GameObject UIPrefab => _uiPrefab;
        public Hero HeroPrefab => _heroPrefab;
        public CameraControl CameraControlPrefab => _cameraControlPrefab;

        private static LevelDefaultPrefabsLinker _instance;
        public static LevelDefaultPrefabsLinker Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = Resources.Load(nameof(LevelDefaultPrefabsLinker));
                    if (obj == null)
                        throw new Exception($"Resource '{nameof(LevelDefaultPrefabsLinker)}' not created.");

                    _instance = obj as LevelDefaultPrefabsLinker;
                    if (_instance == null)
                        throw new Exception($"Resource '{nameof(LevelDefaultPrefabsLinker)}' is not '{nameof(LevelDefaultPrefabsLinker)}'.");
                }
                return _instance;
            }
        }
    }
}