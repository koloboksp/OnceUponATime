using System;
using Assets.Scripts.Core.Mobs;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class HopelessPlace : MonoBehaviour
    {
        public event Action<HopelessPlace> OnHeroEnter;

        private void OnTriggerEnter2D(Collider2D collider2d)
        {
            var hero = collider2d.gameObject.GetComponent<Hero>();
            if (hero != null)
                OnHeroEnter?.Invoke(this);
        }
    }
}