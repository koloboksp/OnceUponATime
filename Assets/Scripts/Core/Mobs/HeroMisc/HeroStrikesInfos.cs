using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    [CreateAssetMenu(fileName = "HeroStrikesInfos", menuName = "Hero/Strikes/HeroStrikesInfos", order = 51)]
    public class HeroStrikesInfos : ScriptableObject
    {
        [FormerlySerializedAs("StrikeInfos")] [SerializeField] private List<HeroStrikeInfo> _strikeInfos = new List<HeroStrikeInfo>();

        public IReadOnlyList<HeroStrikeInfo> StrikeInfos => _strikeInfos;
    }

    [Serializable]
    public class HeroStrikeInfo
    {
        [FormerlySerializedAs("Strike")] [SerializeField] private HeroAvailableStrikes _strike;

        [FormerlySerializedAs("DealDamageTimeInterval")] [SerializeField] private Vector2 _dealDamageTimeInterval = new Vector2(0, 1);
        [FormerlySerializedAs("ShotTime")] [SerializeField] private float _shotTime = 0.5f;

        [FormerlySerializedAs("BlockMovement")] [SerializeField] private bool _blockMovement;
        [FormerlySerializedAs("PreparationNeeded")] [SerializeField] private bool _preparationNeeded;

        [FormerlySerializedAs("Move")] [SerializeField] private bool _move = false;
        [FormerlySerializedAs("MovingSpeed")] [SerializeField] private float _movingSpeed = 1.0f;
        [FormerlySerializedAs("MovingDirection")] [SerializeField] private MovingDirection _movingDirection = MovingDirection.Forward;
        [FormerlySerializedAs("MovingTimeInterval")] [SerializeField] private Vector2 _movingTimeInterval = new Vector2(0, 1);

        public HeroAvailableStrikes Strike => _strike;
        public Vector2 DealDamageTimeInterval => _dealDamageTimeInterval;
        public float ShotTime => _shotTime;
        public bool BlockMovement => _blockMovement;
        public bool PreparationNeeded => _preparationNeeded;
        public bool Move => _move;
        public float MovingSpeed => _movingSpeed;
        public MovingDirection MovingDirection => _movingDirection;
        public Vector2 MovingTimeInterval => _movingTimeInterval;
    }
}