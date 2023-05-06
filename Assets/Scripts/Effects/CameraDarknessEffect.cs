using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Effects
{
    public class CameraDarknessEffect : MonoBehaviour
    {
        private readonly List<HolderInfo> _holderInfos = new List<HolderInfo>();

        [NonSerialized] private Shader _usingShader;
        [NonSerialized] private float _darknessValue = 0.0f;
        [NonSerialized] private Material _material;

        private void Awake()
        {
            if (_usingShader == null)
                _usingShader = Resources.Load<Shader>("BuildIn/Shaders/CameraDarkness");
            
            if (_usingShader != null && _material == null)
            {
                _material = new Material(_usingShader);
                _material.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void Start()
        {
            if (!_usingShader && !_usingShader.isSupported)
                enabled = false;
        }

        private void OnDestroy()
        {
            if (_material)
            {
                DestroyImmediate(_material);
                _material = null;
            }
        }
        
        private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
        {
            _material.SetFloat("_DarknessValue", _darknessValue);
            Graphics.Blit(sourceTexture, destTexture, _material);
        }
        
        private void SetDarkness(DarknessValues value, object holder)
        {
            var fHolderInfo = _holderInfos.Find(i => i.Holder == holder);
            if (value == DarknessValues.One)
            {
                if (fHolderInfo == null)
                    _holderInfos.Add(fHolderInfo = new HolderInfo(holder));
                fHolderInfo.PlungeIntoDarkness((value));
            }
            else
            {
                if (fHolderInfo != null)
                    _holderInfos.Remove(fHolderInfo);
            }

            UpdateDarknessDerivedValue();
        }

        private void PlungeIntoDarkness(float time, DarknessValues startDarknessValue, DarknessValues endDarknessValue, object holder)
        {
            var fHolderInfo = _holderInfos.Find(i => i.Holder == holder);
            if (fHolderInfo == null)
                _holderInfos.Add(fHolderInfo = new HolderInfo(holder));
            fHolderInfo.PlungeIntoDarkness(time, startDarknessValue, endDarknessValue);

            StartCoroutine(ManualUpdate());
        }

        private void UpdateDarknessDerivedValue()
        {
            float maxDarknessValue = 0.0f;
            for (int hiIndex = 0; hiIndex < _holderInfos.Count; hiIndex++)
            {
                HolderInfo hi = _holderInfos[hiIndex];
                maxDarknessValue = Mathf.Max(hi.GetValue(), maxDarknessValue);
            }

            if (_darknessValue != maxDarknessValue)
            {
                _darknessValue = Mathf.Clamp(maxDarknessValue, 0.0f, 1.0f);
                if (_darknessValue <= float.Epsilon)
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

                for (int hiIndex = _holderInfos.Count - 1; hiIndex >= 0; hiIndex--)
                {
                    var holderInfo = _holderInfos[hiIndex];
                    holderInfo.Update(Time.deltaTime);
                    if (holderInfo.CalculationCompleted)
                    {
                        if (holderInfo.EndDarknessValue == DarknessValues.Zero)
                            _holderInfos.RemoveAt(hiIndex);
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
        
        public static float ConvertDarknessValue(DarknessValues value)
        {
            if (value == DarknessValues.One)
                return 1.0f;

            return 0.0f;
        }
        
        public enum DarknessValues
        {
            Zero,
            One,
        }
        
        private class HolderInfo
        {
            public readonly object Holder;
            private float _timer;
            private float _time;
            private DarknessValues _startDarknessValue;
            private DarknessValues _endDarknessValue;

            public bool CalculationCompleted { get; private set; }
            public DarknessValues EndDarknessValue { get { return _endDarknessValue; } }

            public HolderInfo(object holder)
            {
                Holder = holder;
            }
            
            public void PlungeIntoDarkness(float time, DarknessValues startDarknessValue, DarknessValues endDarknessValue)
            {
                _timer = 0.0f;
                _time = time;
                _startDarknessValue = startDarknessValue;
                _endDarknessValue = endDarknessValue;

                CalculationCompleted = false;
            }
            
            public void PlungeIntoDarkness(DarknessValues darknessValue)
            {
                _timer = 0.0f;
                _time = 0;
                _startDarknessValue = darknessValue;
                _endDarknessValue = darknessValue;

                CalculationCompleted = true;
            }

            public float GetValue()
            {
                if (_time > float.Epsilon)
                {
                    float param = Mathf.Clamp01(_timer / _time);
                    return Mathf.Lerp(ConvertDarknessValue(_startDarknessValue), ConvertDarknessValue(_endDarknessValue), param);
                }

                return ConvertDarknessValue(_endDarknessValue);
            }
            
            public void Update(float dTime)
            {
                _timer += dTime;
                if (_timer >= _time)
                    CalculationCompleted = true;
            }
        }
    }
}