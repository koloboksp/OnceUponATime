using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Core
{
    public class CharacterRespawn : MonoBehaviour
    {
        private const string GizmosIconName = "MonsterRespawn";

        public GameObject Prefab;

        protected virtual void Start()
        {
            var instance = Instantiate(Prefab, transform.position, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(instance, gameObject.scene);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(this.gameObject.transform.position, GizmosIconName, true);
        }
    }
}