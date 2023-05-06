using Assets.Scripts.Core.Mobs.Helpers;
using Assets.Scripts.Effects;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs
{
    public class Thorn : Character
    {
        private DealDamageOnContact mDealDamageOnContact;

        [FormerlySerializedAs("InteractionEffect")] [SerializeField] private ShakeEffect _interactionEffect;

        [FormerlySerializedAs("BodyCollider")] [SerializeField] private Collider2D _bodyCollider;

        [FormerlySerializedAs("DamageOnContact")] [SerializeField] private float _damageOnContact = 1;
        [FormerlySerializedAs("DamageForceOnContact")] [SerializeField] private float _damageForceOnContact = 4;
        [FormerlySerializedAs("DealDamageOnContactSpeed")] [SerializeField] private float _dealDamageOnContactSpeed = 4.0f;
        
        private void Awake()
        {
            mDealDamageOnContact = new DealDamageOnContact(OnDealDamageOnContact);
        }

        private void OnDealDamageOnContact(DealDamageOnContact.ContactInfo contactInfo)
        {
            Vector2 forceDirection;
            if (Vector2.Dot(contactInfo.AverageNormal, Vector2.right) < 0)
                forceDirection = Vector2.right + Vector2.up;
            else
                forceDirection = -Vector2.right + Vector2.up;
            forceDirection.Normalize();

            _interactionEffect.enabled = true;

            contactInfo.Target.TakeDamage(this, new DamageInfo(DamageType.Push, _damageOnContact, contactInfo.AveragePoint, _damageForceOnContact, -contactInfo.AverageNormal));
        }

        private void OnCollisionEnter2D(Collision2D collisionInfo)
        {
            mDealDamageOnContact.OnCollisionEnter2D(collisionInfo, this);
        }

        private void OnCollisionExit2D(Collision2D collisionInfo)
        {
            mDealDamageOnContact.OnCollisionExit2D(collisionInfo, this);
        }

        private void Update()
        {
            mDealDamageOnContact.Update(_dealDamageOnContactSpeed);
        }

        public override void TakeDamage(object sender, DamageInfo damageInfo)
        {
            base.TakeDamage(sender, damageInfo);

            _interactionEffect.enabled = true;
        }

        protected override void Destroy()
        {
            base.Destroy();

            _bodyCollider.enabled = false;
        }
    }
}