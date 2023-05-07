using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.Items;
using Assets.Scripts.Core.Mobs;
using Assets.Scripts.Effects;
using Assets.Scripts.Shared.Tags;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core
{
    public class LevelChangeUserData
    {
        public Guid ExitId;
        public List<Item> HeroInventoryItems;
        public List<CheckPointData.HeroWeaponSlotInfo> HeroWeaponSlotInfos;
        public float HeroLives;
    }

    public class Level : MonoBehaviour
    {
        public static Level ActiveLevel;
       
        private CheckPointData _checkPointDataAtTheStart;
        private readonly List<CheckPoint> _checkPoints = new List<CheckPoint>();
        private CheckPoint _lastActivatedCheckPoint = null;
        private readonly List<HopelessPlace> _hopelessPlaces = new List<HopelessPlace>();

        [FormerlySerializedAs("Hero")] [SerializeField] private Hero _hero;
        [FormerlySerializedAs("Id")] [SerializeField] private TagHolder _id;
        [SerializeField] private LevelLighting _levelLighting;
        [FormerlySerializedAs("Bounds")] [SerializeField] private Rect _bounds = new Rect(new Vector2(0,0), new Vector2(10,10));

        public Hero Hero => _hero; 
        public TagHolder Id => _id; 
        public Rect Bounds => _bounds; 
        public LevelLighting Lighting => _levelLighting;

        public void Awake()
        {
            ActiveLevel = this;
            
            var cameraControlInstance = Instantiate(LevelDefaultPrefabsLinker.Instance.CameraControlPrefab);
            SceneManager.MoveGameObjectToScene(cameraControlInstance.gameObject, gameObject.scene);
            cameraControlInstance.Owner = this;

            var uiInstance = Instantiate(LevelDefaultPrefabsLinker.Instance.UIPrefab);
            SceneManager.MoveGameObjectToScene(uiInstance, gameObject.scene);
            
            var heroInstance = FindObjectOfType<Hero>();
            if (heroInstance == null)
            {
                heroInstance = Instantiate(LevelDefaultPrefabsLinker.Instance.HeroPrefab);
                SceneManager.MoveGameObjectToScene(heroInstance.gameObject, gameObject.scene);
            }

            cameraControlInstance.Target = heroInstance;

            bool enterFound = false;
            Vector3 checkPointPosition = Vector3.zero;
            
            if (SceneChanger.LastInfo != null && SceneChanger.LastInfo.UserData is LevelChangeUserData)
            {
                var lastInfoUserData = SceneChanger.LastInfo.UserData as LevelChangeUserData;
                var findObjectsOfType = FindObjectsOfType<ExitFromLevel>();
                var enterToLevel = findObjectsOfType.FirstOrDefault(e => e.Destination.Name == SceneChanger.LastInfo.FromSceneName);

                heroInstance.Translate(enterToLevel.ExitPoint);
                checkPointPosition = enterToLevel.EnterPoint;

                heroInstance.SetLives(lastInfoUserData.HeroLives);
                heroInstance.ClearInventory();
                foreach (var item in lastInfoUserData.HeroInventoryItems)
                    heroInstance.AddNewItemInInventory(item);
                foreach (var slotInfo in lastInfoUserData.HeroWeaponSlotInfos)
                    heroInstance.EquipMainWeapon(heroInstance.InventoryItems.First(i => i.ItemPrefab == slotInfo.ItemPrefab).ItemInstance);


                heroInstance.Mind.EnterToLevel(enterToLevel);
                enterFound = true;
            }

            if (!enterFound)
            {
                if (Application.isEditor)
                {
                    var heroRespawn = FindObjectOfType<HeroDebugRespawn>();
                    if (heroRespawn != null)
                    {
                        heroInstance.Translate(heroRespawn.transform.position);
                        checkPointPosition = heroRespawn.transform.position;

                        enterFound = true;
                    }
                }     
            }
            if (!enterFound)
            {
                var heroRespawn = FindObjectOfType<HeroRespawn>();
                if (heroRespawn != null)
                {
                    heroInstance.Translate(heroRespawn.transform.position);
                    checkPointPosition = heroRespawn.transform.position;
                }
            }

            
            _checkPointDataAtTheStart = CheckPoint.CollectData(heroInstance);
            _checkPointDataAtTheStart.Position = checkPointPosition;

            heroInstance.OnBeforeDestroy += Hero_OnBeforeDestroy;
            gameObject.GetComponentsInChildren(_checkPoints);

            foreach (var checkPoint in _checkPoints)
                checkPoint.OnActivate += CheckPoint_OnBeforeDestroy;

            gameObject.GetComponentsInChildren(_hopelessPlaces);

            foreach (var hopelessPlace in _hopelessPlaces)
                hopelessPlace.OnHeroEnter += HopelessPlace_OnHeroEnter;

            _hero = heroInstance;
        }

        private void HopelessPlace_OnHeroEnter(HopelessPlace obj)
        {
            CameraDarknessEffect.PlungeIntoDarkness(Camera.main, 1.0f, CameraDarknessEffect.DarknessValues.Zero, CameraDarknessEffect.DarknessValues.One, this);
            StartCoroutine(WaitFor());
        }


        private void CheckPoint_OnBeforeDestroy(CheckPoint sender)
        {
            _lastActivatedCheckPoint = sender;
        }

        private void Hero_OnBeforeDestroy(Character sender)
        {
            CameraDarknessEffect.PlungeIntoDarkness(Camera.main, 1.0f, CameraDarknessEffect.DarknessValues.Zero, CameraDarknessEffect.DarknessValues.One, this);
            StartCoroutine(WaitFor());
        }

        private IEnumerator WaitFor()
        {
            yield return new WaitForSeconds(1);

            var checkPointData = (_lastActivatedCheckPoint != null) ? _lastActivatedCheckPoint.Data : _checkPointDataAtTheStart;
            _hero.transform.position = checkPointData.Position;

            yield return null;

            _hero.ChangeDirection(checkPointData.Direction);
            _hero.Treat(this, new TreatmentInfo(checkPointData.HeroLives - _hero.Lives));
          //  foreach (var item in checkPointData.HeroInventoryItems)
          //      Hero.AddNewItemInInventory(item);
          //  foreach (var slotInfo in checkPointData.HeroWeaponSlotInfos)
          //      Hero.EquipMainWeapon(slotInfo.Placement,
          //          Hero.InventoryItems.First(i => i.ItemPrefab == slotInfo.Item).ItemInstance);

            CameraDarknessEffect.PlungeIntoDarkness(Camera.main, 1.0f, CameraDarknessEffect.DarknessValues.One, CameraDarknessEffect.DarknessValues.Zero, this);        
        }

        private void FixedUpdate()
        {
            OrderedFixedUpdateManager.FixedUpdate();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var rtC = transform.position + transform.rotation * (_bounds.center + _bounds.size * new Vector2(0.5f, 0.5f));
            var rbC = transform.position + transform.rotation * (_bounds.center + _bounds.size * new Vector2(0.5f, -0.5f));
            var lbC = transform.position + transform.rotation * (_bounds.center + _bounds.size * new Vector2(-0.5f, -0.5f));
            var tbC = transform.position + transform.rotation * (_bounds.center + _bounds.size * new Vector2(-0.5f, 0.5f));

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(rtC, rbC);
            Gizmos.DrawLine(rbC, lbC);
            Gizmos.DrawLine(lbC, tbC);
            Gizmos.DrawLine(tbC, rtC);
        }
    }
}