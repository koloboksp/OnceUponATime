using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Assets.Scripts.Core
{
    public class CharacterRespawn : MonoBehaviour
    {
        private const string GizmosIconName = "MonsterRespawn";

        [FormerlySerializedAs("Prefab")] [SerializeField] private GameObject _prefab;

        protected virtual void Start()
        {
            var instance = Instantiate(_prefab, transform.position, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(instance, gameObject.scene);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(this.gameObject.transform.position, GizmosIconName, true);
        }
    }
}