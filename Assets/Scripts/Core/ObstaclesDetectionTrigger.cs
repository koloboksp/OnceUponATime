using System.Collections.Generic;
using Assets.Scripts.Core.Mobs;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class ObstaclesDetectionTrigger : Trigger
    {
        private readonly List<Collider2D> _colliders = new List<Collider2D>();

        private void OnTriggerEnter2D(Collider2D collider2d)
        {
            if(collider2d.attachedRigidbody == null && !collider2d.isTrigger)
                if (!_colliders.Contains(collider2d))
                    _colliders.Add(collider2d);
        }

        private void OnTriggerExit2D(Collider2D collider2d)
        {
            if (_colliders.Contains(collider2d))
                _colliders.Remove(collider2d);
        }
        
        public bool WallDetected 
        {
            get { return _colliders.Count > 0; }
        }
    }
}