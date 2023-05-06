using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Items
{
    public class SlingshotBullet : MonoBehaviour
    {   
        public event Action<SlingshotBullet> OnDestroy;
        
        [FormerlySerializedAs("Body")] [SerializeField] private Rigidbody2D _body;
        [FormerlySerializedAs("Collider")] [SerializeField] private Collider2D _collider;
        [FormerlySerializedAs("Trigger")] [SerializeField] private Collider2D _trigger;

        [FormerlySerializedAs("Attack")] [SerializeField] private float _attack = 1.0f;
        [FormerlySerializedAs("DamageType")] [SerializeField] private DamageType _damageType = DamageType.Crush;
        [FormerlySerializedAs("ForceValue")] [SerializeField] private float _forceValue = 1;
        [SerializeField] private Explosion _explosionPrefab;

        private readonly List<Collider2D> _triggeredItems = new List<Collider2D>();
        private bool _ignoreCollisions;
        private bool _canDealDamage = true;
        
        public SlingshotItem Owner { get; set; }
        public float Mass => _body.mass;
        
        public void IgnoreCollisions(bool ignore)
        {
            _ignoreCollisions = ignore;

            Owner.Owner.IgnoreCollisions(_collider, _ignoreCollisions);          
        }

        public void AddForce(Vector3 shotForce, ForceMode2D mode = ForceMode2D.Impulse)    
        {
            _body.AddForce(shotForce, mode);
        }

        private void FixedUpdate()
        {          
            for (var index = 0; index < _triggeredItems.Count; index++)
            {
                var collider2d = _triggeredItems[index];
                if(collider2d.isTrigger) continue;
                
                if (_explosionPrefab != null)
                {
                    var explosionInstance = Instantiate(_explosionPrefab, transform.position, transform.rotation);
                    explosionInstance.SetIgnoreList(new List<IDamageable>(){Owner.Owner});
                    SelfDestroy();
                    break;
                }
                else
                {
                    var damageable = collider2d.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        bool dealDamage = true;
                        if (ReferenceEquals(damageable, Owner.Owner) && _ignoreCollisions)
                            dealDamage = false;
                        
                        if (dealDamage && _canDealDamage)
                        {
                            _canDealDamage = false;
                            damageable.TakeDamage(this, new DamageInfo(_damageType, _attack, _body.position, _forceValue, (_body.position - (Vector2)collider2d.transform.position).normalized));

                            SelfDestroy();
                            break;
                        }
                    }
                }
                
            }
            _triggeredItems.Clear();    
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider2d)
        {
            if (!_triggeredItems.Contains(collider2d))
                _triggeredItems.Add(collider2d);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collisionInfo)
        {
            _canDealDamage = false;

            SelfDestroy();
        }

        protected void SelfDestroy()
        {
            OnDestroy?.Invoke(this);
            Destroy(this.gameObject);
        }
    }
}