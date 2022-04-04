using System;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class Larva : GroundMob
    { 
        public Transform SmoothRotationRoot;
      
        void Update()
        {
            base.InnerUpdate();
        }
    }
}