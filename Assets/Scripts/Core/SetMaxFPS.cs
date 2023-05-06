using UnityEngine;

namespace Assets.Scripts.Core
{
    public class SetMaxFPS : MonoBehaviour
    {
        private void Start()
        {
            Application.targetFrameRate = 60;
        }
    }
}