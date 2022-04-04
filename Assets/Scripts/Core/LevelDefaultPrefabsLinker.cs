using System;
using Assets.Scripts.Core.Mobs;
using UnityEngine;

namespace Assets.Scripts.Core
{
    [CreateAssetMenu(fileName = nameof(LevelDefaultPrefabsLinker), menuName = "Level/" + nameof(LevelDefaultPrefabsLinker), order = 51)]
    public class LevelDefaultPrefabsLinker : ScriptableObject
    {
        public GameObject UIPrefab;
        public Hero HeroPrefab;
        public CameraControl CameraControlPrefab;
        
        static LevelDefaultPrefabsLinker mInstance;
        public static LevelDefaultPrefabsLinker Instance
        {
            get
            {
                if (mInstance == null)
                {
                    var obj = Resources.Load(nameof(LevelDefaultPrefabsLinker));
                    if (obj == null)
                        throw new Exception($"Resource '{nameof(LevelDefaultPrefabsLinker)}' not created.");

                    mInstance = obj as LevelDefaultPrefabsLinker;
                    if (mInstance == null)
                        throw new Exception($"Resource '{nameof(LevelDefaultPrefabsLinker)}'  is not '{nameof(LevelDefaultPrefabsLinker)}'.");
                }
                return mInstance;
            }
        }
    }
}