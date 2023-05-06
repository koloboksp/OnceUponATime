using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Core.Items
{
    public class Explosion : MonoBehaviour
    {
        public event Action<Explosion> OnDestroy;

        private static readonly Collider2D[] _triggeredItems = new Collider2D[20];
        
        [SerializeField] private float _radius = 1;
        [SerializeField] private float _attack = 1.0f;
        [SerializeField] private DamageType _damageType = DamageType.Crush;
        [SerializeField] private float _forceValue = 1;

        [SerializeField] private float _lifeTime = 2;

        private List<IDamageable> _ignoreList = new List<IDamageable>();

        public void SetIgnoreList(IEnumerable<IDamageable> ignoreList)
        {
            _ignoreList.Clear();
            _ignoreList.AddRange(ignoreList);
        }

        private void Start()
        {
            var position = transform.position;
            
            var result = Physics2D.OverlapCircleNonAlloc(position, _radius, _triggeredItems);
            
            for (var index = 0; index < result; index++)
            {
                var collider2d = _triggeredItems[index];
                
                var damageable = collider2d.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    bool dealDamage = true;
                    if (_ignoreList.Contains(damageable))
                        dealDamage = false;

                    if(dealDamage)
                        damageable.TakeDamage(this, new DamageInfo(_damageType, _attack, position, _forceValue, (position - collider2d.transform.position).normalized));
                }
            }
            
            SelfDestroy();
        }
        
        protected void SelfDestroy()
        {
            OnDestroy?.Invoke(this);
            Destroy(this.gameObject, _lifeTime);
        }
    }
}