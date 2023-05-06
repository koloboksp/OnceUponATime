using System;
using System.Collections.Generic;
using Assets.Scripts.Core.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.UI
{
    [CreateAssetMenu(fileName = "UIItemViewInfoManager", menuName = "UI/UIItemViewInfoManager", order = 51)]
    public class UIItemViewInfoManager : ScriptableObject
    {
        public const string ResourceName = "UIItemViewInfoManager";
        
        private static UIItemViewInfoManager _instance;

        [FormerlySerializedAs("ItemsInfos")] [SerializeField] private List<UIItemViewInfo> _itemsInfos = new List<UIItemViewInfo>();
        
        public UIItemViewInfo GetInfo(Item item)
        {
            for (var iIndex = 0; iIndex < _itemsInfos.Count; iIndex++)
            {
                var itemViewInfo = _itemsInfos[iIndex];
                if (itemViewInfo.Target == item)
                    return itemViewInfo;
            }

            return null;
        }
        
        public static UIItemViewInfoManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = Resources.Load(ResourceName) ;
                    if (obj == null)
                        throw new Exception($"Resource '{ResourceName}' not created.");

                    _instance = obj as UIItemViewInfoManager;
                    if (_instance == null)
                        throw new Exception($"Resource '{ResourceName}'  is not '{nameof(UIItemViewInfoManager)}'.");
                }
                return _instance;
            }
        }
    }
}