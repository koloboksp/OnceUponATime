using System.Collections.Generic;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs
{
    public class Stakes : Character
    {
        private DealDamageOnContact _dealDamageOnContact;

        [FormerlySerializedAs("DamageOnContact")] [SerializeField] private float _damageOnContact = 1;
        [FormerlySerializedAs("DamageForceOnContact")] [SerializeField] private float _damageForceOnContact = 4;
        [FormerlySerializedAs("DealDamageOnContactSpeed")] [SerializeField] private float _dealDamageOnContactSpeed = 4.0f;
        
        private void Awake()
        {
            _dealDamageOnContact = new DealDamageOnContact(OnDealDamageOnContact);
        }

        private void OnDealDamageOnContact(DealDamageOnContact.ContactInfo contactInfo)
        {
            Vector2 forceDirection;
            if (Vector2.Dot(contactInfo.AverageNormal, Vector2.right) < 0)
                forceDirection = Vector2.right + Vector2.up;
            else
                forceDirection = -Vector2.right + Vector2.up;
            forceDirection.Normalize();

            contactInfo.Target.TakeDamage(this, new DamageInfo(DamageType.Push, _damageOnContact, contactInfo.AveragePoint, _damageForceOnContact, forceDirection));
        }

        private void OnCollisionEnter2D(Collision2D collisionInfo)
        {
            _dealDamageOnContact.OnCollisionEnter2D(collisionInfo, this);
        }

        private void OnCollisionExit2D(Collision2D collisionInfo)
        {
            _dealDamageOnContact.OnCollisionExit2D(collisionInfo, this); 
        }

        private void Update()
        {
            _dealDamageOnContact.Update(_dealDamageOnContactSpeed);
        }
    }
}