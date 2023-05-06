using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Effects
{
    public class ExtendedLight : MonoBehaviour
    {
        [FormerlySerializedAs("mRange")] 
        [SerializeField] private float _range = 10.0f;
        [FormerlySerializedAs("mColor")] 
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private float _intensity = 1000.0f;

        [FormerlySerializedAs("Blink")] [SerializeField] private bool _blink;
        [FormerlySerializedAs("AttachRoot")] [SerializeField] private Transform _attachRoot;
        [FormerlySerializedAs("Light")] [SerializeField] private Light _light;
        [FormerlySerializedAs("FakeLightMesh")] [SerializeField] private Mesh _fakeLightMesh;
        [FormerlySerializedAs("FakeLightMaterial")] [SerializeField] private Material _fakeLightMaterial;

        private float _blinkingRangeChange = 0.05f;
        private Vector2 _blinkingTimeToRangeChange = new Vector2(0.15f, 0.2f);

        private GameObject _fakeLightObj;
        private MeshRenderer _fakeLightRenderer;
        private MaterialPropertyBlock _materialPropertyBlock;

        public void OnEnable()
        {
            _fakeLightObj = new GameObject("FakeLight");
            _fakeLightObj.layer = 26;
            _fakeLightObj.transform.SetParent(_attachRoot);
            _fakeLightObj.transform.localPosition = Vector3.zero;
            _fakeLightObj.transform.localRotation = Quaternion.identity;
            _fakeLightObj.transform.localScale = new Vector3(_range * 2, _range * 2);
            var meshFilter = _fakeLightObj.AddComponent<MeshFilter>();
            meshFilter.mesh = _fakeLightMesh;
            _fakeLightRenderer = _fakeLightObj.AddComponent<MeshRenderer>();
            _fakeLightRenderer.material = _fakeLightMaterial;

            _materialPropertyBlock = new MaterialPropertyBlock();
            _materialPropertyBlock.SetFloat("_Range", _range);
            _materialPropertyBlock.SetColor("_Color", _color);
            _fakeLightRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        public void OnDisable()
        {
            Destroy(_fakeLightObj);
        }
        public void UpdateTargets()
        {
            UpdateRange(_range);
        }

        void UpdateRange(float range)
        {
            if (_light != null)
            {
                _light.range = range;
                _light.color = _color;
            }

            if (_fakeLightObj != null)
            {
                _fakeLightObj.transform.localScale = new Vector3(range * 2, range * 2);

                _materialPropertyBlock.SetFloat("_Range", range);
                _materialPropertyBlock.SetColor("_Color", _color);
                _materialPropertyBlock.SetFloat("_Intensity", _intensity);
                
                _fakeLightRenderer.SetPropertyBlock(_materialPropertyBlock);
            }
        }


        float mTimer;
        float mTimeToChange ;
        bool WaitFor;

        void Update()
        {
            if (_blink)
            {
                if (WaitFor)
                {
                    mTimer += Time.deltaTime;
                    if (mTimer > mTimeToChange)
                    {
                        WaitFor = false;
                    }
                }
                else
                {
                    var randomRange = Random.Range(-_blinkingRangeChange, _blinkingRangeChange);
                    mTimeToChange = Random.Range(_blinkingTimeToRangeChange.x, _blinkingTimeToRangeChange.y);
                    WaitFor = true;
                    mTimer = 0.0f;

                    var newRange = (randomRange + 1) * _range;
                    UpdateRange(newRange);
                }
               
            }
        }

        public void OnDrawGizmos()
        {
#if UNITY_EDITOR
            UnityEditor.Handles.DrawWireDisc(_attachRoot.position, Vector3.forward, _range);
#endif

        }
    }


}