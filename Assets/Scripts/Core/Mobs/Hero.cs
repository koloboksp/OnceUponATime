using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.Items;
using Assets.Scripts.Core.Mobs.Helpers;
using Assets.Scripts.Core.Mobs.HeroMisc;
using UnityEngine;
using UnityEngine.Serialization;

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

        [FormerlySerializedAs("Mind")] [SerializeField] private HeroMind _mind;
        [FormerlySerializedAs("ViewPart")] [SerializeField] private HeroView _viewPart;
        
        [FormerlySerializedAs("DefaultRightHandItemPrefab")] [SerializeField] private Item _defaultRightHandItemPrefab;
       
        private readonly List<InventoryItem> _inventoryItems = new List<InventoryItem>();
        private readonly List<HeroWeaponSlot> _mainWeaponSlots = new List<HeroWeaponSlot>();

        private readonly HeroPrepareToAttackOperation _prepareToAttackOperation = new HeroPrepareToAttackOperation();
        
        private readonly HeroAttackOperation _attackOperation = new HeroAttackOperation();
        private readonly List<Collider2D> _triggeredItems = new List<Collider2D>();
        
        public HeroPrepareToAttackOperation PrepareToAttackOperation => _prepareToAttackOperation;
        public HeroAttackOperation AttackOperation => _attackOperation;
        public IEnumerable<InventoryItem> InventoryItems => _inventoryItems;
        public IEnumerable<HeroWeaponSlot> MainWeaponSlots => _mainWeaponSlots;
        public HeroView ViewPart => _viewPart;
        public HeroMind Mind => _mind;
        
        private void Awake()
        {
            _mainWeaponSlots.Add(new HeroWeaponSlot(WeaponItemPlacement.InLeftHand, _viewPart.LeftHand));
            _mainWeaponSlots.Add(new HeroWeaponSlot(WeaponItemPlacement.InRightHand, _viewPart.RightHand));
            _mainWeaponSlots.Add(new HeroWeaponSlot(WeaponItemPlacement.OnHand, _viewPart.RightHand));
            _mainWeaponSlots.Add(new HeroWeaponSlot(WeaponItemPlacement.OnLeg, _viewPart.RightHand));

            _attackOperation.OnProcess = AttackOperation_OnProcess;
            _attackOperation.OnComplete = AttackOperation_OnComplete;
            _attackOperation.OnAbort = AttackOperation_OnAbort;
            _attackOperation.OnInWaitPart = AttackOperation_OnInWaitingPart;

            _prepareToAttackOperation.OnProcess = mPrepareToAttackOperation_OnProcess;
            _prepareToAttackOperation.OnComplete = mPrepareToAttackOperation_OnComplete;
        }

        private void Start()
        {
            //foreach (var item in WeaponInInventory)
            //    AddNewItemInInventory(item);

            if (_inventoryItems.FirstOrDefault(i => i.ItemPrefab == _defaultRightHandItemPrefab) == null)
            {
                var inventoryItem = AddNewItemInInventory(_defaultRightHandItemPrefab);
                EquipMainWeapon(inventoryItem.ItemInstance);
            }
        }

        internal InventoryItem AddNewItemInInventory(Item itemPrefab)
        {
            var inventoryItem = _inventoryItems.FirstOrDefault(ii => ii.ItemPrefab == itemPrefab);
            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem(itemPrefab);
                _inventoryItems.Add(inventoryItem);
                inventoryItem.ItemInstance.Owner = this;
            }
            else
                inventoryItem.AddItem(itemPrefab);
            
            return inventoryItem;
        }

        private void Update()
        {  
            base.InnerUpdate();

            for (var index = 0; index < _triggeredItems.Count; index++)
            {
                var collider2d = _triggeredItems[index];
                OnTriggeredSomething?.Invoke(this, collider2d);
            }
            _triggeredItems.Clear();
     
            _attackOperation.Process(Time.deltaTime);
            _prepareToAttackOperation.Process(Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D collider2d)
        {
            if (!_triggeredItems.Contains(collider2d))
                _triggeredItems.Add(collider2d);
        }

        internal void Attack(HeroFightLogic.AttackCombination ac)
        {
            if (ac.UsingType == WeaponItemUsingType.Melee)
            {
                _attackOperation.MeleeExecute(this, ac.AttackType, ac.Movement, ac.BlockMovement, ac.WeaponSlot, ac.DealDamageTimeInterval,
                    ac.AttackPartTime, ac.WaitPartTime);

                OnAttackStarted?.Invoke(this);
            }
            else if (ac.UsingType == WeaponItemUsingType.Ranged)
            {
                _attackOperation.RangedExecute(this, ac.AttackType, ac.Movement, ac.BlockMovement, ac.WeaponSlot, ac.ShotTime, 10.0f,
                    ac.AttackPartTime, ac.WaitPartTime);

                OnAttackStarted?.Invoke(this);
            }
        }

        private HeroFightLogic.AttackCombination mAC;

        internal void PrepareToAttack(HeroFightLogic.AttackCombination ac, Vector3 value)
        {
            mAC = ac;

            _prepareToAttackOperation.Execute(this, mAC.Movement, ac.WeaponSlot, value, 0.5f, float.MaxValue);

            OnPrepareToAttackStateChanged?.Invoke(this);
        }
        
        internal void UpdatePrepareToAttackState(HeroAttackType mainWeapon, Vector3 value)
        {
            _prepareToAttackOperation.SetValue(value);
        }
        internal void CompletePrepareToAttackState(HeroAttackType mainWeapon, Vector3 value)
        {
            _prepareToAttackOperation.ManualComplete();
        }

        private void mPrepareToAttackOperation_OnProcess(Operation sender)
        {
      //      if (OnPrepareToAttackStateChanged != null)
      //          OnPrepareToAttackStateChanged(this);
        }

        private void mPrepareToAttackOperation_OnComplete(Operation sender)
        {
            OnPrepareToAttackStateChanged?.Invoke(this);

            _attackOperation.RangedExecute(this, mAC.AttackType, mAC.Movement, mAC.BlockMovement, mAC.WeaponSlot, mAC.ShotTime, _prepareToAttackOperation.Value.y,
                mAC.AttackPartTime, mAC.WaitPartTime);


            OnAttackStarted?.Invoke(this);
        }

        public void AbortAttack()
        {
            _attackOperation.Abort();
        }

        private void AttackOperation_OnProcess(Operation sender)
        {
          
        }

        private void AttackOperation_OnInWaitingPart(AttackOperation sender)
        {
            OnAttackWaitForNext?.Invoke(this);
        }

        private void AttackOperation_OnComplete(Operation sender)
        {
            OnAttackComplete?.Invoke(this);
        }

        private void AttackOperation_OnAbort(Operation sender)
        {
            OnAttackAbort?.Invoke(this);
        }

        public void EquipMainWeapon(Item item)
        {
            InventoryItem inventoryItem = _inventoryItems.FirstOrDefault(ii => ii.ItemInstance == item);

            _mind.EquipMainWeapon(inventoryItem);


            OnItemInWeaponSlotsChanged?.Invoke(this);
        }

        public void UseOnYourself(Item item)
        {
            if (item is HealthBonusItem)
            {
                var healthBonus = item as HealthBonusItem;
                Treat(this, new TreatmentInfo(healthBonus.Power));

                var inventoryItem = _inventoryItems.Find(i => i.ItemInstance == item);
                _inventoryItems.Remove(inventoryItem);
            }
        }

        public Transform GetPoint(string tagName)
        {
            if (tagName == "Neck")
                return _viewPart.Neck;

            return null;
        }

        protected override void DeathOperation_OnComplete(Operation sender)
        {
            
        }

        public void ClearInventory()
        {
            foreach (var weaponSlot in _mainWeaponSlots)
                weaponSlot.ChangeItem(null);

            _inventoryItems.Clear();
        }

        public void Translate(Vector3 point)
        {
            transform.position = point;
        }
    }
}