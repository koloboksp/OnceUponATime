using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    public class HeroFightLogic : MonoBehaviour
    {
        private static Dictionary<WeaponItemPlacement, int> mItemPriorityInStrikeCombination =
            new Dictionary<WeaponItemPlacement, int>()
            {
                {WeaponItemPlacement.InRightHand, 0},
                {WeaponItemPlacement.OnHand, 1},
                {WeaponItemPlacement.OnLeg, 2},
                {WeaponItemPlacement.InLeftHand, 3},
            };
        
        [FormerlySerializedAs("Mind")] [SerializeField] private HeroMind _mind;
        [FormerlySerializedAs("StrikesInfos")] [SerializeField] private HeroStrikesInfos _strikesInfos;
        
        private List<KeyValuePair<HeroWeaponSlot, HeroRepresentationAboutWeaponItem>> _combination;
        private List<int> _selectedStrikeCombinations ;
        private int _itemIndex = 0;
        private int _strikeCombinationIndex = 0;

        private void Start()
        {
            _mind.Owner.OnItemInWeaponSlotsChanged += Owner_OnItemInWeaponSlotsChanged;
            _mind.Owner.OnTakeDamage += Owner_OnTakeDamage;
        }

        private void Owner_OnTakeDamage(Character arg1, DamageInfo arg2)
        {
            _combination = null;
            _itemIndex = 0;
            _strikeCombinationIndex = 0;
        }

        private void Owner_OnItemInWeaponSlotsChanged(Hero sender)
        {
            _combination = null;
            _itemIndex = 0;
            _strikeCombinationIndex = 0;
        }

        public AttackCombination GetAttackCombination(HeroAttackType attackType)
        {
            if (attackType == HeroAttackType.MainWeapon)
            {
                if (_combination == null)
                {
                    _combination = _mind.Owner.MainWeaponSlots
                        .Where(i => i.InventoryItem != null)
                        .Select(i => new KeyValuePair<HeroWeaponSlot, HeroRepresentationAboutWeaponItem>(i, _mind.RepresentationAboutItems.Find(ii => ii.Target == i.InventoryItem.ItemPrefab) as HeroRepresentationAboutWeaponItem))
                        .OrderBy((l) => mItemPriorityInStrikeCombination[l.Value.Placement]).ToList();    
                }

                if (_itemIndex == 0 && _strikeCombinationIndex == 0)
                {
                    _selectedStrikeCombinations = _combination.Select(i => Random.Range(0, i.Value.StrikesCombinations.Count)).ToList();
                }

                var strikesCombination = _combination[_itemIndex].Value.StrikesCombinations[_selectedStrikeCombinations[_itemIndex]].Strikes;
                var strikeInfo = _strikesInfos.StrikeInfos.First(i => i.Strike == strikesCombination[_strikeCombinationIndex]);

                var ac = new AttackCombination(
                    attackType,
                    _combination[_itemIndex].Value.UsingType,
                    _combination[_itemIndex].Key,
                    strikesCombination[_strikeCombinationIndex],
                    _combination[_itemIndex].Value.AttackPartTime,
                    _combination[_itemIndex].Value.WaitingPartTime,
                    strikeInfo.BlockMovement,
                    strikeInfo.PreparationNeeded,
                    strikeInfo.DealDamageTimeInterval * _combination[_itemIndex].Value.AttackPartTime,
                    strikeInfo.ShotTime * _combination[_itemIndex].Value.AttackPartTime,
                    strikeInfo.Move,
                    strikeInfo.MovingSpeed,
                    strikeInfo.MovingDirection,
                    strikeInfo.MovingTimeInterval);


                _strikeCombinationIndex++;
                if (_strikeCombinationIndex >= strikesCombination.Count)
                {
                    _strikeCombinationIndex = 0;
                    _itemIndex++;
                    if (_itemIndex >= _combination.Count)
                    {
                        _itemIndex = 0;
                    }
                }

                return ac;
            }

            return null;
        }
        
        public class AttackCombination
        {
            public readonly HeroAttackType AttackType;           
            public readonly HeroWeaponSlot WeaponSlot;

            public readonly WeaponItemUsingType UsingType;
            public readonly HeroAvailableStrikes Movement;

            public readonly float AttackPartTime;
            public readonly float WaitPartTime;

            public readonly bool BlockMovement;
            public readonly bool PreparationNeeded;

            public readonly Vector2 DealDamageTimeInterval;
            public readonly float ShotTime;

            public readonly bool Move;
            public readonly float MovingSpeed;
            public readonly MovingDirection MovingDirection;
            public readonly Vector2 MovingTimeInterval;
            
            public AttackCombination(HeroAttackType attackType,
                WeaponItemUsingType usingType,
                HeroWeaponSlot weaponSlot,
                HeroAvailableStrikes heroAvailableStrikes,
                float attackPartTime,
                float waitPartTime,
                bool blockMovement,
                bool preparationNeeded,
                Vector2 dealDamageTimeInterval,
                float shotTime,
                bool move,
                float movingSpeed,
                MovingDirection movingDirection,
                Vector2 movingTimeInterval)
            {
                AttackType = attackType;
                UsingType = usingType;
                WeaponSlot = weaponSlot;
                Movement = heroAvailableStrikes;
                AttackPartTime = attackPartTime;
                WaitPartTime = waitPartTime;
                BlockMovement = blockMovement;
                PreparationNeeded = preparationNeeded;
                DealDamageTimeInterval = dealDamageTimeInterval;
                ShotTime = shotTime;
                Move = move;
                MovingSpeed = movingSpeed;
                MovingDirection = movingDirection;
                MovingTimeInterval = movingTimeInterval;
            }
        }
    }
}