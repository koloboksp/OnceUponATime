using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        enum State
        {
            Show,
            SmoothHide,
        }

        public Image Background;
        public Image HealthBar;
        public RectTransform HealthBarTransform;


        State mCurrentState;

        float mHideTime;
        float mHideTimer = 0.0f;

        Color mBackgroundInitialColor;
        Color mHealthBarInitialColor;
        Color mBackgroundHideColor;
        Color mHealthBarHideColor;
        float mInitialWidth;

        public void OnEnable()
        {
            mBackgroundInitialColor = Background.color;
            mHealthBarInitialColor = HealthBar.color;

            mBackgroundHideColor = mBackgroundInitialColor;
            mBackgroundHideColor.a = 0.0f;
            mHealthBarHideColor = mHealthBarInitialColor;
            mHealthBarHideColor.a = 0.0f;

            mInitialWidth = HealthBarTransform.rect.width;
        }
        public void Hide(float hideTime)
        {
            mCurrentState = State.SmoothHide;
            mHideTimer = 0;
            mHideTime = hideTime;

            if (mHideTime <= 0.0f)
                gameObject.SetActive(false);  
        }

        public void Show(float normValue)
        {
            gameObject.SetActive(true);

            Background.color = mBackgroundInitialColor;
            HealthBar.color = mHealthBarInitialColor;

            HealthBarTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mInitialWidth * normValue);

            mCurrentState = State.Show;
            mHideTimer = 0;
        }

        void Update()
        {
            if (mCurrentState == State.Show)
            {

            }
            else if (mCurrentState == State.SmoothHide)
            {
                if (mHideTimer < mHideTime)
                {
                    mHideTimer += Time.deltaTime;

                    var normValue = mHideTimer / mHideTime;
                    Background.color = Color.Lerp(mBackgroundInitialColor, mBackgroundHideColor, normValue);
                    HealthBar.color = Color.Lerp(mHealthBarInitialColor, mHealthBarHideColor, normValue);
                }
            }         
        }
    }
}