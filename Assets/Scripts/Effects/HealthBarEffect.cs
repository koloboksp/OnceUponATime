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

        HealthBarUI mInstance;
        float mTimer;
        float mVisibleTime = 2.0f;
        float mHideTime = 1.0f;

        UIGamePanel mCachedUIGamePanel;

        State mCurrentState = State.Hide;
    
        public void OnEnable()
        {
            Owner.OnLifeLevelChanged += Owner_OnLifeLevelChanged;
                
            mCachedUIGamePanel = FindObjectOfType<UIGamePanel>();
        }

        public void OnDisable()
        {
            Owner.OnLifeLevelChanged -= Owner_OnLifeLevelChanged;
        }

        void OnDestroy()
        {
            DestroyInstance();
        }
        void Owner_OnLifeLevelChanged(Character sender)
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
            mTimer = 0.0f;
        }

        void UpdateScreenPosition()
        {
            var t = Camera.main.WorldToScreenPoint(Owner.transform.position + Offset);
            mInstance.transform.position = t;
        }

        void DestroyInstance()
        {
            if (mInstance != null)
            {
                Destroy(mInstance.gameObject);
                mInstance = null;
            }
        }

        enum State
        {
            Hide,
            Show,
            StartHide,     
        }
        void Update()
        {
            if (mCurrentState == State.Hide)
            {

            }
            if (mCurrentState == State.Show)
            {
                mTimer += Time.deltaTime;
                UpdateScreenPosition();
                if (mTimer > mVisibleTime)
                {
                    mCurrentState = State.StartHide;

                    mInstance.Hide(mHideTime);
                }
            }

            if (mCurrentState == State.StartHide)
            {
                mTimer += Time.deltaTime;
                UpdateScreenPosition();
                if (mTimer > mVisibleTime + mHideTime)
                {
                    mCurrentState = State.Hide;

                    DestroyInstance();
                }
            }
           
        }

    }
}