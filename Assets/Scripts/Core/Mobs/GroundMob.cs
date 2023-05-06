using System;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs
{
    public class GroundMob : GroundMovementCharacter
    {
        public event Action OnDealDamageOnContact;

        private DealDamageOnContact _dealDamageOnContact;

        [FormerlySerializedAs("DamageOnContact")] [SerializeField] private float _damageOnContact = 1;
        [FormerlySerializedAs("DamageForceOnContact")] [SerializeField] private float _damageForceOnContact = 4;
        [FormerlySerializedAs("DealDamageOnContactSpeed")] [SerializeField] private float _dealDamageOnContactSpeed = 4f;
   
        protected virtual void Start()
        {
            _dealDamageOnContact = new DealDamageOnContact(OnDealDamageOnContactHelper);
        }

        protected override void InnerUpdate()
        {
            base.InnerUpdate();

            _dealDamageOnContact.Update(_dealDamageOnContactSpeed);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collisionInfo)
        {
           
            _dealDamageOnContact.OnCollisionEnter2D(collisionInfo, this);
        }

        protected virtual void OnCollisionExit2D(Collision2D collisionInfo)
        { 
            _dealDamageOnContact.OnCollisionExit2D(collisionInfo, this);
        }

        private void OnDealDamageOnContactHelper(DealDamageOnContact.ContactInfo contactInfo)
        {
            Vector2 forceDirection;
            if (Vector2.Dot(contactInfo.AverageNormal, Vector2.right) < 0)
                forceDirection = Vector2.right + Vector2.up;
            else
                forceDirection = -Vector2.right + Vector2.up;
            forceDirection.Normalize();

            contactInfo.Target.TakeDamage(this, new DamageInfo(DamageType.Push, _damageOnContact, contactInfo.AveragePoint, _damageForceOnContact, forceDirection));

            OnDealDamageOnContact?.Invoke();
        }

        protected override void Destroy()
        {
            _dealDamageOnContact.Enable = false;

            base.Destroy();
        }
    }
}