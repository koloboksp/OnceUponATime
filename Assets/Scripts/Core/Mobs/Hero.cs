using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.Items;
using Assets.Scripts.Core.Mobs.Helpers;
using Assets.Scripts.Core.Mobs.HeroMisc;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public enum HeroAttackType
    {
        MainWeapon,
        AdditionalWeapon,
    }

    public enum WeaponItemUsingType
    {
        Melee,
        Ranged, 
        Aiming,
    }
    public enum WeaponItemPlacement
    {
        InBag = 0,
        InRightHand = 10,
        InLeftHand = 20,
        OnHand = 30,
        OnLeg = 40,
    }
    public enum HeroAvailableStrikes
    {
        ItemInHand1,
        ItemInHand2,
        Leg,
        Hand,

        Magic,
        Throw,
        Aiming,
    }

    public class Hero : GroundMovementCharacter
    {
        public event Action<Hero> OnAttackStarted;
        public event Action<Hero> OnAttackWaitForNext;
        public event Action<Hero> OnAttackComplete;
        public event Action<Hero> OnAttackAbort;

        public event Action<Hero> OnPrepareToAttackStateChanged;


        public event Action<Hero> OnItemInWeaponSlotsChanged;
        public event Action<Hero, Collider2D> OnTriggeredSomething;

        public HeroMind Mind;
        public HeroView ViewPart;
 
      
        public Item DefaultRightHandItemPrefab;
       // public List<Item> WeaponInInventory;

        readonly List<InventoryItem> mInventoryItems = new List<InventoryItem>();
        readonly List<HeroWeaponSlot> mMainWeaponSlots = new List<HeroWeaponSlot>();

        readonly HeroPrepareToAttackOperation mPrepareToAttackOperation = new HeroPrepareToAttackOperation();
        public HeroPrepareToAttackOperation PrepareToAttackOperation => mPrepareToAttackOperation;

        readonly HeroAttackOperation mAttackOperation = new HeroAttackOperation();
        public HeroAttackOperation AttackOperation => mAttackOperation;

        public IEnumerable<InventoryItem> InventoryItems => mInventoryItems;
        public IEnumerable<HeroWeaponSlot> MainWeaponSlots => mMainWeaponSlots;

        readonly List<Collider2D> mTriggeredItems = new List<Collider2D>();

        void Awake()
        {
            mMainWeaponSlots.Add(new HeroWeaponSlot(WeaponItemPlacement.InLeftHand, ViewPart.LeftHand));
            mMainWeaponSlots.Add(new HeroWeaponSlot(WeaponItemPlacement.InRightHand, ViewPart.RightHand));
            mMainWeaponSlots.Add(new HeroWeaponSlot(WeaponItemPlacement.OnHand, ViewPart.RightHand));
            mMainWeaponSlots.Add(new HeroWeaponSlot(WeaponItemPlacement.OnLeg, ViewPart.RightHand));

            mAttackOperation.OnProcess = AttackOperation_OnProcess;
            mAttackOperation.OnComplete = AttackOperation_OnComplete;
            mAttackOperation.OnAbort = AttackOperation_OnAbort;
            mAttackOperation.OnInWaitPart = AttackOperation_OnInWaitingPart;

            mPrepareToAttackOperation.OnProcess = mPrepareToAttackOperation_OnProcess;
            mPrepareToAttackOperation.OnComplete = mPrepareToAttackOperation_OnComplete;
        }
        void Start()
        {
            
            //foreach (var item in WeaponInInventory)
            //    AddNewItemInInventory(item);

            if (mInventoryItems.FirstOrDefault(i => i.ItemPrefab == DefaultRightHandItemPrefab) == null)
            {
                var inventoryItem = AddNewItemInInventory(DefaultRightHandItemPrefab);
                EquipMainWeapon(inventoryItem.ItemInstance);
            }
        }

        internal InventoryItem AddNewItemInInventory(Item itemPrefab)
        {
            var inventoryItem = mInventoryItems.FirstOrDefault(ii => ii.ItemPrefab == itemPrefab);
            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem(itemPrefab);
                mInventoryItems.Add(inventoryItem);
                inventoryItem.ItemInstance.Owner = this;
            }
            else
                inventoryItem.AddItem(itemPrefab);
            
            return inventoryItem;
        }

        void Update()
        {  
            base.InnerUpdate();

            for (var index = 0; index < mTriggeredItems.Count; index++)
            {
                var collider2d = mTriggeredItems[index];
                if (OnTriggeredSomething != null)
                    OnTriggeredSomething(this, collider2d);
            }
            mTriggeredItems.Clear();
     
            mAttackOperation.Process(Time.deltaTime);
            mPrepareToAttackOperation.Process(Time.deltaTime);
        }

        void OnTriggerEnter2D(Collider2D collider2d)
        {
            if (!mTriggeredItems.Contains(collider2d))
                mTriggeredItems.Add(collider2d);
        }

        internal void Attack(HeroFightLogic.AttackCombination ac)
        {
            if (ac.UsingType == WeaponItemUsingType.Melee)
            {
                mAttackOperation.MeleeExecute(this, ac.AttackType, ac.Movement, ac.BlockMovement, ac.WeaponSlot, ac.DealDamageTimeInterval,
                    ac.AttackPartTime, ac.WaitPartTime);

                if (OnAttackStarted != null)
                    OnAttackStarted(this);
            }
            else if (ac.UsingType == WeaponItemUsingType.Ranged)
            {
                mAttackOperation.RangedExecute(this, ac.AttackType, ac.Movement, ac.BlockMovement, ac.WeaponSlot, ac.ShotTime, 10.0f,
                    ac.AttackPartTime, ac.WaitPartTime);

                if (OnAttackStarted != null)
                    OnAttackStarted(this);
            }
        }

        private HeroFightLogic.AttackCombination mAC;

        internal void PrepareToAttack(HeroFightLogic.AttackCombination ac, Vector3 value)
        {
            mAC = ac;

            mPrepareToAttackOperation.Execute(this, mAC.Movement, ac.WeaponSlot, value, 0.5f, float.MaxValue);

            if (OnPrepareToAttackStateChanged != null)
                OnPrepareToAttackStateChanged(this);
        }


       
        internal void UpdatePrepareToAttackState(HeroAttackType mainWeapon, Vector3 value)
        {
            mPrepareToAttackOperation.SetValue(value);
        }
        internal void CompletePrepareToAttackState(HeroAttackType mainWeapon, Vector3 value)
        {
            mPrepareToAttackOperation.ManualComplete();
        }
        void mPrepareToAttackOperation_OnProcess(Operation sender)
        {
      //      if (OnPrepareToAttackStateChanged != null)
      //          OnPrepareToAttackStateChanged(this);
        }

        void mPrepareToAttackOperation_OnComplete(Operation sender)
        {
            if (OnPrepareToAttackStateChanged != null)
                OnPrepareToAttackStateChanged(this);

            mAttackOperation.RangedExecute(this, mAC.AttackType, mAC.Movement, mAC.BlockMovement, mAC.WeaponSlot, mAC.ShotTime, mPrepareToAttackOperation.Value.y,
                mAC.AttackPartTime, mAC.WaitPartTime);

        
            if (OnAttackStarted != null)
                OnAttackStarted(this);
        }

        public void AbortAttack()
        {
            mAttackOperation.Abort();
        }

        void AttackOperation_OnProcess(Operation sender)
        {
          
        }

        void AttackOperation_OnInWaitingPart(AttackOperation sender)
        {
            if (OnAttackWaitForNext != null)
                OnAttackWaitForNext(this);
        }

        void AttackOperation_OnComplete(Operation sender)
        {
            if (OnAttackComplete != null)
                OnAttackComplete(this);
        }

        void AttackOperation_OnAbort(Operation sender)
        {
            if (OnAttackAbort != null)
                OnAttackAbort(this);
        }

        public void EquipMainWeapon(Item item)
        {
            InventoryItem inventoryItem = mInventoryItems.FirstOrDefault(ii => ii.ItemInstance == item);

            Mind.EquipMainWeapon(inventoryItem);


            if (OnItemInWeaponSlotsChanged != null)
                OnItemInWeaponSlotsChanged(this);
        }

        public void UseOnYourself(Item item)
        {
            if (item is HealthBonusItem)
            {
                var healthBonus = item as HealthBonusItem;
                Treat(this, new TreatmentInfo(healthBonus.Power));

                var inventoryItem = mInventoryItems.Find(i => i.ItemInstance == item);
                mInventoryItems.Remove(inventoryItem);
            }
        }

        public Transform GetPoint(string tagName)
        {
            if (tagName == "Neck")
                return ViewPart.Neck;

            return null;
        }

        protected override void DeathOperation_OnComplete(Operation sender)
        {
            
        }

        public void ClearInventory()
        {
            foreach (var weaponSlot in mMainWeaponSlots)
                weaponSlot.ChangeItem(null);

            mInventoryItems.Clear();
        }

        public void Translate(Vector3 point)
        {
            transform.position = point;
        }


        
    }
}