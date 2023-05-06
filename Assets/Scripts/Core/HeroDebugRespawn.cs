using UnityEngine;

namespace Assets.Scripts.Core
{
    public class HeroDebugRespawn : CharacterRespawn
    {
        private const string GizmosIconName = "HeroRespawn";

        protected override void Start()
        {
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(this.gameObject.transform.position, GizmosIconName, true);
        }

        private void OnValidate()
        {
            this.hideFlags = HideFlags.DontSaveInBuild;
        }
    }
}