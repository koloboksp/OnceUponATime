using Assets.Scripts.Core;
using Assets.Scripts.Core.Mobs;
using Assets.Scripts.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Effects
{
    public class HealthBarEffect : MonoBehaviour
    {
        public Character Owner;
        public HealthBarUI UIPartPrefab;
        public Vector3 Offset;

        private HealthBarUI mInstance;
        private float _timer;
        private float mVisibleTime = 2.0f;
        private float mHideTime = 1.0f;

        private UIGamePanel mCachedUIGamePanel;

        private State mCurrentState = State.Hide;
    
        public void OnEnable()
        {
            Owner.OnLifeLevelChanged += Owner_OnLifeLevelChanged;
                
            mCachedUIGamePanel = FindObjectOfType<UIGamePanel>();
        }

        public void OnDisable()
        {
            Owner.OnLifeLevelChanged -= Owner_OnLifeLevelChanged;
        }

        private void OnDestroy()
        {
            DestroyInstance();
        }

        private void Owner_OnLifeLevelChanged(Character sender)
        {
            if (mInstance == null)
            {
                var instanceObj = Object.Instantiate(UIPartPrefab.gameObject);
                mInstance = instanceObj.GetComponent<HealthBarUI>();
                mInstance.transform.SetParent(mCachedUIGamePanel.Root);
                UpdateScreenPosition();
            }

            mInstance.Show(sender.Lives / sender.MaxLives);

            mCurrentState = State.Show;
            _timer = 0.0f;
        }

        private void UpdateScreenPosition()
        {
            var t = Camera.main.WorldToScreenPoint(Owner.transform.position + Offset);
            mInstance.transform.position = t;
        }

        private void DestroyInstance()
        {
            if (mInstance != null)
            {
                Destroy(mInstance.gameObject);
                mInstance = null;
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
            if (mCurrentState == State.Hide)
            {

            }
            if (mCurrentState == State.Show)
            {
                _timer += Time.deltaTime;
                UpdateScreenPosition();
                if (_timer > mVisibleTime)
                {
                    mCurrentState = State.StartHide;

                    mInstance.Hide(mHideTime);
                }
            }

            if (mCurrentState == State.StartHide)
            {
                _timer += Time.deltaTime;
                UpdateScreenPosition();
                if (_timer > mVisibleTime + mHideTime)
                {
                    mCurrentState = State.Hide;

                    DestroyInstance();
                }
            }
           
        }

    }
}