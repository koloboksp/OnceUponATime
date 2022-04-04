using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.UI
{
    public class UIAttackBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public Image DragArea;
        public Image NormalState;
        public Image PressedState;

        bool mMouseDown;

        Vector2 mInputVector;
        public bool IsPressed => mMouseDown;
        public Vector2 Direction => mInputVector;

        public void Start()
        {
            DragArea.enabled = true;
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (mMouseDown)
            {
                Vector2 pos;
                
                var deltaPosition = eventData.position - mStartPointerPosition;
                var btnHalfHeight = PressedState.rectTransform.sizeDelta.y * 0.5f;

                var halfDragSize = DragArea.rectTransform.sizeDelta.y * 0.5f;


                PressedState.rectTransform.anchoredPosition = mBtnStartPosition + new Vector2(0, 
                                                                  Mathf.Clamp(deltaPosition.y, 
                                                                      -halfDragSize + btnHalfHeight - mBtnStartPosition.y, 
                                                                      halfDragSize - btnHalfHeight - mBtnStartPosition.y));

                var deltaPositionY = PressedState.rectTransform.anchoredPosition.y / (halfDragSize - btnHalfHeight);  

                deltaPositionY = Mathf.Clamp(deltaPositionY, -1, 1);
                deltaPositionY = deltaPositionY * 60;
                mInputVector = new Vector2(0, deltaPositionY);

              
            }
        }

       
        Vector2 mStartPointerPosition;
        Vector2 mBtnStartPosition;

        public void OnPointerDown(PointerEventData eventData)
        {
            NormalState.enabled = false;
            PressedState.enabled = true;
            //DragArea.enabled = true;

            mStartPointerPosition = eventData.position;
            mBtnStartPosition = PressedState.rectTransform.anchoredPosition;

            mMouseDown = true;
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            NormalState.enabled = true;
            PressedState.enabled = false;
          //  DragArea.enabled = false;

            mMouseDown = false;
            NormalState.rectTransform.anchoredPosition = PressedState.rectTransform.anchoredPosition;

        }

        public void ResetState()
        {
            mMouseDown = false;

            NormalState.enabled = true;
            PressedState.enabled = false;
        }
    }
}