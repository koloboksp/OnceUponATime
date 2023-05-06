using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    [CreateAssetMenu(fileName = "HeroRepresentationAboutWeaponItem", menuName = "Hero/Representation/AboutWeaponItem", order = 51)]
    public class HeroRepresentationAboutWeaponItem : HeroRepresentationAboutItem, ISerializationCallbackReceiver
    {
        [FormerlySerializedAs("UsingType")] [SerializeField] private WeaponItemUsingType _usingType = WeaponItemUsingType.Melee;
        [FormerlySerializedAs("Placement")] [SerializeField] private WeaponItemPlacement _placement = WeaponItemPlacement.InRightHand;
        [FormerlySerializedAs("RequiredFreeSlots")] [SerializeField] private List<WeaponItemPlacement> _requiredFreeSlots = new List<WeaponItemPlacement>();

        [FormerlySerializedAs("StrikesCombinations")] [SerializeField] private List<HeroAvailableStrikesCombinations> _strikesCombinations = new List<HeroAvailableStrikesCombinations>();

        [FormerlySerializedAs("UseSpeed")] [SerializeField] private float _useSpeed = 1.0f;
        [FormerlySerializedAs("WaitingPart")] [SerializeField] private float _waitingPart = 0.3f;
        [FormerlySerializedAs("AttackPartTime")] [SerializeField] private float _attackPartTime = 0.5f;
        [FormerlySerializedAs("WaitingPartTime")] [SerializeField] private float _waitingPartTime = 0.5f;

        public WeaponItemUsingType UsingType => _usingType;
        public WeaponItemPlacement Placement => _placement;
        public IReadOnlyList<WeaponItemPlacement> RequiredFreeSlots => _requiredFreeSlots;
        public IReadOnlyList<HeroAvailableStrikesCombinations> StrikesCombinations => _strikesCombinations;
        public float UseSpeed => _useSpeed;
        public float WaitingPart => _waitingPart;
        public float AttackPartTime => _attackPartTime;
        public float WaitingPartTime => _waitingPartTime;
        
        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            float allTime = 1 / _useSpeed;

            _attackPartTime = allTime - allTime * _waitingPart;
            _waitingPartTime = allTime * _waitingPart;
        }
    }

    [Serializable]
    public class HeroAvailableStrikesCombinations
    {
        [FormerlySerializedAs("Strikes")] [SerializeField] private List<HeroAvailableStrikes> _strikes = new List<HeroAvailableStrikes>();

        public IReadOnlyList<HeroAvailableStrikes> Strikes => _strikes;
    }
}