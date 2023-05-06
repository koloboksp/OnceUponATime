using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core
{
    public class ItemRespawn : MonoBehaviour
    {
        private const string GizmosIconName = "ItemRespawn";

        [FormerlySerializedAs("Prefab")] [SerializeField] private GameObject _prefab;

        private void Start()
        {
            var instance = Instantiate(_prefab);
            SceneManager.MoveGameObjectToScene(instance, gameObject.scene);
            instance.transform.SetParent(transform, false);         
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(this.gameObject.transform.position, GizmosIconName, true);
        }
    }
}