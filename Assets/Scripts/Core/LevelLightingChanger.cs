using System;
using System.Collections.Generic;
using Assets.Scripts.Core.Mobs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Core
{
    public class LevelLightingChanger : MonoBehaviour
    {
        const string GizmosIconName = "LevelLightingChanger";

        private static readonly List<Hero> NoAllocGetComponent = new List<Hero>();
        
        [SerializeField] private CircleCollider2D _collider;
        [SerializeField] private LevelLightingSettings _from;
        [SerializeField] private LevelLightingSettings _to;
        [SerializeField] private Level _level;
        
        private void OnTriggerStay2D(Collider2D other)
        {
            if (!enabled)
                return;

            other.gameObject.GetComponents<Hero>(NoAllocGetComponent);

            if (NoAllocGetComponent.Count > 0)
            {
                var heroPosition = NoAllocGetComponent[0].ViewPart.transform.position;
                var vecToHero = this.transform.InverseTransformPoint(heroPosition);
                var pVecToHero = Vector3.Project(vecToHero, Vector3.right);

                var pDirToHero = pVecToHero.normalized;
                var side = Vector3.Dot(Vector3.right, pDirToHero);

                var distanceToHero = pVecToHero.magnitude;
                var nDistanceToHero = distanceToHero / _collider.radius;
                var transition = 0.0f;
                transition = side < 0 ? (1.0f - nDistanceToHero) * 0.5f : 0.5f + nDistanceToHero * 0.5f;
                transition = Mathf.Clamp01(transition);
                _level.Lighting.Blend(_from, _to, transition);
            }
        }
        
        void OnDrawGizmos()
        {
            Gizmos.DrawIcon(this.gameObject.transform.position, GizmosIconName, true);
        }
    }
}