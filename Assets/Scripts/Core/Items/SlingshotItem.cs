using UnityEngine;

namespace Assets.Scripts.Core.Items
{
    [CreateAssetMenu(fileName = "SlingshotItem", menuName = "Items/SlingshotItem", order = 51)]
    public class SlingshotItem : RangedWeaponItem
    {
        public SlingshotBullet BulletPrefab;
        public float ShotForce = 20;


        SlingshotBullet mBulletInstance;

        public override void Shot(float angle)
        {
            mBulletInstance = Instantiate(BulletPrefab, mEquipmentViewPartInstance.transform.position, mEquipmentViewPartInstance.transform.rotation);
            mBulletInstance.Owner = this;
            mBulletInstance.IgnoreCollisions(true);
            
            var directionOrientation = Owner.transform.rotation * Quaternion.Euler(0, 0, angle);
            mBulletInstance.AddForce((directionOrientation * Vector3.right) * ShotForce * mBulletInstance.Mass);
            mBulletInstance.OnDestroy += BulletInstance_OnDestroy;
        }

        public override void EndAttack()
        {
            if(mBulletInstance != null)
                mBulletInstance.IgnoreCollisions(false);
        }

        void BulletInstance_OnDestroy(SlingshotBullet bullet)
        {
            if (mBulletInstance == bullet)
            {
                mBulletInstance.OnDestroy -= BulletInstance_OnDestroy;
                mBulletInstance = null;
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