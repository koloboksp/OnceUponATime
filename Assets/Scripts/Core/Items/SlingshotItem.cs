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

            var quaternion = Owner.transform.rotation * Quaternion.Euler(0, 0, angle);
            mBulletInstance.Body.AddForce((quaternion * Vector3.right) * ShotForce * mBulletInstance.Body.mass, ForceMode2D.Impulse);
        }

        public override void EndAttack()
        {
            //if(mBulletInstance != null)
                mBulletInstance.IgnoreCollisions(false);
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