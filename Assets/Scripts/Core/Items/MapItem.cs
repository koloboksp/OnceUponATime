using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core.Items
{
    public class MapItem : MonoBehaviour
    {
        private const float RotationAngleSpeed = 30.0f;

        [FormerlySerializedAs("ViewRoot")] [SerializeField] private Transform _viewRoot;
        [FormerlySerializedAs("Trigger")] [SerializeField] private Collider2D _trigger;
        [FormerlySerializedAs("TargetPrefab")] [SerializeField] private Item _targetPrefab;

        public Transform ViewRoot => _viewRoot;
        public Collider2D Trigger => _trigger;
        public Item TargetPrefab => _targetPrefab;
        
        public virtual void Taken()
        {
            Destroy(this.gameObject);
        }

        private void Update()
        {
            _viewRoot.localRotation *= Quaternion.Euler(0, RotationAngleSpeed * Time.deltaTime, 0);
        }
    }
}