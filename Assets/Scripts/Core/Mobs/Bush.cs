using Assets.Scripts.Core.Items;
using Assets.Scripts.Effects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs
{
    public class Bush : MonoBehaviour, IDamageable
    {
        [FormerlySerializedAs("InteractionEffect")] [SerializeField] private ShakeEffect _interactionEffect;
        [FormerlySerializedAs("BodyCollider")] [SerializeField] private Collider2D _bodyCollider;

        [FormerlySerializedAs("DropDownItemsLevel")] [SerializeField] private DropDownItemsLevel _dropDownItemsLevel;

        private void OnTriggerEnter2D(Collider2D collider2d)
        {
            _interactionEffect.enabled = true;
        }
        
        public virtual void TakeDamage(object sender, DamageInfo damageInfo)
        {
            DropDownItemsManager.Instance.GenerateItem(_dropDownItemsLevel, transform.position);

            Destroy(this.gameObject);  
        }
    }
}

