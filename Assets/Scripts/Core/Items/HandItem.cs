using UnityEngine;

namespace Assets.Scripts.Core.Items
{
    [CreateAssetMenu(fileName = "HandItem", menuName = "Items/HandItem", order = 51)]
    public class HandItem : WeaponItem
    {
        public GameObject HandPunchEffectPrefab;

        GameObject mHandPunchEffectInstance;

        public override void BeginDealDamage(Rect damageArea)
        { 
            mHandPunchEffectInstance = Instantiate(HandPunchEffectPrefab);
            mHandPunchEffectInstance.transform.position = damageArea.center;
            var componentInChildren = HandPunchEffectPrefab.GetComponentInChildren<Animation>();
            componentInChildren.Play();
        }

        public override void EndDealDamage()
        {
            if (mHandPunchEffectInstance != null)
            {
                Destroy(mHandPunchEffectInstance);
                mHandPunchEffectInstance = null;
            }
            base.EndDealDamage();
        }
    }
}