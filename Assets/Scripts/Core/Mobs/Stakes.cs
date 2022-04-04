using System.Collections.Generic;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class Stakes : Character
    {
        public float DamageOnContact = 1;
        public float DamageForceOnContact = 4;
        public float DealDamageOnContactSpeed = 4.0f;

        DealDamageOnContact mDealDamageOnContact;

        void Awake()
        {
            mDealDamageOnContact = new DealDamageOnContact(OnDealDamageOnContact);
        }

        void OnDealDamageOnContact(DealDamageOnContact.ContactInfo contactInfo)
        {
            Vector2 forceDirection;
            if (Vector2.Dot(contactInfo.AverageNormal, Vector2.right) < 0)
                forceDirection = Vector2.right + Vector2.up;
            else
                forceDirection = -Vector2.right + Vector2.up;
            forceDirection.Normalize();

            contactInfo.Target.TakeDamage(this, new DamageInfo(DamageType.Push, DamageOnContact, contactInfo.AveragePoint, DamageForceOnContact, forceDirection));
        }

        void OnCollisionEnter2D(Collision2D collisionInfo)
        {
            mDealDamageOnContact.OnCollisionEnter2D(collisionInfo, this);
        }

        void OnCollisionExit2D(Collision2D collisionInfo)
        {
            mDealDamageOnContact.OnCollisionExit2D(collisionInfo, this); 
        }

        void Update()
        {
            mDealDamageOnContact.Update(DealDamageOnContactSpeed);
        }
    }
}