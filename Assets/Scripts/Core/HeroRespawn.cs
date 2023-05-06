using UnityEngine;

namespace Assets.Scripts.Core
{
    public class HeroRespawn : CharacterRespawn
    {
        private const string GizmosIconName = "HeroRespawn";

        protected override void Start()
        {
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(this.gameObject.transform.position, GizmosIconName, true);
        }
    }
}