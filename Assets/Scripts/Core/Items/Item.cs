using Assets.Scripts.Core.Mobs;
using UnityEngine;

namespace Assets.Scripts.Core.Items
{
    public enum ItemType
    {
        Undefined = 0,
        HealthBonus = 10,
        Gloves = 20,
        Sword = 30,
        Coins = 40,
        Boots = 50,
        Shestoper = 60,
        Slingshot = 70,
    }
    public class Item : ScriptableObject
    {
        protected EquipmentItemView mEquipmentViewPartInstance;
        protected ItemPreparationView mItemPreparationViewInstance;


        public Character Owner;

        public ItemType Type;
        public EquipmentItemView EquipmentViewPartPrefab;
        public ItemPreparationView ItemPreparationViewPrefab;


        public virtual void DealDamage(Vector2 position, Vector2 direction) { }

        public virtual void BeginDealDamage(Rect damageArea)
        {
            if (mEquipmentViewPartInstance != null)
            {
                mEquipmentViewPartInstance.BeginDealDamage();
            }
        }

        public virtual void EndDealDamage()
        {
            if (mEquipmentViewPartInstance != null)
            {
                mEquipmentViewPartInstance.EndDealDamage();
            }
        }

        public void CreateEquipmentViewPart(Transform anchor)
        {
            if (EquipmentViewPartPrefab != null && mEquipmentViewPartInstance == null)
            {
                mEquipmentViewPartInstance = Instantiate(EquipmentViewPartPrefab);

                mEquipmentViewPartInstance.transform.SetParent(anchor, true);
                mEquipmentViewPartInstance.transform.localPosition = Vector3.zero;
                mEquipmentViewPartInstance.transform.localRotation = Quaternion.identity;
            }
        }
        public void DestroyEquipmentViewPart()
        {
            if (mEquipmentViewPartInstance != null)
            {
                Object.Destroy(mEquipmentViewPartInstance.gameObject);
                mEquipmentViewPartInstance = null;
            }
        }

        public ItemPreparationView CreatePreparationView(Transform anchor)
        {
            if (ItemPreparationViewPrefab != null && mItemPreparationViewInstance == null)
            {
                mItemPreparationViewInstance = Instantiate(ItemPreparationViewPrefab);

                mItemPreparationViewInstance.transform.SetParent(anchor, true);
                mItemPreparationViewInstance.transform.localPosition = Vector3.zero;
                mItemPreparationViewInstance.transform.localRotation = Quaternion.identity;

                return mItemPreparationViewInstance;
            }

            return null;
        }

        public void DestroyPreparationView()
        {
            if (mItemPreparationViewInstance != null)
            {
                Object.Destroy(mItemPreparationViewInstance.gameObject);
                mItemPreparationViewInstance = null;
            }
        }
    }
}