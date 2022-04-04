using Assets.Scripts.Effects;

namespace Assets.Scripts.Core.Items
{
    public class SwordEquipmentItemView : EquipmentItemView
    {
        public TrailEffect TrailEffect;

        public override void BeginDealDamage()
        {
            base.BeginDealDamage();

            TrailEffect.Emit = true;
        }

        public override void EndDealDamage()
        {
            base.EndDealDamage();

            TrailEffect.Emit = false;
        }
    }
}