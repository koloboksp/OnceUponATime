using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Items
{
    [CreateAssetMenu(fileName = "SlingshotItem", menuName = "Items/SlingshotItem", order = 51)]
    public class SlingshotItem : RangedWeaponItem
    {
        [FormerlySerializedAs("BulletPrefab")] [SerializeField] private SlingshotBullet _bulletPrefab;
        [FormerlySerializedAs("ShotForce")] [SerializeField] private float _shotForce = 20;
        
        private SlingshotBullet _bulletInstance;

        public override void Shot(float angle)
        {
            _bulletInstance = Instantiate(_bulletPrefab, mEquipmentViewPartInstance.transform.position, mEquipmentViewPartInstance.transform.rotation);
            _bulletInstance.Owner = this;
            _bulletInstance.IgnoreCollisions(true);
            
            var directionOrientation = Owner.transform.rotation * Quaternion.Euler(0, 0, angle);
            _bulletInstance.AddForce((directionOrientation * Vector3.right) * _shotForce * _bulletInstance.Mass);
            _bulletInstance.OnDestroy += BulletInstance_OnDestroy;
        }

        public override void EndAttack()
        {
            if(_bulletInstance != null)
                _bulletInstance.IgnoreCollisions(false);
        }

        private void BulletInstance_OnDestroy(SlingshotBullet bullet)
        {
            if (_bulletInstance == bullet)
            {
                _bulletInstance.OnDestroy -= BulletInstance_OnDestroy;
                _bulletInstance = null;
            }
        }
    }

    public class RangedWeaponItem : WeaponItem
    {
        public virtual void Shot(float angle)
        {
        }
        public virtual void EndAttack()
        {
        }
    }
}