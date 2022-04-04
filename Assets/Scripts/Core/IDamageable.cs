using System;
using UnityEngine;

namespace Assets.Scripts.Core
{
    [Flags]
    public enum DamageType
    {
        Cut,
        Crush,
        Push,
        Prick
    }

    public class DamageInfo
    {
        public readonly DamageType Type;
        public readonly float Attack;
        public readonly float ForceValue;
        public readonly Vector2 ForceDirection;
        public readonly Vector2 Point;

        public DamageInfo(DamageType damageType, float attack, Vector2 point, float forceValue, Vector2 forceDirection)
        {
            Type = damageType;
            Attack = attack;
            Point = point;
            ForceValue = forceValue;
            ForceDirection = forceDirection;
        }
    }

    public class TreatmentInfo
    {
        public readonly float Power;
        public TreatmentInfo(float power)
        {
            Power = power; 
        }
    }

    public interface IDamageable
    { 
        void TakeDamage(object sender, DamageInfo damageInfo);
    }
}