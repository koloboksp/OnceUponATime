using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.Items;
using Assets.Scripts.Core.Mobs;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class CheckPointData
    {
        public class HeroWeaponSlotInfo
        {
            public Item ItemPrefab;
            public WeaponItemPlacement Placement;

            public HeroWeaponSlotInfo(WeaponItemPlacement placement, Item itemPrefab)
            {
                Placement = placement;
                ItemPrefab = itemPrefab;
            }
        }
        public float HeroLives;
        public List<Item> HeroInventoryItems;
        public List<HeroWeaponSlotInfo> HeroWeaponSlotInfos;
        public Vector3 Position;
        public Direction Direction;

    }

    public class CheckPoint : MonoBehaviour, IDamageable
    {
        public event Action<CheckPoint> OnActivate;

        public Transform WobbleEffectRoot;
        CheckPointData mData;

        bool mActivated;
        float mTimer;
        
        public virtual void TakeDamage(object sender, DamageInfo damageInfo)
        {
            if (!mActivated)
            {
                mActivated = true;
                var hero = Level.ActiveLevel.Hero;


                mData = CollectData(hero);

                if (OnActivate != null)
                    OnActivate(this);

            }
        }

        public CheckPointData Data
        {
            get
            {
                return mData;
            }
        }

        void Update()
        {
            if (mActivated)
            {
                mTimer += Time.deltaTime;
                WobbleEffectRoot.localRotation = Quaternion.Euler(0,0,Mathf.Sin(mTimer* 4) * 20) * Quaternion.Euler(0, Mathf.Clamp01(mTimer / 1) * Mathf.PI * Mathf.Rad2Deg, 0);
            }      
        }
        
        public static CheckPointData CollectData(Hero hero)
        {
            CheckPointData data = new CheckPointData();

            data.Position = hero.transform.position;
            data.Direction = hero.Direction;
            data.HeroLives = hero.Lives;
           
            return data;
        }
    }
}