using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        private State _currentState;

        private float _hideTime;
        private float _hideTimer = 0.0f;

        private Color _backgroundInitialColor;
        private Color _healthBarInitialColor;
        private Color _backgroundHideColor;
        private Color _healthBarHideColor;
        private float _initialWidth;

        [FormerlySerializedAs("Background")] [SerializeField] private Image _background;
        [FormerlySerializedAs("HealthBar")] [SerializeField] private Image _healthBar;
        [FormerlySerializedAs("HealthBarTransform")] [SerializeField] private RectTransform _healthBarTransform;
        
        public void OnEnable()
        {
            _backgroundInitialColor = _background.color;
            _healthBarInitialColor = _healthBar.color;

            _backgroundHideColor = _backgroundInitialColor;
            _backgroundHideColor.a = 0.0f;
            _healthBarHideColor = _healthBarInitialColor;
            _healthBarHideColor.a = 0.0f;

            _initialWidth = _healthBarTransform.rect.width;
        }
        public void Hide(float hideTime)
        {
            _currentState = State.SmoothHide;
            _hideTimer = 0;
            _hideTime = hideTime;

            if (_hideTime <= 0.0f)
                gameObject.SetActive(false);  
        }

        public void Show(float normValue)
        {
            gameObject.SetActive(true);

            _background.color = _backgroundInitialColor;
            _healthBar.color = _healthBarInitialColor;

            _healthBarTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _initialWidth * normValue);

            _currentState = State.Show;
            _hideTimer = 0;
        }

        private void Update()
        {
            if (_currentState == State.Show)
            {

            }
            else if (_currentState == State.SmoothHide)
            {
                if (_hideTimer < _hideTime)
                {
                    _hideTimer += Time.deltaTime;

                    var normValue = _hideTimer / _hideTime;
                    _background.color = Color.Lerp(_backgroundInitialColor, _backgroundHideColor, normValue);
                    _healthBar.color = Color.Lerp(_healthBarInitialColor, _healthBarHideColor, normValue);
                }
            }         
        }
        
        private enum State
        {
            Show,
            SmoothHide,
        }

    }
}