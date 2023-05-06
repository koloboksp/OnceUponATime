using UnityEngine;

namespace Assets.Scripts.Core.Items
{
    public class DropDownMapItemEffect : MonoBehaviour
    {
        public MapItem Target;

        private Rigidbody2D _body;

        private void Start()
        {
            _body = gameObject.AddComponent<Rigidbody2D>();
            _body.freezeRotation = true;
            var circleCollider2d = gameObject.AddComponent<CircleCollider2D>();
            circleCollider2d.radius = 0.1f;
            circleCollider2d.offset = Vector2.up * 0.1f;
            _body.AddForce(Vector2.up * 4, ForceMode2D.Impulse);
        }

        private void Update()
        {
            if (_body.IsSleeping())
                Destroy(this.gameObject);

            if (Target != null)
                Target.transform.position = this.transform.position;
        }
    }
}