using System;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class GroundMob : GroundMovementCharacter
    {
        public event Action OnDealDamageOnContact;

        DealDamageOnContact mDealDamageOnContact;

        public float DamageOnContact = 1;
        public float DamageForceOnContact = 4;
        public float DealDamageOnContactSpeed = 4f;
   
        protected virtual void Start()
        {
            mDealDamageOnContact = new DealDamageOnContact(OnDealDamageOnContactHelper);
        }

        protected override void InnerUpdate()
        {
            base.InnerUpdate();

            mDealDamageOnContact.Update(DealDamageOnContactSpeed);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collisionInfo)
        {
           
            mDealDamageOnContact.OnCollisionEnter2D(collisionInfo, this);
        }

        protected virtual void OnCollisionExit2D(Collision2D collisionInfo)
        { 
            mDealDamageOnContact.OnCollisionExit2D(collisionInfo, this);
        }

        void OnDealDamageOnContactHelper(DealDamageOnContact.ContactInfo contactInfo)
        {
            Vector2 forceDirection;
            if (Vector2.Dot(contactInfo.AverageNormal, Vector2.right) < 0)
                forceDirection = Vector2.right + Vector2.up;
            else
                forceDirection = -Vector2.right + Vector2.up;
            forceDirection.Normalize();

            contactInfo.Target.TakeDamage(this, new DamageInfo(DamageType.Push, DamageOnContact, contactInfo.AveragePoint, DamageForceOnContact, forceDirection));

            if (OnDealDamageOnContact != null)
                OnDealDamageOnContact();
        }

        protected override void Destroy()
        {
            mDealDamageOnContact.Enable = false;

            base.Destroy();
        }
    }
}