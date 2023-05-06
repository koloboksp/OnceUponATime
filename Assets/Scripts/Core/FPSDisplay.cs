using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Core
{
    public class FPSDisplay : MonoBehaviour
    {
        private float _updateTime = 1.0f;

        private float _averageTimer = 0.0f;
        private int _frameCount = 0;

        private float _averageTime = 0.0f;
        private float _averageFPS = 0.0f;

        private Text _text;

        private void Start()
        {
            var foundCanvas = FindObjectOfType<Canvas>();

            GameObject fpsDrawer = new GameObject("FPS", typeof(RectTransform));
            fpsDrawer.transform.SetParent(foundCanvas.transform);
            var rectTransform = fpsDrawer.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = Vector2.zero;

            _text = fpsDrawer.AddComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _text.color = Color.yellow;
            _text.text = "";
        }

        private void Update()
        {
            _averageTimer += Time.deltaTime;
            _frameCount += 1;
            if (_averageTimer >= _updateTime)
            {
                _averageTime = _averageTimer / _frameCount;
                _averageFPS = _frameCount / _updateTime;


                _averageTimer = _averageTimer - _updateTime;
                _frameCount = 0;

                if(_text != null)
                    _text.text = (_averageTime * 1000.0f).ToString() + "#" + _averageFPS.ToString();
            }
        }    
    }
}