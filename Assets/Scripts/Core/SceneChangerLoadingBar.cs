using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Assets.Scripts.Core
{
    public class SceneChangerLoadingBar : MonoBehaviour
    {
        [FormerlySerializedAs("ProgressBar")] [SerializeField] private Image _progressBar;

        public void SetProgress(float progress)
        {
            _progressBar.fillAmount = progress;
        }
    }
}