using System.Linq;
using Assets.Scripts.Core.Mobs;
using Assets.Scripts.Defines;
using Assets.Scripts.Shared.Tags;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class ExitFromLevel : MonoBehaviour
    {
        public TagHolder Destination;
        public TagHolder DestinationExit;

        public Direction ExitDirection;

        public Vector2 EnterPoint
        {
            get
            {
                return (Vector2)transform.position + ((ExitDirection == Direction.Right) ? Vector2.left : Vector2.right);
            }
        }
        public Vector2 ExitPoint
        {
            get
            {
                return (Vector2)transform.position + ((ExitDirection == Direction.Right) ? Vector2.right : Vector2.left);
            }
        }
        public Direction EnterDirection
        {
            get
            {
                return (ExitDirection == Direction.Right) ? Direction.Left : Direction.Right;
            }
        }
        public void ChangeLevel(Hero hero)
        {
            var levelTag = TagsStorageManager.FindTag<LevelTag>(Destination.Id);
            LevelChangeUserData changeUserData = new LevelChangeUserData();
            changeUserData.ExitId = DestinationExit.Id;
            changeUserData.HeroLives = hero.Lives;
            changeUserData.HeroInventoryItems = hero.InventoryItems.Select((item, i) => item.ItemPrefab).ToList();
            changeUserData.HeroWeaponSlotInfos = hero.MainWeaponSlots.Where(slot => slot.InventoryItem != null).Select(slot => new CheckPointData.HeroWeaponSlotInfo(slot.Placement, slot.InventoryItem.ItemPrefab)).ToList();

            SceneChanger.Change(levelTag.Name, TagsStorageManager.FindTag<LevelTag>(Level.ActiveLevel.Id.Id).Name, true, this, changeUserData, 1, 1);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + ((ExitDirection == Direction.Right) ? Vector3.right : Vector3.left), 0.1f);
        }
    }
}