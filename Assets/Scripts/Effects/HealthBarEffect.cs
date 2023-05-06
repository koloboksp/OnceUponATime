using Assets.Scripts.Core;
using Assets.Scripts.Core.Mobs;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Effects
{
    public class HealthBarEffect : MonoBehaviour
    {
        [FormerlySerializedAs("Owner")] [SerializeField] private Character _owner;
        [FormerlySerializedAs("UIPartPrefab")] [SerializeField] private HealthBarUI _uiPartPrefab;
        [FormerlySerializedAs("Offset")] [SerializeField] private Vector3 _offset;

        private HealthBarUI _instance;
        private float _timer;
        private float _visibleTime = 2.0f;
        private float _hideTime = 1.0f;

        private UIGamePanel _cachedUIGamePanel;

        private State _currentState = State.Hide;
    
        public void OnEnable()
        {
            _owner.OnLifeLevelChanged += Owner_OnLifeLevelChanged;
                
            _cachedUIGamePanel = FindObjectOfType<UIGamePanel>();
        }

        public void OnDisable()
        {
            _owner.OnLifeLevelChanged -= Owner_OnLifeLevelChanged;
        }

        private void OnDestroy()
        {
            DestroyInstance();
        }

        private void Owner_OnLifeLevelChanged(Character sender)
        {
            if (_instance == null)
            {
                var instanceObj = Object.Instantiate(_uiPartPrefab.gameObject);
                _instance = instanceObj.GetComponent<HealthBarUI>();
                _instance.transform.SetParent(_cachedUIGamePanel.Root);
                UpdateScreenPosition();
            }

            _instance.Show(sender.Lives / sender.MaxLives);

            _currentState = State.Show;
            _timer = 0.0f;
        }

        private void UpdateScreenPosition()
        {
            var t = Camera.main.WorldToScreenPoint(_owner.transform.position + _offset);
            _instance.transform.position = t;
        }

        private void DestroyInstance()
        {
            if (_instance != null)
            {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

        private enum State
        {
            Hide,
            Show,
            StartHide,     
        }

        private void Update()
        {
            if (_currentState == State.Hide)
            {

            }
            if (_currentState == State.Show)
            {
                _timer += Time.deltaTime;
                UpdateScreenPosition();
                if (_timer > _visibleTime)
                {
                    _currentState = State.StartHide;

                    _instance.Hide(_hideTime);
                }
            }

            if (_currentState == State.StartHide)
            {
                _timer += Time.deltaTime;
                UpdateScreenPosition();
                if (_timer > _visibleTime + _hideTime)
                {
                    _currentState = State.Hide;

                    DestroyInstance();
                }
            }
           
        }

    }
}