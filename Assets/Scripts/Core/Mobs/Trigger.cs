using System;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class Trigger : MonoBehaviour
    {
        public event Action<Trigger, Collider2D> OnSomethingEnter;
        public event Action<Trigger, Collider2D> OnSomethingExit;

        void OnTriggerEnter2D(Collider2D collider2d)
        {
            if (OnSomethingEnter != null)
                OnSomethingEnter(this, collider2d);
        }

        void OnTriggerExit2D(Collider2D collider2d)
        {
            if (OnSomethingExit != null)
                OnSomethingExit(this, collider2d);
        }
    }
}