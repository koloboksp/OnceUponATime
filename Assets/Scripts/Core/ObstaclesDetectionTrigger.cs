using System.Collections.Generic;
using Assets.Scripts.Core.Mobs;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class ObstaclesDetectionTrigger : Trigger
    {
        List<Collider2D> mColliders = new List<Collider2D>();

        void OnTriggerEnter2D(Collider2D collider2d)
        {
            if(collider2d.attachedRigidbody == null && !collider2d.isTrigger)
                if (!mColliders.Contains(collider2d))
                    mColliders.Add(collider2d);
        }

        void OnTriggerExit2D(Collider2D collider2d)
        {
            if (mColliders.Contains(collider2d))
                mColliders.Remove(collider2d);
        }
    

        public bool WallDetected 
        {
            get { return mColliders.Count > 0; }
        }
    }
}