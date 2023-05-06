using System.Linq;
using Assets.Scripts.Core.Mobs;
using Assets.Scripts.Defines;
using Assets.Scripts.Shared.Tags;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core
{
    public class ExitFromLevel : MonoBehaviour
    {
        [FormerlySerializedAs("Destination")] [SerializeField] private TagHolder _destination;
        [FormerlySerializedAs("DestinationExit")] [SerializeField] private TagHolder _destinationExit;

        [FormerlySerializedAs("ExitDirection")] [SerializeField] private Direction _exitDirection;

        public Vector2 EnterPoint => (Vector2)transform.position + ((_exitDirection == Direction.Right) ? Vector2.left : Vector2.right);
        public Vector2 ExitPoint => (Vector2)transform.position + ((_exitDirection == Direction.Right) ? Vector2.right : Vector2.left);
        public Direction EnterDirection => (_exitDirection == Direction.Right) ? Direction.Left : Direction.Right;
        public TagHolder Destination => _destination;
        public TagHolder DestinationExit => _destinationExit;
        public Direction ExitDirection => _exitDirection;

        public void ChangeLevel(Hero hero)
        {
            var levelTag = TagsStorageManager.FindTag<LevelTag>(_destination.Id);
            LevelChangeUserData changeUserData = new LevelChangeUserData();
            changeUserData.ExitId = _destinationExit.Id;
            changeUserData.HeroLives = hero.Lives;
            changeUserData.HeroInventoryItems = hero.InventoryItems.Select((item, i) => item.ItemPrefab).ToList();
            changeUserData.HeroWeaponSlotInfos = hero.MainWeaponSlots.Where(slot => slot.InventoryItem != null).Select(slot => new CheckPointData.HeroWeaponSlotInfo(slot.Placement, slot.InventoryItem.ItemPrefab)).ToList();

            SceneChanger.Change(levelTag.Name, TagsStorageManager.FindTag<LevelTag>(Level.ActiveLevel.Id.Id).Name, true, this, changeUserData, 1, 1);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + ((_exitDirection == Direction.Right) ? Vector3.right : Vector3.left), 0.1f);
        }
    }
}