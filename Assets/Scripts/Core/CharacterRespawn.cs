using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Core
{
    public class CharacterRespawn : MonoBehaviour
    {
        const string GizmosIconName = "MonsterRespawn";

        public GameObject Prefab;

        protected virtual void Start()
        {
            var instance = Instantiate(Prefab, transform.position, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(instance, gameObject.scene);
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawIcon(this.gameObject.transform.position, GizmosIconName, true);
        }
    }
}