using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.Items;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    public class HeroFightLogic : MonoBehaviour
    {
        public HeroMind Mind;
        
        public HeroStrikesInfos StrikesInfos;

        private static Dictionary<WeaponItemPlacement, int> mItemPriorityInStrikeCombination =
            new Dictionary<WeaponItemPlacement, int>()
            {
                {WeaponItemPlacement.InRightHand, 0},
                {WeaponItemPlacement.OnHand, 1},
                {WeaponItemPlacement.OnLeg, 2},
                {WeaponItemPlacement.InLeftHand, 3},
            };

        List<KeyValuePair<HeroWeaponSlot, HeroRepresentationAboutWeaponItem>> mCombination;
        List<int> mSelectedStrikeCombinations ;
        int mItemIndex = 0;
        int mStrikeCombinationIndex = 0;

        void Start()
        {
            Mind.Owner.OnItemInWeaponSlotsChanged += Owner_OnItemInWeaponSlotsChanged;
            Mind.Owner.OnTakeDamage += Owner_OnTakeDamage;
        }

        void Owner_OnTakeDamage(Character arg1, DamageInfo arg2)
        {
            mCombination = null;
            mItemIndex = 0;
            mStrikeCombinationIndex = 0;
        }

        void Owner_OnItemInWeaponSlotsChanged(Hero sender)
        {
            mCombination = null;
            mItemIndex = 0;
            mStrikeCombinationIndex = 0;
        }

        public AttackCombination GetAttackCombination(HeroAttackType attackType)
        {
            if (attackType == HeroAttackType.MainWeapon)
            {
                if (mCombination == null)
                {
                    mCombination = Mind.Owner.MainWeaponSlots
                        .Where(i => i.InventoryItem != null)
                        .Select(i => new KeyValuePair<HeroWeaponSlot, HeroRepresentationAboutWeaponItem>(i, Mind.RepresentationAboutItems.Find(ii => ii.Target == i.InventoryItem.ItemPrefab) as HeroRepresentationAboutWeaponItem))
                        .OrderBy((l) => mItemPriorityInStrikeCombination[l.Value.Placement]).ToList();    
                }

                if (mItemIndex == 0 && mStrikeCombinationIndex == 0)
                {
                    mSelectedStrikeCombinations = mCombination.Select(i => Random.Range(0, i.Value.StrikesCombinations.Count)).ToList();
                }

                var strikesCombination = mCombination[mItemIndex].Value.StrikesCombinations[mSelectedStrikeCombinations[mItemIndex]].Strikes;
                var strikeInfo = StrikesInfos.StrikeInfos.Find(i => i.Strike == strikesCombination[mStrikeCombinationIndex]);

                var ac = new AttackCombination(
                    attackType,
                    mCombination[mItemIndex].Value.UsingType,
                    mCombination[mItemIndex].Key,
                    strikesCombination[mStrikeCombinationIndex],
                    mCombination[mItemIndex].Value.AttackPartTime,
                    mCombination[mItemIndex].Value.WaitingPartTime,
                    strikeInfo.BlockMovement,
                    strikeInfo.PreparationNeeded,
                    strikeInfo.DealDamageTimeInterval * mCombination[mItemIndex].Value.AttackPartTime,
                    strikeInfo.ShotTime * mCombination[mItemIndex].Value.AttackPartTime,
                    strikeInfo.Move,
                    strikeInfo.MovingSpeed,
                    strikeInfo.MovingDirection,
                    strikeInfo.MovingTimeInterval);


                mStrikeCombinationIndex++;
                if (mStrikeCombinationIndex >= strikesCombination.Count)
                {
                    mStrikeCombinationIndex = 0;
                    mItemIndex++;
                    if (mItemIndex >= mCombination.Count)
                    {
                        mItemIndex = 0;
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