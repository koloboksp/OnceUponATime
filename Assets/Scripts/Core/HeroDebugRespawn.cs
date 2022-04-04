using UnityEngine;

namespace Assets.Scripts.Core
{
    public class HeroDebugRespawn : CharacterRespawn
    {
        const string GizmosIconName = "HeroRespawn";

        protected override void Start()
        {
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawIcon(this.gameObject.transform.position, GizmosIconName, true);
        }

        void OnValidate()
        {
            this.hideFlags = HideFlags.DontSaveInBuild;
        }
    }
}