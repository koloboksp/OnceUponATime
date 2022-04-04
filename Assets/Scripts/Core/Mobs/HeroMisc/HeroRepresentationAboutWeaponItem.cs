using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    [CreateAssetMenu(fileName = "HeroRepresentationAboutWeaponItem", menuName = "Hero/Representation/AboutWeaponItem", order = 51)]
    public class HeroRepresentationAboutWeaponItem : HeroRepresentationAboutItem, ISerializationCallbackReceiver
    {
        public WeaponItemUsingType UsingType = WeaponItemUsingType.Melee;
        public WeaponItemPlacement Placement = WeaponItemPlacement.InRightHand;
        public List<WeaponItemPlacement> RequiredFreeSlots = new List<WeaponItemPlacement>();

        public List<HeroAvailableStrikesCombinations> StrikesCombinations = new List<HeroAvailableStrikesCombinations>();

        [SerializeField]
        float UseSpeed = 1.0f;
        [SerializeField]
        float WaitingPart = 0.3f;

        public float AttackPartTime = 0.5f;
        public float WaitingPartTime = 0.5f;

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            float allTime = 1 / UseSpeed;

            AttackPartTime = allTime - allTime * WaitingPart;
            WaitingPartTime = allTime * WaitingPart;
        }
    }

    [Serializable]
    public class HeroAvailableStrikesCombinations
    {
        public List<HeroAvailableStrikes> Strikes = new List<HeroAvailableStrikes>();
    }
}