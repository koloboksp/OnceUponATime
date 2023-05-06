using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Core.Mobs.DiggerMisc;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs
{
    public class Character : MonoBehaviour, IDamageable
    {
        public event Action<Character, DamageInfo> OnTakeDamage;
        public event Action<Character> OnLifeLevelChanged;
        public event Action<Character> OnBeforeDestroy;

        [FormerlySerializedAs("Bounds")] [SerializeField] private Rect _bounds = Rect.zero;

        [FormerlySerializedAs("Lives")] [SerializeField] private float _lives = 1;
        [FormerlySerializedAs("MaxLives")] [SerializeField] private float _maxLives = 1;

        [FormerlySerializedAs("DeathTime")] [SerializeField] private float _deathTime = 1;

        private readonly Operation _deathOperation = new Operation();

        public Rect Bounds => _bounds;
        public float Lives => _lives;
        public float MaxLives => _maxLives;
        public float DeathTime => _deathTime;
        public Operation DeathOperation => _deathOperation;
        
        public virtual void TakeDamage(object sender, DamageInfo damageInfo)
        {
            float previousLives = _lives;

            _lives -= damageInfo.Attack;
            _lives = Mathf.Clamp(_lives, 0, _maxLives);

            if (OnTakeDamage != null)
                OnTakeDamage(this, damageInfo);

            if (previousLives != _lives)
            {
                if (OnLifeLevelChanged != null)
                    OnLifeLevelChanged(this);
            }

            if (_lives <= 0)
            {
                if (!_deathOperation.InProcess)
                {
                    if (OnBeforeDestroy != null)
                        OnBeforeDestroy(this);

                    Destroy();
                }
            }
        }

        public virtual void SetLives(float lives)
        {
            _lives = lives;
        }
        
        public virtual void Treat(object sender, TreatmentInfo treatmentInfo)
        {
            _lives += Mathf.Min(treatmentInfo.Power, _maxLives - _lives);
            _lives = Mathf.Clamp(_lives, 0, _maxLives);

            if (OnLifeLevelChanged != null)
                OnLifeLevelChanged(this);

            if (_lives > 0)
            {
                if(_deathOperation.InProcess)
                    _deathOperation.Abort();
            }
        }

        protected virtual void Destroy()
        {
            _deathOperation.Execute(_deathTime);
            _deathOperation.OnComplete = DeathOperation_OnComplete;

            StartCoroutine(WaitForDeath());
        }

        private IEnumerator WaitForDeath()
        {
            while (_deathOperation.InProcess)
            {
                _deathOperation.Process(Time.deltaTime);
                yield return null;
            }
        }

        protected virtual void DeathOperation_OnComplete(Operation sender)
        {
            Destroy(this.gameObject);
        }

        public bool IsAlive
        {
            get { return _lives > 0; }
        }

        public virtual void IgnoreCollisions(Collider2D target, bool ignore)
        {
            
        }

        protected virtual void OnDrawGizmosSelected()
        {
            var rtC = transform.position + transform.rotation * (_bounds.center + _bounds.size * new Vector2(0.5f, 0.5f));
            var rbC = transform.position + transform.rotation * (_bounds.center + _bounds.size * new Vector2(0.5f, -0.5f));
            var lbC = transform.position + transform.rotation * (_bounds.center + _bounds.size * new Vector2(-0.5f, -0.5f));
            var tbC = transform.position + transform.rotation * (_bounds.center + _bounds.size * new Vector2(-0.5f, 0.5f));

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(rtC, rbC);
            Gizmos.DrawLine(rbC, lbC);
            Gizmos.DrawLine(lbC, tbC);
            Gizmos.DrawLine(tbC, rtC);
        }
    }
}