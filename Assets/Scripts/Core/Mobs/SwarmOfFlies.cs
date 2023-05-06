using System.Collections.Generic;
using Assets.Scripts.Core.Items;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs
{
    public class SwarmOfFlies : Character
    {
        private Level _level;

        private State _currentState = State.WaitForTarget;

        private Vector3 _startPosition;

        private bool _enemyDetected = false;
        private Hero _enemy;

        private float _searchTime = 2.0f;
        private float _searchTimer = 0.0f;

        private bool _canAttack = true;
        private float _attackSpeed = 1.0f;
        private float _attackTimer = 0.0f;

        private float _attackDistance = 0.3f;

        private float _dieEffectTimer;
        
        private float _getCriticalDamageEffectTimer;
        private float _getCriticalDamageEffectTime = 0.5f;

        private readonly List<Fly> _flies = new List<Fly>();

        [FormerlySerializedAs("FlyPrefab")] [SerializeField] private Fly _flyPrefab;
        [FormerlySerializedAs("BodyTrigger")] [SerializeField] private CircleCollider2D _bodyTrigger;

        [FormerlySerializedAs("ReactionTrigger")] [SerializeField] private Trigger _reactionTrigger;
        
        [FormerlySerializedAs("Speed")] [SerializeField] private float _speed = 1.0f;
        [FormerlySerializedAs("Damage")] [SerializeField] private float _damage = 1.0f;

        [FormerlySerializedAs("DamageReduce")] [SerializeField] private float _damageReduce = 0.1f;
        [FormerlySerializedAs("DropDownItemsLevel")] [SerializeField] private DropDownItemsLevel _dropDownItemsLevel = DropDownItemsLevel.CoinsAndMomentBonuses50;
        
        private void Start()
        {
            _startPosition = transform.position;
            _level = FindObjectOfType<Level>();

            for (int i = 0; i < 5; i++)
            {
                var instantiate = Instantiate(_flyPrefab.gameObject);
                var fly = instantiate.GetComponent<Fly>();

                fly.RandomRangeRadius = _bodyTrigger.radius;
                fly.transform.SetParent(transform, false);
                fly.transform.position = Vector3.zero;

                _flies.Add(fly);
            }

            _reactionTrigger.OnSomethingEnter += ReactionTrigger_OnSomethingEnter;
            _reactionTrigger.OnSomethingExit += ReactionTrigger_OnSomethingExit;
        }

        private static List<Hero> mNoAllocGetComponent = new List<Hero>();


        private void ReactionTrigger_OnSomethingEnter(Trigger sender, Collider2D collider2d)
        {
            collider2d.gameObject.GetComponents<Hero>(mNoAllocGetComponent);

            if (mNoAllocGetComponent.Count > 0)
            {
                _enemyDetected = true;
                _enemy = mNoAllocGetComponent[0];
            }
        }

        private void ReactionTrigger_OnSomethingExit(Trigger sender, Collider2D collider2d)
        {
            collider2d.gameObject.GetComponents<Hero>(mNoAllocGetComponent);

            if (mNoAllocGetComponent.Count > 0)
            {
                _enemyDetected = false;
                //mEnemy = null;
            }
        }

        private void Update()
        {  
            if (_currentState == State.WaitForTarget)
            {
                if (_enemyDetected)
                    _currentState = State.MoveToTarget;
            }
            else if (_currentState == State.MoveToTarget)
            {
                var point = _enemy.GetPoint("Neck");

                var vecToDestination = point.position - this.transform.position;
                var distanceToDestination = vecToDestination.magnitude;
                var timeToDestination = distanceToDestination / _speed;

                if (distanceToDestination > _attackDistance)
                {
                    float timeParts = timeToDestination / Time.deltaTime;
                    if (timeParts > 1)
                    {
                        this.transform.position += vecToDestination * (1.0f / timeParts);
                    }
                }
                else
                {
                    _currentState = State.Attack;
                }

                if (!_enemyDetected)
                {
                    _currentState = State.SearchTarget;
                    _searchTimer = 0.0f;
                }
            }
            else if (_currentState == State.SearchTarget)
            {
                _searchTimer += Time.deltaTime;
                if (_searchTimer > _searchTime)
                {
                    _currentState = State.Return;
                }

                var point = _enemy.GetPoint("Neck");

                var vecToDestination = point.position - this.transform.position;
                var distanceToDestination = vecToDestination.magnitude;

                var timeToDestination = distanceToDestination / _speed;
    
                float timeParts = timeToDestination / Time.deltaTime;
                if (timeParts > 1)
                {
                    this.transform.position += vecToDestination * (1.0f / timeParts);
                }
                
                if (_enemyDetected)
                {
                    _currentState = State.MoveToTarget;
                }
            }
            else if (_currentState == State.Return)
            {
                var vecToDestination = _startPosition - this.transform.position;
                var distanceToDestination = vecToDestination.magnitude;

                var timeToDestination = distanceToDestination / _speed;

                float timeParts = timeToDestination / Time.deltaTime;
                if (timeParts > 1)
                {
                    this.transform.position += vecToDestination * (1.0f / timeParts);
                }
                else
                {
                    this.transform.position = _startPosition;
                    _currentState = State.WaitForTarget;
                }
            }
            else if (_currentState == State.Attack)
            {
                 var point = _enemy.GetPoint("Neck");

                if (_canAttack)
                {
                    _canAttack = false;
                    _attackTimer = 0.0f;
                    
                    foreach (var fly in _flies)
                    {
                        fly.SetLocalDestination(transform.InverseTransformPoint(point.position));
                    }
                    _attackTimer = 0.0f;

                    _enemy.TakeDamage(null, new DamageInfo(DamageType.Prick, _damage, point.position, 0, Vector2.zero));
                }
                var vecToDestination = point.position - this.transform.position;
                var distanceToDestination = vecToDestination.magnitude;

                if (distanceToDestination > _attackDistance)
                {
                    _currentState = State.MoveToTarget;
                }
            }
            else if (_currentState == State.GetCriticalDamage)
            {
                _getCriticalDamageEffectTimer += Time.deltaTime;
                if (_getCriticalDamageEffectTimer >= _getCriticalDamageEffectTime)
                {
                    _currentState = State.SearchTarget;
                    _getCriticalDamageEffectTimer = 0.0f;
                } 
            }
            else if (_currentState == State.Die)
            {
                _dieEffectTimer += Time.deltaTime;
                if (_dieEffectTimer >= DeathTime)
                {
                    Object.Destroy(this.gameObject);
                }
            }

            if (!_canAttack)
            {
                _attackTimer += Time.deltaTime;
                if (_attackTimer > 1.0f / _attackSpeed)
                {
                    _canAttack = true;
                }
            }
        }

        public override void TakeDamage(object sender, DamageInfo damageInfo)
        {
            DamageInfo newDamageInfo;
            if ((damageInfo.Type & DamageType.Crush) != 0 ||
                (damageInfo.Type & DamageType.Push) != 0)
            {
                newDamageInfo = new DamageInfo(damageInfo.Type, damageInfo.Attack, damageInfo.Point, damageInfo.ForceValue, damageInfo.ForceDirection);
                _currentState = State.GetCriticalDamage;

                foreach (var fly in _flies)
                {
                    fly.StunEffect(transform.InverseTransformPoint(damageInfo.Point), damageInfo.ForceValue, _getCriticalDamageEffectTime);
                }
            }
            else
            {
                newDamageInfo = new DamageInfo(damageInfo.Type, damageInfo.Attack * _damageReduce, damageInfo.Point, damageInfo.ForceValue, damageInfo.ForceDirection);
            }

            base.TakeDamage(sender, newDamageInfo);        
        }



      
        protected override void Destroy()
        {
            base.Destroy();
            _currentState = State.Die;

            _bodyTrigger.enabled = false;
            _reactionTrigger.enabled = false;

            foreach (var fly in _flies)
            {
                fly.Die(UnityEngine.Random.Range(0.0f, DeathTime * 0.2f), UnityEngine.Random.Range(DeathTime * 0.6f, DeathTime * 0.8f));
            }

        }

        protected override void DeathOperation_OnComplete(Operation sender)
        {
            DropDownItemsManager.Instance.GenerateItem(_dropDownItemsLevel, transform.position);

            base.DeathOperation_OnComplete(sender);
        }
        
        private enum State
        {
            WaitForTarget,
            MoveToTarget,
            Attack,
            SearchTarget,
            Return,
            GetCriticalDamage,
            Die,
        }
    }
    
}