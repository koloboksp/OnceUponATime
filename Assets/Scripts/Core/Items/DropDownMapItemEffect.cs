using UnityEngine;

namespace Assets.Scripts.Core.Items
{
    public class DropDownMapItemEffect : MonoBehaviour
    {
        public MapItem Target;

        Rigidbody2D mBody;

        void Start()
        {
            mBody = gameObject.AddComponent<Rigidbody2D>();
            mBody.freezeRotation = true;
            var circleCollider2d = gameObject.AddComponent<CircleCollider2D>();
            circleCollider2d.radius = 0.1f;
            circleCollider2d.offset = Vector2.up * 0.1f;
            mBody.AddForce(Vector2.up * 4, ForceMode2D.Impulse);
        }

        void Update()
        {
            if (mBody.IsSleeping())
                Destroy(this.gameObject);

            if (Target != null)
                Target.transform.position = this.transform.position;
        }
    }
}