using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs.Helpers
{
    public class DealDamageOnContact
    {
        private static readonly List<IDamageable> NoAllocGetComponent = new List<IDamageable>();

        private readonly List<ContactInfo> _contactInfos = new List<ContactInfo>();

        private readonly Action<ContactInfo> _onDealDamage;

        public DealDamageOnContact(Action<ContactInfo> onDealDamage)
        {
            _onDealDamage = onDealDamage;
        }

        public bool Enable { get; set; } = true;

        public void Update(float dealDamageSpeed)
        {
            if(!Enable)
                return;
            
            for (var index = _contactInfos.Count - 1; index >= 0; index--)
            {
                var contactInfo = _contactInfos[index];
                if (!contactInfo.HasContacts() && 
                    contactInfo.AtLeastOnceBeenAttacked &&
                    contactInfo.MinimalAttackIntervalPassed)
                {
                    _contactInfos.RemoveAt(index);
                }
                else
                {
                    if (contactInfo.CanAttack())
                    {
                        contactInfo.Attack();

                        _onDealDamage(contactInfo);
                    }

                    contactInfo.UpdateDealDamageState(Time.deltaTime, dealDamageSpeed);
                }
            }
        }

        public void OnCollisionEnter2D(Collision2D collisionInfo, IDamageable ignored)
        {
            if (!Enable)
                return;

            collisionInfo.collider.gameObject.GetComponents<IDamageable>(NoAllocGetComponent);

            Vector2 averageNormal = Vector2.zero;
            Vector2 averagePoint = Vector2.zero;


            for (var index = 0; index < collisionInfo.contacts.Length; index++)
            {
                var collisionInfoContact = collisionInfo.contacts[index];

                averagePoint += collisionInfoContact.point / collisionInfo.contacts.Length;
                averageNormal += collisionInfoContact.normal / collisionInfo.contacts.Length;
            }


            for (var index = 0; index < NoAllocGetComponent.Count; index++)
            {
                var damageable = NoAllocGetComponent[index];
                if (!ReferenceEquals(damageable, ignored))
                {
                    var findIndex = _contactInfos.FindIndex(i => i.Target == damageable);
                    if (findIndex >= 0)
                        _contactInfos[findIndex].UpdateInfo(collisionInfo.collider, averagePoint, averageNormal);
                    else
                        _contactInfos.Add(new ContactInfo(damageable, collisionInfo.collider, averagePoint, averageNormal));
                }
            }
        }

        public void OnCollisionExit2D(Collision2D collisionInfo, IDamageable ignored)
        {
            if (!Enable)
                return;

            collisionInfo.collider.gameObject.GetComponents<IDamageable>(NoAllocGetComponent);

            for (var index = 0; index < NoAllocGetComponent.Count; index++)
            {
                var damageable = NoAllocGetComponent[index];
                if (!ReferenceEquals(damageable, ignored))
                {
                    var findIndex = _contactInfos.FindIndex(i => i.Target == damageable);
                    if (findIndex >= 0)
                        _contactInfos[findIndex].RemoveCollider(collisionInfo.collider);
                }
            }
        }
        
        public class ContactInfo
        {
            public readonly IDamageable Target;
            private readonly List<Collider2D> mColliders = new List<Collider2D>();

            private bool mMinimalAttackIntervalPassed;
            private float mAttackTimer;
            private int mAttacksCount = 0;

            private Vector3 mAveragePoint;
            private Vector3 mAverageNormal;

            public ContactInfo(IDamageable damageable, Collider2D collider, Vector2 averagePoint, Vector2 averageNormal)
            {
                Target = damageable;
                mColliders.Add(collider);
                mAveragePoint = averagePoint;
                mAverageNormal = averageNormal;

                mMinimalAttackIntervalPassed = true;
            }

            public void UpdateInfo(Collider2D collider2D, Vector2 middlePoint, Vector2 averageNormal)
            {
                mAveragePoint = middlePoint;
                mAverageNormal = averageNormal;

                if (!mColliders.Contains(collider2D))
                    mColliders.Add(collider2D);
            }

            public void RemoveCollider(Collider2D collider2D)
            {
                mColliders.Remove(collider2D);
            }


            public bool HasContacts()
            {
                return mColliders.Count > 0;
            }

            public Vector3 AveragePoint => mAveragePoint;
            public Vector3 AverageNormal => mAverageNormal;
            public bool MinimalAttackIntervalPassed => mMinimalAttackIntervalPassed;

            public bool AtLeastOnceBeenAttacked
            {
                get { return mAttacksCount != 0; }
            }

            public void Attack()
            {
                mAttacksCount++;
                mAttackTimer = 0.0f;
                mMinimalAttackIntervalPassed = false;
            }

            public bool CanAttack()
            {
                return mAttacksCount == 0;
            }

            public void UpdateDealDamageState(float deltaTime, float attackSpeed)
            {
                if (!mMinimalAttackIntervalPassed)
                {
                    mAttackTimer += deltaTime;
                    if (mAttackTimer >= 1.0f / attackSpeed)
                    {
                        mMinimalAttackIntervalPassed = true;                
                    }
                }
            }
        }
    }
}