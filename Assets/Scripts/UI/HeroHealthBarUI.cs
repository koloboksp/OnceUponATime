using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class HeroHealthBarUI : MonoBehaviour
    {
        public RectTransform HealthBarTransform;

        private float mInitialWidth;

        private void OnEnable()
        {
            mInitialWidth = HealthBarTransform.rect.width;
        }
        public void Show(float normValue)
        {
            HealthBarTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mInitialWidth * normValue);  
        }
    }
}