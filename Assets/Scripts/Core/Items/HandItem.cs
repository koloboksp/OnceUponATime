using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Items
{
    [CreateAssetMenu(fileName = "HandItem", menuName = "Items/HandItem", order = 51)]
    public class HandItem : WeaponItem
    {
        [FormerlySerializedAs("HandPunchEffectPrefab")] 
        [SerializeField] private GameObject _handPunchEffectPrefab;

        private GameObject _handPunchEffectInstance;

        public override void BeginDealDamage(Rect damageArea)
        { 
            _handPunchEffectInstance = Instantiate(_handPunchEffectPrefab);
            _handPunchEffectInstance.transform.position = damageArea.center;
            var componentInChildren = _handPunchEffectPrefab.GetComponentInChildren<Animation>();
            componentInChildren.Play();
        }

        public override void EndDealDamage()
        {
            if (_handPunchEffectInstance != null)
            {
                Destroy(_handPunchEffectInstance);
                _handPunchEffectInstance = null;
            }
            base.EndDealDamage();
        }
    }
}