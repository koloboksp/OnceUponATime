using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    [CreateAssetMenu(fileName = "HeroStrikesInfos", menuName = "Hero/Strikes/HeroStrikesInfos", order = 51)]
    public class HeroStrikesInfos : ScriptableObject
    {
        public List<HeroStrikeInfo> StrikeInfos = new List<HeroStrikeInfo>();   
    }

    [Serializable]
    public class HeroStrikeInfo
    {
        public HeroAvailableStrikes Strike;

        public Vector2 DealDamageTimeInterval = new Vector2(0, 1);
        public float ShotTime = 0.5f;

        public bool BlockMovement;
        public bool PreparationNeeded;

        public bool Move = false;
        public float MovingSpeed = 1.0f;
        public MovingDirection MovingDirection = MovingDirection.Forward;
        public Vector2 MovingTimeInterval = new Vector2(0, 1);
    }
}