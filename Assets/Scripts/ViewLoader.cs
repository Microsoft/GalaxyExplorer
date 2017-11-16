// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GalaxyExplorer
{
    public delegate void SceneLoaded(GameObject content, string oldSceneName);

    public class ViewLoader : SingleInstance<ViewLoader>
    {
        public static float AudioFadeoutTime = 1.0f;

        public string StartingView;
        public MovableAudioSource movableAudioSource;

        public string CurrentView
        {
            get; private set;
        }

        public event Action CoreSystemsLoaded
        {
            add
            {
                if (coreSystemsLoaded)
                {
                    value();
                }
                else
                {
                    _coreSystemsLoaded += value;
                }
            }

            remove
            {
                _coreSystemsLoaded -= value;
            }
        }

        private string CoreSystems = "CoreSystems";
        private string Volume = "Volume";
        private Stack<string> viewBackStack = new Stack<string>();
        private GameObject VolumeGameObject;
        private bool coreSystemsLoaded = false;
        private AudioSource transitionAudioSource;

        private event Action _coreSystemsLoaded;

        private const string IdleState = "Base Layer.Idle";
        private const string IntroState = "Base Layer.Intro";
        private const string OutroState = "Base Layer.Outro";

        protected void Awake()
        {
            if (ViewLoader.Instance != this)
            {
                DestroyObject(gameObject);
                return;
            }

            transitionAudioSource = GetComponent<AudioSource>();
        }

        private IEnumerator Start()
        {
            // Setting orientation to landscape for platforms that respect
            // window orientation like mobile.
            UnityEngine.Screen.orientation = ScreenOrientation.Landscape;

            yield return StartCoroutine(LoadCoreSystemsAsync());
            ToolManager.Instance.HideTools(instant: true);
        }

        public GameObject GetCurrentContent()
        {
            GameObject currentContent = GameObject.Find(CurrentView + "Content");

            if (currentContent != null)
            {
                return currentContent;
            }

            return GameObject.Find("Content");
        }

        public GameObject GetHeroView()
        {
            GameObject heroView = GameObject.Find("HeroView");

            if (heroView == null)
            {
                heroView = GetCurrentContent();
                //Debug.LogWarning("Couldn't find HeroView in Scene. Falling back to the Current Content");
            }

            return heroView;
        }

        public IEnumerator LoadStartingView()
        {
#if NETFX_CORE
        System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.LowLatency;
#endif

            TransitionManager.Instance.PreLoadScene("SolarSystemView");

            while (TransitionManager.Instance.InTransition)
            {
                yield return null;
            }

            // wait a frame after the solar system is done preloading, so other calls can respond to the transition being complete
            // (i.e.: IntroductionState.IntroductionStatePreloadSolarSystem waits to play VO after the preload)
            yield return null;

            Debug.Log("Loading Starting View...");
            TransitionManager.Instance.LoadNextScene(StartingView, null);
        }

        public void ShowVolume()
        {
            if (VolumeGameObject)
            {
                VolumeGameObject.SetActive(true);
            }
        }

        public void HideVolume()
        {
            if (VolumeGameObject)
            {
                VolumeGameObject.SetActive(false);
            }
        }

        public bool GoBack()
        {
            bool didGoBack = false;

            if (viewBackStack.Count > 0 && !TransitionManager.Instance.InTransition)
            {
                string viewToLoad = viewBackStack.Pop();

                if (!string.IsNullOrEmpty(viewToLoad))
                {
                    TransitionManager.Instance.LoadPrevScene(viewToLoad);
                    didGoBack = true;
                }

                if (viewBackStack.Count == 0)
                {
                    ToolManager.Instance.HideBackButton();
                }
            }

            return didGoBack;
        }

        private IEnumerator LoadCoreSystemsAsync()
        {
            // It seems that Unity doesn't always performs well when we try to load a new scene
            // on the first frame, so we want a single frame before trying anything
            yield return null;

            GameObject coreSystems = GameObject.Find(CoreSystems);

            if (coreSystems == null)
            {
                var coreSystemsOp = SceneManager.LoadSceneAsync(CoreSystems, LoadSceneMode.Additive);

                while (!coreSystemsOp.isDone)
                {
                    yield return new WaitForEndOfFrame();
                }

                // If we are on Windows Mixed Reality, load scene with MR-specific stuff
                if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD)
                {
                    coreSystemsOp = SceneManager.LoadSceneAsync(String.Format("{0}_ImmersiveHMD", CoreSystems), LoadSceneMode.Additive);

                    while (!coreSystemsOp.isDone)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
            }

            if (!coreSystemsLoaded)
            {
                coreSystemsLoaded = true;

                if (_coreSystemsLoaded != null)
                {
                    _coreSystemsLoaded();
                }
            }

            VolumeGameObject = GameObject.Find(Volume);
            HideVolume();

#if UNITY_EDITOR
            // make it easier to inspect everything in the editor
            foreach (var camera in FindObjectsOfType<Camera>())
            {
                camera.nearClipPlane = 0.001f;
            }
#endif
        }

        public void LoadViewAsync(string viewName, bool forwardNavigation = false, SceneLoaded sceneLoadedCallback = null)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                Debug.LogError("ViewLoader: no scene name specified when calling LoadViewAsync() - cannot load the scene");
                return;
            }

            if (forwardNavigation && CurrentView != null && !IntroductionFlow.Instance.enabled)
            {
                viewBackStack.Push(CurrentView);
                ToolManager.Instance.ShowBackButton();
            }

            StartCoroutine(LoadViewAsyncInternal(viewName, sceneLoadedCallback));
        }

        private IEnumerator LoadViewAsyncInternal(string viewName, SceneLoaded sceneLoadedCallback)
        {
            ToolManager.Instance.HideTools();

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(viewName, LoadSceneMode.Additive);

            if (loadOperation == null)
            {
                throw new InvalidOperationException(string.Format("ViewLoader: Unable to load {0}. Make sure that the scene is enabled in the Build Settings.", viewName));
            }

            string oldView = CurrentView;
            GameObject oldContent = GameObject.Find(oldView + "Content");

            while (!loadOperation.isDone)
            {
                yield return null;
            }

            Debug.Log("ViewLoader: Loaded " + viewName);

            CurrentView = viewName;

            GameObject newContent = GameObject.Find("Content");

            if (oldContent)
            {
                ContentView contentView = oldContent.GetComponent<ContentView>();
                if (contentView)
                {
                    contentView.WillUnload();
                }
            }

            if (newContent)
            {
                newContent.name = viewName + "Content";
                newContent.transform.position = transform.position;
                newContent.transform.rotation = transform.rotation;
                newContent.transform.SetParent(gameObject.transform, true);
            }

            if (ToolManager.Instance)
            {
                ToolManager.Instance.UnselectAllTools();
            }

            if (sceneLoadedCallback != null)
            {
                sceneLoadedCallback(newContent, oldView);
            }
        }

        public void UnloadView(string viewName)
        {
            // content's parent is no longer the scene that was loaded, so it is not unloaded with the scene - ensure that it is freed here
            GameObject content = GameObject.Find(viewName + "Content");
            if (content != null)
            {
                Destroy(content);
            }

            SceneManager.UnloadSceneAsync(viewName);
        }

        public void ActivateContent(GameObject newContent)
        {
            if (newContent)
            {
                if (movableAudioSource)
                {
                    movableAudioSource.Setup(newContent.transform.position, Camera.main.transform.position);
                    movableAudioSource.Activate();
                }

                if (transitionAudioSource)
                {
                    transitionAudioSource.Play();
                }
            }
        }

        private bool CheckAnimationState(Animator animator, string state)
        {
            bool currentState = false;
            if (animator && !string.IsNullOrEmpty(state))
            {
                currentState = animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash(state);
            }

            return currentState;
        }

        public IEnumerator UnloadAfterAnimation(Animator animator, string viewName)
        {
            if (animator)
            {
                bool hasStarted = false;
                bool hasFinished = false;

                while (!hasStarted || !hasFinished)
                {
                    if (CheckAnimationState(animator, OutroState))
                    {
                        hasStarted = true;
                    }

                    if (hasStarted &&
                        CheckAnimationState(animator, IdleState))
                    {
                        hasFinished = true;
                    }

                    yield return new WaitForEndOfFrame();
                }
            }

            UnloadView(viewName);

            Debug.Log("Unloaded " + viewName);
        }

        public void SetTransitionSFX(AudioClip stationarySFX, AudioClip movingSFX)
        {
            if (transitionAudioSource)
            {
                transitionAudioSource.clip = stationarySFX;
            }

            if (movableAudioSource)
            {
                var audioSource = movableAudioSource.GetComponent<AudioSource>();
                if (audioSource)
                {
                    audioSource.clip = movingSFX;
                }
            }
        }
    }
}