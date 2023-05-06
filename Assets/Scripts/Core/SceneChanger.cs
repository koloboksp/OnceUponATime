using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Effects;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

namespace Assets.Scripts.Core
{
    public class SceneChanger : MonoBehaviour
    {  
        public string NextLevelName = "";
        public string CurrentLevelName = "";

        public bool UnpausedAfter = true;
        public float WaitBeforeTime = 1.0f;
        public float DarknesEffectTime = 0.25f;

        private AsyncOperation mLoadSceneResult;
        private string mLoadingSceneName;
        private Scene mLoadingScene;
        private SceneChangerLoadingBar mLoadingBar;
        private List<CanvasSettings> mLoadedSceneSavedCanvasSettings;

        public object Sender;
        public object UserData;

        public event Action<SceneChanger> OnComplete;

        private void StartChange(string nextLevelName, string currentLevelName, bool unpauseAfter, object sender, object userData, float waitBeforeTime, float darknesEffectTime)
        {
            NextLevelName = nextLevelName;
            CurrentLevelName = currentLevelName;

            UnpausedAfter = unpauseAfter;
            Sender = sender;
            UserData = userData;
            WaitBeforeTime = waitBeforeTime;
            DarknesEffectTime = darknesEffectTime;

            StartCoroutine(Execute());
        }

        private IEnumerator Execute()
        {
            CameraDarkness();
            yield return new WaitForSeconds(DarknesEffectTime);

            WaitForOldLevelUnloaded();
            while (!mOldSceneUnloaded)
                yield return null;

            StartLoadingLevel();
            while (!(mNewSceneLoaded && mObjectsOfNewSceneLoaded))
            {
                CheckLevelLoading();
                yield return null;
            }

            WaitForTransitionSceneUnloaded();
            while (!mTransitionSceneUnloaded)
                yield return null;

            CameraBrighten();
            yield return new WaitForSeconds(DarknesEffectTime);

            Logic_OnComplete();
        }

        private void CameraDarkness()
        {
            Scene currentActiveScene = SceneManager.GetActiveScene();
            var findedCameras = Camera.allCameras;
            foreach (Camera findedCamera in findedCameras)
            {
                if (findedCamera.gameObject.scene == currentActiveScene)
                {
                    CameraDarknessEffect.PlungeIntoDarkness(findedCamera, DarknesEffectTime, CameraDarknessEffect.DarknessValues.Zero, CameraDarknessEffect.DarknessValues.One, this);
                }
            }

            GameObject[] rootGameObjects = currentActiveScene.GetRootGameObjects();
            foreach (GameObject rootGameObjectForCanvas in rootGameObjects)
            {
                Canvas findedCanvas = rootGameObjectForCanvas.GetComponent<Canvas>();
                if (findedCanvas != null)
                {
                    foreach (GameObject rootGameObjectForCamera in rootGameObjects)
                    {
                        Camera findedCamera = rootGameObjectForCamera.GetComponentInChildren<Camera>();
                        if (findedCamera != null)
                        {
                            findedCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                            findedCanvas.worldCamera = findedCamera;
                        }
                    }
                }
            }
        }

        private void WaitForOldLevelUnloaded()
        {
            Scene previousActiveScene = SceneManager.GetActiveScene();

            Scene transitionScene = SceneManager.CreateScene("TransitionScene");
            GameObject transitionSceneCameraObj = new GameObject("Camera");
            SceneManager.MoveGameObjectToScene(transitionSceneCameraObj, transitionScene);
            Camera transitionCamera = transitionSceneCameraObj.AddComponent<Camera>();
            transitionCamera.depth = float.MaxValue;

            GameObject backgroundPrefab = Resources.Load<GameObject>("BuildIn/Objects/BackgroundOfTransitionScene");
            GameObject backgroundInstance = GameObject.Instantiate(backgroundPrefab);
            mLoadingBar = backgroundInstance.GetComponent<SceneChangerLoadingBar>();
            SceneManager.MoveGameObjectToScene(backgroundInstance, transitionScene);

            SceneManager.SetActiveScene(transitionScene);

            AsyncOperation unloadSceneAsync = SceneManager.UnloadSceneAsync(previousActiveScene);
            unloadSceneAsync.completed += UnloadSceneAsyncOnCompleted;
        }
        private void UnloadSceneAsyncOnCompleted(AsyncOperation asyncOperation)
        {
            mOldSceneUnloaded = true;
        }

        private bool mOldSceneUnloaded = false;
        private bool mNewSceneLoaded = false;
        private bool mObjectsOfNewSceneLoaded = false;

        private bool mTransitionSceneUnloaded = false;

        private void StartLoadingLevel()
        {
            mLoadingSceneName = NextLevelName;
            mLoadSceneResult = SceneManager.LoadSceneAsync(mLoadingSceneName, LoadSceneMode.Additive);

            if (mLoadSceneResult == null)
            {
                mLoadingSceneName = CurrentLevelName;
                mLoadSceneResult = SceneManager.LoadSceneAsync(CurrentLevelName, LoadSceneMode.Additive);
            }


            mLoadSceneResult.allowSceneActivation = true;
            mLoadSceneResult.completed += LoadSceneResultOnCompleted;
        }

        private void LoadSceneResultOnCompleted(AsyncOperation asyncOperation)
        {
            mNewSceneLoaded = true;
            mLoadingScene = SceneManager.GetSceneByName(mLoadingSceneName);
        }

        private void CheckLevelLoading()
        {
            if (mLoadingScene.IsValid())
            {
                ChangeCanvasSettins(mLoadingScene, out mLoadedSceneSavedCanvasSettings);
                mObjectsOfNewSceneLoaded = true;
            }
            else
            {
                if (mLoadingBar != null)
                    mLoadingBar.SetProgress(mLoadSceneResult.progress);
            }
        }

        public class CanvasSettings
        {
            public Canvas Target;
            public RenderMode RenderMode;
            public Camera Camera;
        }

        public static void ChangeCanvasSettins(Scene scene, out List<CanvasSettings> canvasSavedSettings)
        {
            canvasSavedSettings = new List<CanvasSettings>();
            GameObject[] rootGameObjects = scene.GetRootGameObjects();

            foreach (GameObject rootGameObjectForCanvas in rootGameObjects)
            {
                Canvas findedCanvas = rootGameObjectForCanvas.GetComponent<Canvas>();
                if (findedCanvas != null)
                {
                    foreach (GameObject rootGameObjectForCamera in rootGameObjects)
                    {
                        Camera foundCamera = rootGameObjectForCamera.GetComponentInChildren<Camera>();
                        if (foundCamera != null)
                        {
                            canvasSavedSettings.Add(new CanvasSettings() { Target = findedCanvas, RenderMode = findedCanvas.renderMode, Camera = findedCanvas.worldCamera });

                            findedCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                            if(findedCanvas.planeDistance > 2.0f)
                                Debug.LogError("planeDistance required value less oe equal 2.0f.");

                            findedCanvas.worldCamera = foundCamera;
                        }
                    }
                }

            }
        }

        public static void RestoreCanvasSettings(List<CanvasSettings> canvasSavedSettings)
        {
            foreach (CanvasSettings canvasSettings in canvasSavedSettings)
            {
                try
                {
                    canvasSettings.Target.renderMode = canvasSettings.RenderMode;
                    canvasSettings.Target.worldCamera = canvasSettings.Camera;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

            }
        }


        private void WaitForTransitionSceneUnloaded()
        {
            Scene previousActiveScene = SceneManager.GetActiveScene();

            Camera findedCameraInLoadedScene = CameraDarknessEffect.FindCameraInScene(mLoadingScene);
            findedCameraInLoadedScene.enabled = true;

            CameraDarknessEffect.PlungeIntoDarkness(findedCameraInLoadedScene, CameraDarknessEffect.DarknessValues.One, this);
            SceneManager.SetActiveScene(mLoadingScene);

            AsyncOperation unloadSceneAsync = SceneManager.UnloadSceneAsync(previousActiveScene);
            unloadSceneAsync.completed += TransitionSceneUnloaded_OnCompleted;
        }

        private void TransitionSceneUnloaded_OnCompleted(AsyncOperation asyncOperation)
        {
            mTransitionSceneUnloaded = true;
        }

        private void CameraBrighten()
        {
            Camera findedCameraInLoadedScene = null;
            GameObject[] rootGameObjects = mLoadingScene.GetRootGameObjects();
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                Camera findedCamera = rootGameObject.GetComponentInChildren<Camera>();
                if (findedCamera != null)
                {
                    findedCameraInLoadedScene = findedCamera;
                    break;
                }
            }
            CameraDarknessEffect.PlungeIntoDarkness(findedCameraInLoadedScene, DarknesEffectTime, CameraDarknessEffect.DarknessValues.One, CameraDarknessEffect.DarknessValues.Zero, this);
        }
        private void Logic_OnComplete()
        {
            RestoreCanvasSettings(mLoadedSceneSavedCanvasSettings);

            if (OnComplete != null)
                OnComplete(this);

            Destroy(this.gameObject, 3.0f);
        }
       


        public static SceneChanger Change(string nextLevelName, string currentLevelName, bool unpause, object sender, object userData, float waitBeforeTime, float darknesEffectTime)
        {
            mInfos.Add(new SceneChangeInfo(currentLevelName, nextLevelName, sender, userData));

            GameObject levelChangerObj = new GameObject("(System)LevelChanger to :" + nextLevelName);
            DontDestroyOnLoad(levelChangerObj);

            SceneChanger sceneChanger = levelChangerObj.AddComponent<SceneChanger>();
            sceneChanger.StartChange(nextLevelName, currentLevelName, unpause, sender, userData, waitBeforeTime, darknesEffectTime);

            return sceneChanger;
        }

        private static List<SceneChangeInfo> mInfos = new List<SceneChangeInfo>();

        public static SceneChangeInfo LastInfo
        {
            get
            {
                if (mInfos.Count > 0)
                    return mInfos[mInfos.Count - 1];

                return null;
            }
        }

        public class SceneChangeInfo
        {
            public readonly string FromSceneName;
            public readonly string ToSceneName;
            public readonly object Sender;
            public readonly object UserData;

            public SceneChangeInfo(string fromSceneName, string toSceneName, object sender, object userData)
            {
                FromSceneName = fromSceneName;
                ToSceneName = toSceneName;
                Sender = sender;
                UserData = userData;
            }
        }
    }
}