using System;
using Assets.Scripts.Core.Mobs;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class HopelessPlace : MonoBehaviour
    {
        public event Action<HopelessPlace> OnHeroEnter;

        void OnTriggerEnter2D(Collider2D collider2d)
        {
            var component = collider2d.gameObject.GetComponent<Hero>();
            if (component != null)
            {
                if(OnHeroEnter != null)
                    OnHeroEnter(this);
            }
        }
    }
}