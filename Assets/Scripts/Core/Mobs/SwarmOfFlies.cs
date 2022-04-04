using System.Collections.Generic;
using Assets.Scripts.Core.Items;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class SwarmOfFlies : Character
    {
        enum State
        {
            WaitForTarget,
            MoveToTarget,
            Attack,
            SearchTarget,
            Return,
            GetCriticalDamage,
            Die,
        }

        public Fly FlyPrefab;
       // public float Radius = 0.5f;
        public CircleCollider2D BodyTrigger;

        public Trigger ReactionTrigger;
        

        public float Speed = 1.0f;
        public float Damage = 1.0f;

        public float DamageReduce = 0.1f;
        public DropDownItemsLevel DropDownItemsLevel = DropDownItemsLevel.CoinsAndMomentBonuses50;

        Level mLevel;

        State mCurrentState = State.WaitForTarget;

        Vector3 mStartPosition;

        bool mEnemyDetected = false;
        Hero mEnemy;

        float mSearchTime = 2.0f;
        float mSearchTimer = 0.0f;

        bool mCanAttack = true;
        float mAttackSpeed = 1.0f;
        float mAttackTimer = 0.0f;

        float mAttackDistance = 0.3f;

        float mDieEffectTimer;
    

        float mGetCriticalDamageEffectTimer;
        float mGetCriticalDamageEffectTime = 0.5f;

        readonly List<Fly> mFlies = new List<Fly>();

        void Start()
        {
            mStartPosition = transform.position;
            mLevel = FindObjectOfType<Level>();

            for (int i = 0; i < 5; i++)
            {
                var instantiate = Instantiate(FlyPrefab.gameObject);
                var component = instantiate.GetComponent<Fly>();

                component.RandomRangeRadius = BodyTrigger.radius;
                component.transform.SetParent(transform, false);
                component.transform.position = Vector3.zero;

                mFlies.Add(component);
            }

            ReactionTrigger.OnSomethingEnter += ReactionTrigger_OnSomethingEnter;
            ReactionTrigger.OnSomethingExit += ReactionTrigger_OnSomethingExit;
        }

        static List<Hero> mNoAllocGetComponent = new List<Hero>();

        
        void ReactionTrigger_OnSomethingEnter(Trigger sender, Collider2D collider2d)
        {
            collider2d.gameObject.GetComponents<Hero>(mNoAllocGetComponent);

            if (mNoAllocGetComponent.Count > 0)
            {
                mEnemyDetected = true;
                mEnemy = mNoAllocGetComponent[0];
            }
        }
        void ReactionTrigger_OnSomethingExit(Trigger sender, Collider2D collider2d)
        {
            collider2d.gameObject.GetComponents<Hero>(mNoAllocGetComponent);

            if (mNoAllocGetComponent.Count > 0)
            {
                mEnemyDetected = false;
                //mEnemy = null;
            }
        }
        void Update()
        {  
            if (mCurrentState == State.WaitForTarget)
            {
                if (mEnemyDetected)
                    mCurrentState = State.MoveToTarget;
            }
            else if (mCurrentState == State.MoveToTarget)
            {
                var point = mEnemy.GetPoint("Neck");

                var vecToDestination = point.position - this.transform.position;
                var distanceToDestination = vecToDestination.magnitude;
                var timeToDestination = distanceToDestination / Speed;

                if (distanceToDestination > mAttackDistance)
                {
                    float timeParts = timeToDestination / Time.deltaTime;
                    if (timeParts > 1)
                    {
                        this.transform.position += vecToDestination * (1.0f / timeParts);
                    }
                }
                else
                {
                    mCurrentState = State.Attack;
                }

                if (!mEnemyDetected)
                {
                    mCurrentState = State.SearchTarget;
                    mSearchTimer = 0.0f;
                }
            }
            else if (mCurrentState == State.SearchTarget)
            {
                mSearchTimer += Time.deltaTime;
                if (mSearchTimer > mSearchTime)
                {
                    mCurrentState = State.Return;
                }

                var point = mEnemy.GetPoint("Neck");

                var vecToDestination = point.position - this.transform.position;
                var distanceToDestination = vecToDestination.magnitude;

                var timeToDestination = distanceToDestination / Speed;
    
                float timeParts = timeToDestination / Time.deltaTime;
                if (timeParts > 1)
                {
                    this.transform.position += vecToDestination * (1.0f / timeParts);
                }
                
                if (mEnemyDetected)
                {
                    mCurrentState = State.MoveToTarget;
                }
            }
            else if (mCurrentState == State.Return)
            {
                var vecToDestination = mStartPosition - this.transform.position;
                var distanceToDestination = vecToDestination.magnitude;

                var timeToDestination = distanceToDestination / Speed;

                float timeParts = timeToDestination / Time.deltaTime;
                if (timeParts > 1)
                {
                    this.transform.position += vecToDestination * (1.0f / timeParts);
                }
                else
                {
                    this.transform.position = mStartPosition;
                    mCurrentState = State.WaitForTarget;
                }
            }
            else if (mCurrentState == State.Attack)
            {
                 var point = mEnemy.GetPoint("Neck");

                if (mCanAttack)
                {
                    mCanAttack = false;
                    mAttackTimer = 0.0f;
                    
                    foreach (var fly in mFlies)
                    {
                        fly.SetLocalDestination(transform.InverseTransformPoint(point.position));
                    }
                    mAttackTimer = 0.0f;

                    mEnemy.TakeDamage(null, new DamageInfo(DamageType.Prick, Damage, point.position, 0, Vector2.zero));
                }
                var vecToDestination = point.position - this.transform.position;
                var distanceToDestination = vecToDestination.magnitude;

                if (distanceToDestination > mAttackDistance)
                {
                    mCurrentState = State.MoveToTarget;
                }
            }
            else if (mCurrentState == State.GetCriticalDamage)
            {
                mGetCriticalDamageEffectTimer += Time.deltaTime;
                if (mGetCriticalDamageEffectTimer >= mGetCriticalDamageEffectTime)
                {
                    mCurrentState = State.SearchTarget;
                    mGetCriticalDamageEffectTimer = 0.0f;
                } 
            }
            else if (mCurrentState == State.Die)
            {
                mDieEffectTimer += Time.deltaTime;
                if (mDieEffectTimer >= DeathTime)
                {
                    Object.Destroy(this.gameObject);
                }
            }

            if (!mCanAttack)
            {
                mAttackTimer += Time.deltaTime;
                if (mAttackTimer > 1.0f / mAttackSpeed)
                {
                    mCanAttack = true;
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
                mCurrentState = State.GetCriticalDamage;

                foreach (var fly in mFlies)
                {
                    fly.StunEffect(transform.InverseTransformPoint(damageInfo.Point), damageInfo.ForceValue, mGetCriticalDamageEffectTime);
                }
            }
            else
            {
                newDamageInfo = new DamageInfo(damageInfo.Type, damageInfo.Attack * DamageReduce, damageInfo.Point, damageInfo.ForceValue, damageInfo.ForceDirection);
            }

            base.TakeDamage(sender, newDamageInfo);        
        }



      
        protected override void Destroy()
        {
            base.Destroy();
            mCurrentState = State.Die;

            BodyTrigger.enabled = false;
            ReactionTrigger.enabled = false;

            foreach (var fly in mFlies)
            {
                fly.Die(UnityEngine.Random.Range(0.0f, DeathTime * 0.2f), UnityEngine.Random.Range(DeathTime * 0.6f, DeathTime * 0.8f));
            }

        }

        protected override void DeathOperation_OnComplete(Operation sender)
        {
            DropDownItemsManager.Instance.GenerateItem(DropDownItemsLevel, transform.position);

            base.DeathOperation_OnComplete(sender);
        }
    }
    
}