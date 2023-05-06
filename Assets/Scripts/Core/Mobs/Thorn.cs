using Assets.Scripts.Core.Mobs.Helpers;
using Assets.Scripts.Effects;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class Thorn : Character
    {
        public ShakeEffect InteractionEffect;

        public Collider2D BodyCollider;

        public float DamageOnContact = 1;
        public float DamageForceOnContact = 4;
        public float DealDamageOnContactSpeed = 4.0f;

        private DealDamageOnContact mDealDamageOnContact;

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

            InteractionEffect.enabled = true;

            contactInfo.Target.TakeDamage(this, new DamageInfo(DamageType.Push, DamageOnContact, contactInfo.AveragePoint, DamageForceOnContact, -contactInfo.AverageNormal));
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
            mDealDamageOnContact.Update(DealDamageOnContactSpeed);
        }

        public override void TakeDamage(object sender, DamageInfo damageInfo)
        {
            base.TakeDamage(sender, damageInfo);

            InteractionEffect.enabled = true;
        }

        protected override void Destroy()
        {
            base.Destroy();

            BodyCollider.enabled = false;
        }
    }
}