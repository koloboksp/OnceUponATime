using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Core
{
    public class FPSDisplay : MonoBehaviour
    {
        float mUpdateTime = 1.0f;

        float mAvergeTimer = 0.0f;
        int mFrameCount = 0;

        float mAvergeTime = 0.0f;
        float mAvergeFPS = 0.0f;

        Text mText;

        void Start()
        {
            var foundCanvas = FindObjectOfType<Canvas>();

            GameObject fpsDrawer = new GameObject("FPS", typeof(RectTransform));
            fpsDrawer.transform.SetParent(foundCanvas.transform);
            var rectTransform = fpsDrawer.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = Vector2.zero;

            mText = fpsDrawer.AddComponent<Text>();
            mText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            mText.color = Color.yellow;
            mText.text = "";
        }

        void Update()
        {
            mAvergeTimer += Time.deltaTime;
            mFrameCount += 1;
            if (mAvergeTimer >= mUpdateTime)
            {
                mAvergeTime = mAvergeTimer / mFrameCount;
                mAvergeFPS = mFrameCount / mUpdateTime;


                mAvergeTimer = mAvergeTimer - mUpdateTime;
                mFrameCount = 0;

                if(mText != null)
                    mText.text = (mAvergeTime * 1000.0f).ToString() + "#" + mAvergeFPS.ToString();
            }
        }    
    }
}