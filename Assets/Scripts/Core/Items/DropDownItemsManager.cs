using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Core.Items
{

    public enum DropDownItemsLevel
    {
        Nothing = 0,
        CoinsAndMomentBonuses20 = 10,
        CoinsAndMomentBonuses50 = 20,
    }

    [Serializable]
    public class DropDownLevelInfo
    {
        public DropDownItemsLevel Level;
        public int Chance = 100;
        public List<MapItem> Items;
    }

    [CreateAssetMenu(fileName = "DropDownItemsManager", menuName = "Items/DropDownItemsManager", order = 51)]
    public class DropDownItemsManager : ScriptableObject
    { 
        public const string ResourceName = "DropDownItemsManager";

        public List<DropDownLevelInfo> LevelInfos = new List<DropDownLevelInfo>();

        static DropDownItemsManager mInstance;
        public static DropDownItemsManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    var obj = Resources.Load(ResourceName);
                    if (obj == null)
                        throw new Exception($"Resource '{ResourceName}' not created.");

                    mInstance = obj as DropDownItemsManager;
                    if (mInstance == null)
                        throw new Exception($"Resource '{ResourceName}'  is not '{nameof(DropDownItemsManager)}'.");
                }
                return mInstance;
            }
        }

        public MapItem GenerateItem(DropDownItemsLevel level, Vector3 transformPosition)
        {
            var levelInfo = LevelInfos.Find(i => i.Level == level);

            if (levelInfo != null)
            {
                var changeThrow = Random.Range(0, 100);

                if (changeThrow < levelInfo.Chance)
                {
                    if (levelInfo.Items.Count > 0)
                    {
                        var mapItem = levelInfo.Items[Random.Range(0, levelInfo.Items.Count)];
                        var mapItemInstance = Object.Instantiate(mapItem, transformPosition, Quaternion.identity);

                        GameObject dropDownEffectObj = new GameObject("DropDownEffect");
                        dropDownEffectObj.layer = 23;
                        dropDownEffectObj.transform.position = transformPosition;
                        var dropDownItemEffect = dropDownEffectObj.AddComponent<DropDownMapItemEffect>();
                        dropDownItemEffect.Target = mapItemInstance;

                        return mapItemInstance;
                    }
                }
            }

            return null;
        }
    }
}