using UnityEngine;

namespace Assets.Scripts.Core
{
    public class SetMaxFPS : MonoBehaviour
    {

        void Start()
        {
            Application.targetFrameRate = 60;
        }
    }
}