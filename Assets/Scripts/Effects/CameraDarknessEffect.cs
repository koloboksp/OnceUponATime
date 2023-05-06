using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Effects
{
    public class CameraDarknessEffect : MonoBehaviour
    {
        public enum DarknessValues
        {
            Zero,
            One,
        }
        public static float ConvertDarknessValue(DarknessValues value)
        {
            if (value == DarknessValues.One)
                return 1.0f;

            return 0.0f;
        }

        [NonSerialized] private Shader mUsingShader;
        [NonSerialized] private float mDarknessValue = 0.0f;
        [NonSerialized] private Material mMaterial;

        private void Awake()
        {
            if (mUsingShader == null)
            {
                mUsingShader = Resources.Load<Shader>("BuildIn/Shaders/CameraDarkness");
            }
            if (mUsingShader != null && mMaterial == null)
            {
                mMaterial = new Material(mUsingShader);
                mMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void Start()
        {
            if (!mUsingShader && !mUsingShader.isSupported)
                enabled = false;
        }

        private void OnDestroy()
        {
            if (mMaterial)
            {
                DestroyImmediate(mMaterial);
                mMaterial = null;
            }
        }


        private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
        {
            mMaterial.SetFloat("_DarknessValue", mDarknessValue);
            Graphics.Blit(sourceTexture, destTexture, mMaterial);
        }

        private List<HolderInfo> mHolderInfos = new List<HolderInfo>();

        private class HolderInfo
        {
            public readonly object Holder;
            private float mTimer;
            private float mTime;
            private DarknessValues mStartDarknessValue;
            private DarknessValues mEndDarknessValue;

            public bool CalculationCompleted { get; private set; }
            public DarknessValues EndDarknessValue { get { return mEndDarknessValue; } }

            public HolderInfo(object holder)
            {
                Holder = holder;
            }
            public void PlungeIntoDarkness(float time, DarknessValues startDarknessValue, DarknessValues endDarknessValue)
            {
                mTimer = 0.0f;
                mTime = time;
                mStartDarknessValue = startDarknessValue;
                mEndDarknessValue = endDarknessValue;

                CalculationCompleted = false;
            }
            public void PlungeIntoDarkness(DarknessValues darknessValue)
            {
                mTimer = 0.0f;
                mTime = 0;
                mStartDarknessValue = darknessValue;
                mEndDarknessValue = darknessValue;

                CalculationCompleted = true;
            }

            public float GetValue()
            {
                if (mTime > float.Epsilon)
                {
                    float param = Mathf.Clamp01(mTimer / mTime);
                    return Mathf.Lerp(ConvertDarknessValue(mStartDarknessValue), ConvertDarknessValue(mEndDarknessValue), param);
                }

                return ConvertDarknessValue(mEndDarknessValue);
            }
            public void Update(float dTime)
            {
                mTimer += dTime;
                if (mTimer >= mTime)
                    CalculationCompleted = true;
            }
        }

        private void SetDarkness(DarknessValues value, object holder)
        {
            var fHolderInfo = mHolderInfos.Find(i => i.Holder == holder);
            if (value == DarknessValues.One)
            {
                if (fHolderInfo == null)
                    mHolderInfos.Add(fHolderInfo = new HolderInfo(holder));
                fHolderInfo.PlungeIntoDarkness((value));
            }
            else
            {
                if (fHolderInfo != null)
                    mHolderInfos.Remove(fHolderInfo);
            }

            UpdateDarknessDerivedValue();
        }

        private void PlungeIntoDarkness(float time, DarknessValues startDarknessValue, DarknessValues endDarknessValue, object holder)
        {
            var fHolderInfo = mHolderInfos.Find(i => i.Holder == holder);
            if (fHolderInfo == null)
                mHolderInfos.Add(fHolderInfo = new HolderInfo(holder));
            fHolderInfo.PlungeIntoDarkness(time, startDarknessValue, endDarknessValue);

            StartCoroutine(ManualUpdate());
        }

        private void UpdateDarknessDerivedValue()
        {
            float maxDarknessValue = 0.0f;
            for (int hiIndex = 0; hiIndex < mHolderInfos.Count; hiIndex++)
            {
                HolderInfo hi = mHolderInfos[hiIndex];
                maxDarknessValue = Mathf.Max(hi.GetValue(), maxDarknessValue);
            }

            if (mDarknessValue != maxDarknessValue)
            {
                mDarknessValue = Mathf.Clamp(maxDarknessValue, 0.0f, 1.0f);
                if (mDarknessValue <= float.Epsilon)
                    this.enabled = false;
                else
                    this.enabled = true;
            }
        }

        private IEnumerator ManualUpdate()
        {
            while (true)
            {
                bool calculationsCompleted = true;

                for (int hiIndex = mHolderInfos.Count - 1; hiIndex >= 0; hiIndex--)
                {
                    var holderInfo = mHolderInfos[hiIndex];
                    holderInfo.Update(Time.deltaTime);
                    if (holderInfo.CalculationCompleted)
                    {
                        if (holderInfo.EndDarknessValue == DarknessValues.Zero)
                            mHolderInfos.RemoveAt(hiIndex);
                    }
                    else
                        calculationsCompleted = false;

                }

                UpdateDarknessDerivedValue();

                if (calculationsCompleted)
                {
                    yield break;
                }
                else
                {
                    yield return null;
                }
            }
        }


        public static void PlungeIntoDarkness(UnityEngine.Camera targetCamera, float time, DarknessValues startDarknessValue, DarknessValues endDarknessValue, object holder)
        {
            CameraDarknessEffect cameraDarknesEffect = targetCamera.gameObject.GetComponent<CameraDarknessEffect>();
            if (cameraDarknesEffect == null)
                cameraDarknesEffect = targetCamera.gameObject.AddComponent<CameraDarknessEffect>();

            cameraDarknesEffect.PlungeIntoDarkness(time, startDarknessValue, endDarknessValue, holder);
        }

        public static void PlungeIntoDarkness(UnityEngine.Camera targetCamera, DarknessValues darknessValue, object holder)
        {
            CameraDarknessEffect cameraDarknesEffect = targetCamera.gameObject.GetComponent<CameraDarknessEffect>();
            if (cameraDarknesEffect == null)
                cameraDarknesEffect = targetCamera.gameObject.AddComponent<CameraDarknessEffect>();

            cameraDarknesEffect.SetDarkness(darknessValue, holder);
        }


        public static Camera FindCameraInScene(Scene scene)
        {
            Camera foundCamera = null;
            GameObject[] rootGameObjects = scene.GetRootGameObjects();
            for (var oIndex = 0; oIndex < rootGameObjects.Length; oIndex++)
            {
                GameObject rootGameObject = rootGameObjects[oIndex];
                Camera findedCamera = rootGameObject.GetComponentInChildren<Camera>();
                if (findedCamera != null)
                {
                    foundCamera = findedCamera;
                    break;
                }
            }

            return foundCamera;
        }
    }
}