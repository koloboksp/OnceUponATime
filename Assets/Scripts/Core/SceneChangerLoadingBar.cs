using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Core
{
    public class SceneChangerLoadingBar : MonoBehaviour
    {
        public Image ProgressBar;

        public void SetProgress(float progress)
        {
            ProgressBar.fillAmount = progress;
        }
    }
}