using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.Items;
using Assets.Scripts.Core.Mobs;
using UnityEngine;
using UnityEngine.Serialization;

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

        private CheckPointData _data;

        private bool _activated;
        private float _timer;
        
        [FormerlySerializedAs("WobbleEffectRoot")] [SerializeField] private Transform _wobbleEffectRoot;
        
        public virtual void TakeDamage(object sender, DamageInfo damageInfo)
        {
            if (!_activated)
            {
                _activated = true;
                var hero = Level.ActiveLevel.Hero;


                _data = CollectData(hero);

                OnActivate?.Invoke(this);

            }
        }

        public CheckPointData Data
        {
            get
            {
                return _data;
            }
        }

        private void Update()
        {
            if (_activated)
            {
                _timer += Time.deltaTime;
                _wobbleEffectRoot.localRotation = Quaternion.Euler(0,0,Mathf.Sin(_timer* 4) * 20) * Quaternion.Euler(0, Mathf.Clamp01(_timer / 1) * Mathf.PI * Mathf.Rad2Deg, 0);
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