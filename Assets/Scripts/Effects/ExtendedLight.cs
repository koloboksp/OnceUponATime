using UnityEngine;

namespace Assets.Scripts.Effects
{
    public class ExtendedLight : MonoBehaviour
    {
        [SerializeField]
        float mRange = 10.0f;
        [SerializeField]
        Color mColor = Color.white;

        public bool Blink;
        float mBlinkingRangeChange = 0.05f;
        Vector2 mBlinkingTimeToRangeChange = new Vector2(0.15f, 0.2f);

        public Transform AttachRoot;
        public Light Light;

        public Mesh FakeLightMesh;
        public Material FakeLightMaterial;

        GameObject mFakeLightObj;
        MeshRenderer mFakeLightRenderer;
        MaterialPropertyBlock mMaterialPropertyBlock;

        public void OnEnable()
        {
            mFakeLightObj = new GameObject("FakeLight");
            mFakeLightObj.layer = 26;
            mFakeLightObj.transform.SetParent(AttachRoot);
            mFakeLightObj.transform.localPosition = Vector3.zero;
            mFakeLightObj.transform.localRotation = Quaternion.identity;
            mFakeLightObj.transform.localScale = new Vector3(mRange * 2, mRange * 2);
            var meshFilter = mFakeLightObj.AddComponent<MeshFilter>();
            meshFilter.mesh = FakeLightMesh;
            mFakeLightRenderer = mFakeLightObj.AddComponent<MeshRenderer>();
            mFakeLightRenderer.material = FakeLightMaterial;

            mMaterialPropertyBlock = new MaterialPropertyBlock();
            mMaterialPropertyBlock.SetFloat("_Range", mRange);
            mMaterialPropertyBlock.SetColor("_Color", mColor);
            mFakeLightRenderer.SetPropertyBlock(mMaterialPropertyBlock);
        }

        public void OnDisable()
        {
            Destroy(mFakeLightObj);
        }
        public void UpdateTargets()
        {
            UpdateRange(mRange);
        }

        void UpdateRange(float range)
        {
            if (Light != null)
            {
                Light.range = range;
                Light.color = mColor;
            }

            if (mFakeLightObj != null)
            {
                mFakeLightObj.transform.localScale = new Vector3(range * 2, range * 2);

                mMaterialPropertyBlock.SetFloat("_Range", range);
                mMaterialPropertyBlock.SetColor("_Color", mColor);

                mFakeLightRenderer.SetPropertyBlock(mMaterialPropertyBlock);
            }
        }


        float mTimer;
        float mTimeToChange ;
        bool WaitFor;

        void Update()
        {
            if (Blink)
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
                    var randomRange = Random.Range(-mBlinkingRangeChange, mBlinkingRangeChange);
                    mTimeToChange = Random.Range(mBlinkingTimeToRangeChange.x, mBlinkingTimeToRangeChange.y);
                    WaitFor = true;
                    mTimer = 0.0f;

                    var newRange = (randomRange + 1) * mRange;
                    UpdateRange(newRange);
                }
               
            }
        }

        public void OnDrawGizmos()
        {
#if UNITY_EDITOR
            UnityEditor.Handles.DrawWireDisc(AttachRoot.position, Vector3.forward, mRange);
#endif

        }
    }


}