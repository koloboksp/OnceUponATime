using System;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;

namespace Assets.Scripts.Core.Mobs
{
    public class Larva : GroundMob
    { 
        public Transform SmoothRotationRoot;

        private void Update()
        {
            base.InnerUpdate();
        }
    }
}