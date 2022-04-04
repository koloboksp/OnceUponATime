using Assets.Scripts.Core.Items;
using Assets.Scripts.Effects;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class Bush : MonoBehaviour, IDamageable
    {
        public ShakeEffect InteractionEffect;
        public Collider2D BodyCollider;

        public DropDownItemsLevel DropDownItemsLevel; 
        
        void OnTriggerEnter2D(Collider2D collider2d)
        {
            InteractionEffect.enabled = true;
        }
        public virtual void TakeDamage(object sender, DamageInfo damageInfo)
        {
            DropDownItemsManager.Instance.GenerateItem(DropDownItemsLevel, transform.position);

            Destroy(this.gameObject);  
        }
    }
}

