using Assets.Scripts.Effects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Items
{
    public class SwordEquipmentItemView : EquipmentItemView
    {
        [FormerlySerializedAs("TrailEffect")] [SerializeField] private TrailEffect _trailEffect;

        public override void BeginDealDamage()
        {
            base.BeginDealDamage();

            _trailEffect.Emit = true;
        }

        public override void EndDealDamage()
        {
            base.EndDealDamage();

            _trailEffect.Emit = false;
        }
    }
}