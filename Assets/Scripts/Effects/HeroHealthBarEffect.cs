using Assets.Scripts.Core;
using Assets.Scripts.Core.Mobs;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Effects
{
    public class HeroHealthBarEffect : MonoBehaviour
    {
        private HeroHealthBarUI _instance;

        [FormerlySerializedAs("UIPartPrefab")] [SerializeField] private HeroHealthBarUI _uiPartPrefab;
        [FormerlySerializedAs("Owner")] [SerializeField] private GroundMovementCharacter _owner;
        
        public void Start()
        {
            var gamePanel = FindObjectOfType<UIGamePanel>();

            var instanceObj = Object.Instantiate(_uiPartPrefab.gameObject);
            _instance = instanceObj.GetComponent<HeroHealthBarUI>();
            _instance.transform.SetParent(gamePanel.HeroHealthBarRoot);
            _instance.transform.localPosition = Vector3.zero;
            _instance.transform.localRotation = Quaternion.identity;
          
            _owner.OnLifeLevelChanged += Owner_OnLifeLevelChanged;
            _instance.Show(_owner.Lives / _owner.MaxLives);
        }

        public void OnDisable()
        {
            _owner.OnLifeLevelChanged -= Owner_OnLifeLevelChanged;
        }

        private void Owner_OnLifeLevelChanged(Character sender)
        {
            _instance.Show(sender.Lives / sender.MaxLives);     
        }
    }
}