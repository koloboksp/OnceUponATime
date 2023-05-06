using Assets.Scripts.Core;
using Assets.Scripts.Core.Mobs;
using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts.Effects
{
    public class HeroHealthBarEffect : MonoBehaviour
    {
        public HeroHealthBarUI UIPartPrefab;
        public GroundMovementCharacter Owner;

        private HeroHealthBarUI mInstance;
      
        public void Start()
        {
            var gamePanel = FindObjectOfType<UIGamePanel>();

            var instanceObj = Object.Instantiate(UIPartPrefab.gameObject);
            mInstance = instanceObj.GetComponent<HeroHealthBarUI>();
            mInstance.transform.SetParent(gamePanel.HeroHealthBarRoot);
            mInstance.transform.localPosition = Vector3.zero;
            mInstance.transform.localRotation = Quaternion.identity;
          
            Owner.OnLifeLevelChanged += Owner_OnLifeLevelChanged;
            mInstance.Show(Owner.Lives / Owner.MaxLives);
        }

        public void OnDisable()
        {
            Owner.OnLifeLevelChanged -= Owner_OnLifeLevelChanged;
        }

        private void Owner_OnLifeLevelChanged(Character sender)
        {
            mInstance.Show(sender.Lives / sender.MaxLives);     
        }
    }
}