using System;
using Assets.Scripts.Core.Mobs.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs
{
    public class Larva : GroundMob
    { 
        [FormerlySerializedAs("SmoothRotationRoot")] [SerializeField] private Transform _smoothRotationRoot;
        
        public Transform SmoothRotationRoot => _smoothRotationRoot;

        private void Update()
        {
            base.InnerUpdate();
        }
    }
}