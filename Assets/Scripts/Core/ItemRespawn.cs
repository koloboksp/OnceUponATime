using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Core
{
    public class ItemRespawn : MonoBehaviour
    {
        const string GizmosIconName = "ItemRespawn";

        public GameObject Prefab;

        void Start()
        {
            var instance = Instantiate(Prefab);
            SceneManager.MoveGameObjectToScene(instance, gameObject.scene);
            instance.transform.SetParent(transform, false);         
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawIcon(this.gameObject.transform.position, GizmosIconName, true);
        }
    }
}