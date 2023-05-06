using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class HeroHealthBarUI : MonoBehaviour
    {
        private float _initialWidth;

        [FormerlySerializedAs("HealthBarTransform")] [SerializeField] private RectTransform _healthBarTransform;
        
        private void OnEnable()
        {
            _initialWidth = _healthBarTransform.rect.width;
        }
        public void Show(float normValue)
        {
            _healthBarTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _initialWidth * normValue);  
        }
    }
}