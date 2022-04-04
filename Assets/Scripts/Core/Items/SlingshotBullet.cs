using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Items
{
    public class SlingshotBullet : MonoBehaviour
    {   
        List<Collider2D> mTriggeredItems = new List<Collider2D>();
        bool mIgnoreCollisions;
        bool mCanDealDamage = true;

        public SlingshotItem Owner { get; set; }
      
        public Rigidbody2D Body;
        public Collider2D Collider;
        public Collider2D Trigger;

        public float Attack = 1.0f;
        public DamageType DamageType = DamageType.Crush;
        public float ForceValue = 1;
  
        public void IgnoreCollisions(bool ignore)
        {
            mIgnoreCollisions = ignore;

            Owner.Owner.IgnoreCollisions(Collider, mIgnoreCollisions);          
        }

        void FixedUpdate()
        {          
            for (var index = 0; index < mTriggeredItems.Count; index++)
            {
                var collider2d = mTriggeredItems[index];
                
                var damageable = collider2d.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    bool dealDamage = true;

                    if (ReferenceEquals(damageable, Owner.Owner) && mIgnoreCollisions)
                        dealDamage = false;

                    if(dealDamage && mCanDealDamage)
                    {
                        mCanDealDamage = false;
                        damageable.TakeDamage(this, new DamageInfo(DamageType, Attack, Body.position, ForceValue, (Body.position - (Vector2)collider2d.transform.position).normalized));
                        StartCoroutine(Destroy());
                    }
                }
            }
            mTriggeredItems.Clear();    
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider2d)
        {
            if (!mTriggeredItems.Contains(collider2d))
                mTriggeredItems.Add(collider2d);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collisionInfo)
        {
            mCanDealDamage = false;
            StartCoroutine(Destroy());
        }

        IEnumerator Destroy()
        {
            float mTimer = 0.0f;

            while (mTimer < 1.0f)
            {
                mTimer += Time.deltaTime;
                yield return null;
            }

            Destroy(this.gameObject);
        }
    }
}