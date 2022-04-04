using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Core.Mobs.DiggerMisc;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class Character : MonoBehaviour, IDamageable
    {
        public event Action<Character, DamageInfo> OnTakeDamage;
        public event Action<Character> OnLifeLevelChanged;
        public event Action<Character> OnBeforeDestroy;

        public Rect Bounds = Rect.zero;

        public float Lives = 1;
        public float MaxLives = 1;

        public float DeathTime = 1;

        readonly Operation mDeathOperation = new Operation();

        public Operation DeathOperation => mDeathOperation;
        public virtual void TakeDamage(object sender, DamageInfo damageInfo)
        {
            float previousLives = Lives;

            Lives -= damageInfo.Attack;
            Lives = Mathf.Clamp(Lives, 0, MaxLives);

            if (OnTakeDamage != null)
                OnTakeDamage(this, damageInfo);

            if (previousLives != Lives)
            {
                if (OnLifeLevelChanged != null)
                    OnLifeLevelChanged(this);
            }

            if (Lives <= 0)
            {
                if (!mDeathOperation.InProcess)
                {
                    if (OnBeforeDestroy != null)
                        OnBeforeDestroy(this);

                    Destroy();
                }
            }
        }

        public virtual void Treat(object sender, TreatmentInfo treatmentInfo)
        {
            Lives += Mathf.Min(treatmentInfo.Power, MaxLives - Lives);
            Lives = Mathf.Clamp(Lives, 0, MaxLives);

            if (OnLifeLevelChanged != null)
                OnLifeLevelChanged(this);

            if (Lives > 0)
            {
                if(mDeathOperation.InProcess)
                    mDeathOperation.Abort();
            }
        }

        protected virtual void Destroy()
        {
            mDeathOperation.Execute(DeathTime);
            mDeathOperation.OnComplete = DeathOperation_OnComplete;

            StartCoroutine(WaitForDeath());
        }

        IEnumerator WaitForDeath()
        {
            while (mDeathOperation.InProcess)
            {
                mDeathOperation.Process(Time.deltaTime);
                yield return null;
            }
        }

        protected virtual void DeathOperation_OnComplete(Operation sender)
        {
            Destroy(this.gameObject);
        }

        public bool IsAlive
        {
            get { return Lives > 0; }
        }

        public virtual void IgnoreCollisions(Collider2D target, bool ignore)
        {
            
        }

        protected virtual void OnDrawGizmosSelected()
        {
            var rtC = transform.position + transform.rotation * (Bounds.center + Bounds.size * new Vector2(0.5f, 0.5f));
            var rbC = transform.position + transform.rotation * (Bounds.center + Bounds.size * new Vector2(0.5f, -0.5f));
            var lbC = transform.position + transform.rotation * (Bounds.center + Bounds.size * new Vector2(-0.5f, -0.5f));
            var tbC = transform.position + transform.rotation * (Bounds.center + Bounds.size * new Vector2(-0.5f, 0.5f));

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(rtC, rbC);
            Gizmos.DrawLine(rbC, lbC);
            Gizmos.DrawLine(lbC, tbC);
            Gizmos.DrawLine(tbC, rtC);
        }
    }
}