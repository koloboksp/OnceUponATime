using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.UI
{
    public class UIAttackBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private Style _activeStyle;
        private bool _mouseDown;
        private Vector2 _inputVector;
        private Vector2 _startPointerPosition;
        private Vector2 _btnStartPosition;
        
        [FormerlySerializedAs("DragArea")] [SerializeField] private Image _dragArea;
        [FormerlySerializedAs("NormalState")] [SerializeField] private Image _normalState;
        [FormerlySerializedAs("PressedState")] [SerializeField] private Image _pressedState;
        
        public bool IsPressed => _mouseDown;
        public Vector2 Direction => _inputVector;

        public void Start()
        {
            _dragArea.enabled = true;
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (_mouseDown)
            {
                if (_activeStyle == Style.Ranged)
                {
                    var deltaPosition = eventData.position - _startPointerPosition;
                    var btnHalfHeight = _pressedState.rectTransform.sizeDelta.y * 0.5f;

                    var halfDragSize = _dragArea.rectTransform.sizeDelta.y * 0.5f;

                    _pressedState.rectTransform.anchoredPosition = _btnStartPosition + new Vector2(0,
                        Mathf.Clamp(deltaPosition.y,
                            -halfDragSize + btnHalfHeight - _btnStartPosition.y,
                            halfDragSize - btnHalfHeight - _btnStartPosition.y));

                    var deltaPositionY = _pressedState.rectTransform.anchoredPosition.y / (halfDragSize - btnHalfHeight);

                    deltaPositionY = Mathf.Clamp(deltaPositionY, -1, 1);
                    deltaPositionY = deltaPositionY * 60;
                    _inputVector = new Vector2(0, deltaPositionY);
                }
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _normalState.enabled = false;
            _pressedState.enabled = true;
            //DragArea.enabled = true;

            _startPointerPosition = eventData.position;
            _btnStartPosition = _pressedState.rectTransform.anchoredPosition;

            _mouseDown = true;
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _normalState.enabled = true;
            _pressedState.enabled = false;
          //  DragArea.enabled = false;

            _mouseDown = false;
            _normalState.rectTransform.anchoredPosition = _pressedState.rectTransform.anchoredPosition;

        }

        public void ResetState()
        {
            _mouseDown = false;

            _normalState.enabled = true;
            _pressedState.enabled = false;
        }

        public void SetStyle(Style style)
        {
            _activeStyle = style;
            if (_activeStyle == Style.Melee)
            {
                _dragArea.gameObject.SetActive(false);
                _pressedState.rectTransform.anchoredPosition = Vector2.zero;
                _normalState.rectTransform.anchoredPosition = _pressedState.rectTransform.anchoredPosition;
            }
            else
            {
                _dragArea.gameObject.SetActive(true);
            }
        }

        public enum Style
        {
            Melee,
            Ranged,
        }
    }
}