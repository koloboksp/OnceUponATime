using Assets.Scripts.Core.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Mobs.HeroMisc
{
    public abstract class HeroRepresentationAboutItem : ScriptableObject
    {
        [FormerlySerializedAs("Target")] [SerializeField] private Item _target;

        public Object Target => _target;
    }
}