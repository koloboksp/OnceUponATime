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
        public Hero Hero;
        public TagHolder Id;

        private CheckPointData mCheckPointDataAtTheStart;
        private readonly List<CheckPoint> mCheckPoints = new List<CheckPoint>();
        private CheckPoint mLastActivatedCheckPoint = null;

        [SerializeField] private LevelLighting _levelLighting;

        private readonly List<HopelessPlace> mHopelessPlaces = new List<HopelessPlace>();

        public Rect Bounds = new Rect(new Vector2(0,0), new Vector2(10,10));

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

            
            mCheckPointDataAtTheStart = CheckPoint.CollectData(heroInstance);
            mCheckPointDataAtTheStart.Position = checkPointPosition;

            heroInstance.OnBeforeDestroy += Hero_OnBeforeDestroy;
            gameObject.GetComponentsInChildren(mCheckPoints);

            foreach (var checkPoint in mCheckPoints)
                checkPoint.OnActivate += CheckPoint_OnBeforeDestroy;

            gameObject.GetComponentsInChildren(mHopelessPlaces);

            foreach (var hopelessPlace in mHopelessPlaces)
                hopelessPlace.OnHeroEnter += HopelessPlace_OnHeroEnter;

            Hero = heroInstance;
        }

        private void HopelessPlace_OnHeroEnter(HopelessPlace obj)
        {
            CameraDarknessEffect.PlungeIntoDarkness(Camera.main, 1.0f, CameraDarknessEffect.DarknessValues.Zero, CameraDarknessEffect.DarknessValues.One, this);
            StartCoroutine(WaitFor());
        }


        private void CheckPoint_OnBeforeDestroy(CheckPoint sender)
        {
            mLastActivatedCheckPoint = sender;
        }

        private void Hero_OnBeforeDestroy(Character sender)
        {
            CameraDarknessEffect.PlungeIntoDarkness(Camera.main, 1.0f, CameraDarknessEffect.DarknessValues.Zero, CameraDarknessEffect.DarknessValues.One, this);
            StartCoroutine(WaitFor());
        }

        private IEnumerator WaitFor()
        {
            yield return new WaitForSeconds(1);

            var checkPointData = (mLastActivatedCheckPoint != null) ? mLastActivatedCheckPoint.Data : mCheckPointDataAtTheStart;
            Hero.transform.position = checkPointData.Position;

            yield return null;

            Hero.ChangeDirection(checkPointData.Direction);
            Hero.Treat(this, new TreatmentInfo(checkPointData.HeroLives - Hero.Lives));
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
            var rtC = transform.position + transform.rotation * (Bounds.center + Bounds.size * new Vector2(0.5f, 0.5f));
            var rbC = transform.position + transform.rotation * (Bounds.center + Bounds.size * new Vector2(0.5f, -0.5f));
            var lbC = transform.position + transform.rotation * (Bounds.center + Bounds.size * new Vector2(-0.5f, -0.5f));
            var tbC = transform.position + transform.rotation * (Bounds.center + Bounds.size * new Vector2(-0.5f, 0.5f));

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(rtC, rbC);
            Gizmos.DrawLine(rbC, lbC);
            Gizmos.DrawLine(lbC, tbC);
            Gizmos.DrawLine(tbC, rtC);
        }
    }
}