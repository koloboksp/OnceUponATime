using System;
using System.Collections.Generic;
using Assets.Scripts.Core.Items;
using UnityEngine;

namespace Assets.Scripts.UI
{
    [CreateAssetMenu(fileName = "UIItemViewInfoManager", menuName = "UI/UIItemViewInfoManager", order = 51)]
    public class UIItemViewInfoManager : ScriptableObject
    {
        public const string ResourceName = "UIItemViewInfoManager";

        public List<UIItemViewInfo> ItemsInfos = new List<UIItemViewInfo>();

        public UIItemViewInfo GetInfo(Item item)
        {
            for (var iIndex = 0; iIndex < ItemsInfos.Count; iIndex++)
            {
                var itemViewInfo = ItemsInfos[iIndex];
                if (itemViewInfo.Target == item)
                    return itemViewInfo;
            }

            return null;
        }

        static UIItemViewInfoManager mInstance;
        public static UIItemViewInfoManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    var obj = Resources.Load(ResourceName) ;
                    if (obj == null)
                        throw new Exception($"Resource '{ResourceName}' not created.");

                    mInstance = obj as UIItemViewInfoManager;
                    if (mInstance == null)
                        throw new Exception($"Resource '{ResourceName}'  is not '{nameof(UIItemViewInfoManager)}'.");
                }
                return mInstance;
            }
        }
        
    }
}