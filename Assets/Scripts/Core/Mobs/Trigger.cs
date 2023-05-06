using System;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class Trigger : MonoBehaviour
    {
        public event Action<Trigger, Collider2D> OnSomethingEnter;
        public event Action<Trigger, Collider2D> OnSomethingExit;

        private void OnTriggerEnter2D(Collider2D collider2d)
        {
            OnSomethingEnter?.Invoke(this, collider2d);
        }

        private void OnTriggerExit2D(Collider2D collider2d)
        {
            OnSomethingExit?.Invoke(this, collider2d);
        }
    }
}