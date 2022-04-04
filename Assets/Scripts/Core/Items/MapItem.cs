using UnityEngine;

namespace Assets.Scripts.Core.Items
{
    public class MapItem : MonoBehaviour
    {
        const float RotationAngleSpeed = 30.0f;

        public Transform ViewRoot;
        public Collider2D Trigger;
        public Item TargetPrefab;

        public virtual void Taken()
        {
            Destroy(this.gameObject);
        }

        void Update()
        {
            ViewRoot.localRotation *= Quaternion.Euler(0, RotationAngleSpeed * Time.deltaTime, 0);
        }
    }
}