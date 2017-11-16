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
    public class TransitionManager : SingleInstance<TransitionManager>
    {
        public enum FadeType
        {
            FadeIn,
            FadeOut,
            FadeUnload
        }

        public enum SpeedType
        {
            SpeedUp,
            SlowDown
        }

        [Serializable]
        public struct AudioTransition
        {
            public AudioClip StaticClip;
            public AudioClip MovingClip;

            public AudioTransition(AudioClip staticClip, AudioClip movingClip)
            {
                StaticClip = staticClip;
                MovingClip = movingClip;
            }
        }

        private bool isIntro = true;

        public bool IsIntro
        {
            get
            {
                return isIntro;
            }

            set
            {
                if (isIntro != value)
                {
                    isIntro = value;

                    if (isIntro == false && preLoadedContent != null)
                    {
                        Destroy(preLoadedContent);
                    }
                }
            }
        }

        public GameObject ViewVolume;
        public GameObject Tools;
        public GameObject Cube;
        private HoloCube holoCube;

        [Header("To Fullscreen Transition")]
        [Tooltip("The time it takes to complete a transition content from 'tesseract' to 'full screen' view.")]
        public float TransitionTimeFullscreen = 3.0f;
        [Tooltip("The opacity animation used to hide the box and UI controls when transitioning from the cube to fullscreen.")]
        public AnimationCurve OpacityCurveFullscreen;
        [Tooltip("The curve that defines how content transitions to fullscreen.")]
        public AnimationCurve TransitionCurveFullscreen;

        [Header("To Cube Transition")]
        [Tooltip("The time it takes to complete a transition content from 'full screen' to 'tesseract' view.")]
        public float TransitionTimeCube = 3.0f;
        [Tooltip("The opacity animation used to make the box and UI controls visible when transition from fullscreen to the cube.")]
        public AnimationCurve OpacityCurveCube;
        [Tooltip("The curve that defines how content transitions to the cube from fullscreen.")]
        public AnimationCurve TransitionCurveCube;

        [Header("Skybox Fade Out")]
        [Tooltip("The time it takes for a skybox to fade out.")]
        public float TransitionTimeSkyboxFadeOut = 0.5f;
        [Tooltip("The opacity animation from completely visible to hidden that happens as soon as a transition starts.")]
        public AnimationCurve OpacityCurveSkyboxFadeOut;

        [Header("Scene Transitions")]
        [Tooltip("The first time the galaxy appears, this defines how the scene moves into position.")]
        public AnimationCurve IntroTransitionCurveContentChange;
        [Tooltip("The curve that defines how content moves when transitioning from the galaxy to the solar system scene.")]
        public AnimationCurve GalaxyToSSTransitionCurveContentChange;
        [Tooltip("The curve that defines how content moves when transitioning from the solar system to the galaxy.")]
        public AnimationCurve SSToGalaxyTransitionCurveContentChange;
        [Tooltip("The curve that defines how content moves when transitioning from the solar system to a planet or the sun.")]
        public AnimationCurve SSToPlanetTransitionCurveContentChange;
        [Tooltip("The curve that defines how content moves (position and scale only) when transitioning from a planet or the sun to the solar system.")]
        public AnimationCurve PlanetToSSPositionScaleCurveContentChange;
        [Tooltip("The curve that defines how content moves (rotation only) when transitioning from a planet or the sun to the solar system.")]
        public AnimationCurve PlanetToSSRotationCurveContentChange;

        [Header("FirstScene")]
        [Tooltip("When the galaxy first loads, this controls the opacity of the galaxy (uses TransitionTimeOpeningScene for timing).")]
        public AnimationCurve OpacityCurveFirstScene;

        [Header("OpeningScene")]
        [Tooltip("The time it takes to fully transition from one scene opening and getting into position at the center of the cube or room.")]
        public float TransitionTimeOpeningScene = 3.0f;
        [Tooltip("Drives the opacity of the new scene that was loaded in when transitioning backwards.")]
        public AnimationCurve BackTransitionOpacityCurveContentChange;
        [Tooltip("Drives the opacity of the new scene that was loaded when transitioning from planet to solar system view.")]
        public AnimationCurve PlanetToSSTransitionOpacityCurveContentChange;

        [Header("Closing Scene")]
        [Tooltip("How long it takes to completely fade the galaxy scene when transitioning from this scene.")]
        public float GalaxyVisibilityTimeClosingScene = 1.0f;
        [Tooltip("How long it takes to completely fade the solar system scene when transitioning from this scene.")]
        public float SolarSystemVisibilityTimeClosingScene = 1.0f;
        [Tooltip("How long it takes to completely fade a planet or sun scene when transitioning from this scene.")]
        public float PlanetVisibilityTimeClosingScene = 1.0f;
        [Tooltip("Drives the opacity animation for the scene that is closing.")]
        public AnimationCurve OpacityCurveClosingScene;

        [Header("Start Transition")]
        public float StartTransitionTime = 1.0f;
        [Tooltip("Drives the POI opacity animation for the closing scene before content is loaded and starts moving into position.")]
        public AnimationCurve POIOpacityCurveStartTransition;
        public AnimationCurve OrbitSpeedCurveStartTransition;

        [Header("End Transition")]
        [Tooltip("This offset is applied to the time it takes to completely transition, so the end transition can start slightly before content has completely moved into place.")]
        public float EndTransitionTimeOffset = -1.0f;
        [Tooltip("The time it takes for one point of interest to completely fade out and the end of a transition.")]
        public float POIOpacityChangeTimeEndTransition = 1.0f;
        [Tooltip("The time between the previous and next points of interest fading out at the end of a transition.")]
        public float POIOpacityTimeOffsetEndTransition = 0.5f;
        [Tooltip("Drives the POI opacity animation for the opening scene after it has completely moved into place.")]
        public AnimationCurve POIOpacityCurveEndTransition;

        [Header("Skybox Fade In")]
        [Tooltip("The time it takes for the skybox to fade in. The skybox fades in at the end of a Content Change Transition.")]
        public float TransitionTimeSkyboxFadeIn = 0.5f;
        [Tooltip("The opacity animation from hidden to completely visible that happens at the end of a scene transition.")]
        public AnimationCurve OpacityCurveSkyboxFadeIn;

        [Header("Audio Transitions")]
        public AudioTransition GalaxyClips;
        public AudioTransition SolarSystemClips;
        public AudioTransition PlanetClips;
        public AudioTransition BackClips;

        public event Action ContentLoaded;

        public event Action ResetStarted;

        public event Action ResetFinished;

        // tracking data
        private GameObject doNotDisableScene;   // set when a scene transitions to prevent the scene from deactivating because that stops coroutines from running on it
        private GameObject prevSceneLoaded;     // tracks the last scene loaded for transitions when loading new scenes
        private GameObject loadSource;          // when new content is loaded, this is what the viewer selected to bring in that content
        private GameObject preLoadedContent;

        private string sceneToUnload;

        private bool inTransition = false;

        public bool InTransition
        {
            get
            {
                return inTransition;
            }
        }

        private bool fadingPointsOfInterest = false; // prevent content from physically transitioning until POIs have completely faded out

        private void Start()
        {
            if (ViewVolume == null)
            {
                Debug.LogError("TransitionManager: No view volume was specified for the cube view - unable to process transitions.");
                Destroy(this);
                return;
            }

            if (ViewLoader.Instance == null)
            {
                Debug.LogError("TransitionManager: No ViewLoader found - unable to process transitions.");
                Destroy(this);
                return;
            }

            holoCube = ViewVolume.GetComponentInChildren<HoloCube>();
            if (holoCube == null)
            {
                Debug.LogWarning("TransitionManager: No HoloCube found from the ViewVolume - unable to fade in/out the cube skybox.");
            }
        }

        public void ShowToolsAndCursor()
        {
            Tools.SetActive(true);

            // Only show the cursor for HoloLens Gaze input and in the Editor
#if UNITY_EDITOR
            Cursor.Instance.visible = true;
#else
            Cursor.Instance.visible = UnityEngine.XR.XRDevice.isPresent;
#endif
        }

        public void ResetView()
        {
            if (inTransition)
            {
                Debug.LogWarning("TransitionManager: Currently in a transition and cannot change view mode for '" + prevSceneLoaded.scene.name + "' until current transition completes.");
                return;
            }

            if (ResetStarted != null)
            {
                ResetStarted();
            }

            inTransition = true;

            Vector3 desiredPosition;
            Quaternion desiredRotation;
            float desiredScale;
            GetOrientationFromView(out desiredPosition, out desiredRotation, out desiredScale);

            StartCoroutine(TransitionContent(
                prevSceneLoaded,
                desiredPosition,
                desiredRotation,
                desiredScale,
                TransitionTimeCube,
                TransitionCurveCube,
                null, // no rotation
                TransitionCurveCube));

            RotateContentTowardViewer();

            CrossFadeToolbar(TransitionTimeCube);

            TriggerResetFinished(TransitionTimeCube);
        }

        /// <summary>
        /// Will hide to the toolbar for at least transitionTime and then shows it
        /// </summary>
        /// <param name="transitionTime"></param>
        private void CrossFadeToolbar(float transitionTime)
        {
            StartCoroutine(CrossFadeToolbarAsync(transitionTime));
        }

        private void TriggerResetFinished(float transitionTime)
        {
            StartCoroutine(TriggerResetFinishedAsync(transitionTime));
        }

        private IEnumerator CrossFadeToolbarAsync(float transitionTime)
        {
            var startTime = Time.time;
            yield return StartCoroutine(ToolManager.Instance.HideToolsAsync(instant: false));

            var timeLeftToWait = Mathf.Max(0, (Time.time - startTime) - transitionTime);

            if (timeLeftToWait > 0)
            {
                while (timeLeftToWait > 0)
                {
                    timeLeftToWait -= Time.deltaTime;
                    yield return null;
                }
            }

            yield return StartCoroutine(ToolManager.Instance.ShowToolsAsync());
        }

        private IEnumerator TriggerResetFinishedAsync(float transitionTime)
        {
            yield return new WaitForSeconds(transitionTime);

            if (ResetFinished != null)
            {
                ResetFinished();
            }
        }

        private void RotateContentTowardViewer()
        {
            var contentToCamera = Camera.main.transform.position - ViewLoader.Instance.transform.position;
            contentToCamera.y = 0;

            if (contentToCamera.magnitude <= float.Epsilon)
            {
                // We can't normalize that
                return;
            }

            contentToCamera.Normalize();
            var desiredRotation = Quaternion.LookRotation(contentToCamera);

            // Rotating the ViewLoader (box) to face the viewer
            StartCoroutine(TransitionContent(
                ViewLoader.Instance.gameObject,
                Vector3.zero,
                desiredRotation,
                1,
                TransitionTimeCube,
                null,
                TransitionCurveCube, // only rotation
                null));

            // Reset local tilt by rotating the content inside of the box to face the viewer
            StartCoroutine(TransitionContent(
               prevSceneLoaded,
               Vector3.zero,
               Quaternion.identity,
               1,
               TransitionTimeCube,
               null,
               TransitionCurveCube, // only rotation
               null,
               animateInLocalSpace: true));
        }

        private void StartTransitionForNewScene(GameObject source)
        {
            loadSource = source;

            // make sure nothing is fading or moving when we start a transition
            StopAllCoroutines();

            // fade in cube
            if (!ViewVolume.activeInHierarchy)
            {
                ViewVolume.SetActive(true);

                if (!IsIntro)
                {
                    Tools.SetActive(true);
                }

                StartCoroutine(FadeContent(
                    Cube,
                    FadeType.FadeIn,
                    TransitionTimeOpeningScene,
                    OpacityCurveClosingScene));
            }

            if (prevSceneLoaded != null && !IsIntro)
            {
                // fade out points of interest for the current scene
                PointOfInterest[] focusPoints = prevSceneLoaded.GetComponentsInChildren<PointOfInterest>();
                foreach (PointOfInterest focalPoint in focusPoints)
                {
                    // if faders has their coroutines killed, then need to be initialized to a disabled state
                    Fader focalPointFader = focalPoint.GetComponent<Fader>();
                    if (focalPointFader != null)
                    {
                        focalPointFader.DisableFade();
                    }

                    StartCoroutine(FadeContent(
                        focalPoint.gameObject,
                        FadeType.FadeOut,
                        StartTransitionTime,
                        POIOpacityCurveStartTransition));
                }

                // slow down the solar system for the current scene
                StartCoroutine(TransitionOrbitSpeed(
                    prevSceneLoaded,
                    SpeedType.SlowDown,
                    StartTransitionTime,
                    OrbitSpeedCurveStartTransition));

                // this prevents content from starting transitions until points of interest have completely faded out
                fadingPointsOfInterest = focusPoints.Length > 0;
            }
        }

        public void InitializeLoadedContent(GameObject content)
        {
            GameObject contentTarget = content.transform.transform.GetChild(0).gameObject;

            // hide content as soon as it is created
            if (contentTarget.activeInHierarchy)
            {
                // the galaxy does not play nice with being set to inactive, so use the transparency alpha instead to hide it
                SpiralGalaxy[] galaxies = content.GetComponentsInChildren<SpiralGalaxy>(true);
                Fader[] contentFaders = content.GetComponentsInChildren<Fader>(true);
                if (galaxies.Length > 0)
                {
                    foreach (SpiralGalaxy galaxy in galaxies)
                    {
                        galaxy.TransitionAlpha = 0.0f;
                    }

                    foreach (Fader fader in contentFaders)
                    {
                        fader.Hide();
                    }
                }
                else
                {
                    contentTarget.SetActive(false);
                }
            }

            // Disable all colliders during a transition to workaround physics issues with scaling box and mesh colliders down to really small numbers (~0)
            Collider[] colliders = content.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
        }

        public void PreLoadScene(string sceneName)
        {
            inTransition = true;

            ViewLoader.Instance.LoadViewAsync(sceneName, false, PreLoadSceneLoaded);
        }

        private void PreLoadSceneLoaded(GameObject content, string oldSceneName)
        {
            // activate the content target but zero its scale, so we do not see it the first frame it is loaded
            GameObject contentTarget = content.transform.GetChild(0).gameObject;
            contentTarget.SetActive(true);
            contentTarget.transform.localScale = Vector3.zero;

            // delete content that has custom renderers; these preloaded assets will distort actually-loaded solar system assets, so get rid of them
            OrbitalTrail[] trails = content.GetComponentsInChildren<OrbitalTrail>(true);
            foreach (OrbitalTrail trail in trails)
            {
                Destroy(trail.gameObject);
            }

            AsteroidRing[] asteroids = content.GetComponentsInChildren<AsteroidRing>(true);
            foreach (AsteroidRing asteroid in asteroids)
            {
                Destroy(asteroid.gameObject);
            }

            // POIs need to be removed so the batched rendering doesn't show the preloaded POIs
            PointOfInterest[] pois = content.GetComponentsInChildren<PointOfInterest>();
            foreach (PointOfInterest poi in pois)
            {
                Destroy(poi.gameObject);
            }

            // prevent the earth from finding the wrong sun during the transition
            GameObject sun = GameObject.Find("Sun");
            if (sun != null)
            {
                Destroy(sun);
            }

            preLoadedContent = content;
            SceneManager.UnloadSceneAsync(ViewLoader.Instance.CurrentView);

            inTransition = false;
        }

        public void LoadPrevScene(string sceneName)
        {
            if (inTransition)
            {
                Debug.LogWarning("TransitionManager: Currently in a transition and cannot change view to '" + sceneName + "' until current transition completes.");
                return;
            }

            inTransition = true;
            StartTransitionForNewScene(null);

            SwitchAudioClips(sceneName, forwardNavigation: false);
            ViewLoader.Instance.LoadViewAsync(sceneName, false, PrevSceneLoaded);
        }

        private void PrevSceneLoaded(GameObject content, string oldSceneName)
        {
            sceneToUnload = oldSceneName;

            StartCoroutine(PrevSceneLoadedCoroutine(content));
        }

        private IEnumerator PrevSceneLoadedCoroutine(GameObject content)
        {
            WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();

            // wait until introduction animations are complete (fading out points of interest, slowing down orbit speeds, etc.) before transitioning content
            while (fadingPointsOfInterest)
            {
                yield return fixedUpdate;
            }

            // activate the content target but zero its scale, so we do not see it the first frame it is loaded
            GameObject contentTarget = content.transform.GetChild(0).gameObject;
            contentTarget.SetActive(true);
            Vector3 contentTargetLocalScale = contentTarget.transform.localScale;
            contentTarget.transform.localScale = Vector3.zero;

            yield return null;

            Collider[] colliders = content.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }

            yield return null;

            // all content is loaded, so make it visible again
            contentTarget.transform.localScale = contentTargetLocalScale;

            // Play audio
            ViewLoader.Instance.ActivateContent(content);

            // hide loaded content
            Fader[] contentFaders = content.GetComponentsInChildren<Fader>(true);
            foreach (Fader fader in contentFaders)
            {
                fader.Hide();
            }

            PointOfInterest[] focusPoints = content.GetComponentsInChildren<PointOfInterest>(true);
            if (!IsIntro)
            {
                // transition points of interest
                float fadeOffset = TransitionTimeOpeningScene + EndTransitionTimeOffset;
                foreach (PointOfInterest focalPoint in focusPoints)
                {
                    StartCoroutine(FadeContent(
                        focalPoint.gameObject,
                        FadeType.FadeIn,
                        POIOpacityChangeTimeEndTransition,
                        POIOpacityCurveEndTransition,
                        fadeTimeOffset: fadeOffset));

                    if (focalPoint.TargetPoint != null && focalPoint.TargetPoint.transform.parent != null)
                    {
                        StartCoroutine(TransitionOrbitSpeed(
                            focalPoint.TargetPoint.transform.parent.gameObject,
                            SpeedType.SpeedUp,
                            POIOpacityChangeTimeEndTransition,
                            OrbitSpeedCurveStartTransition,
                            transitionTimeOffset: fadeOffset));
                    }

                    fadeOffset += POIOpacityTimeOffsetEndTransition;
                }
            }
            else
            {
                // when the intro runs, the POIs will never fade in; enabling the fader prevents these POIs from fading with the content (fade is in progress- kind of)
                foreach (PointOfInterest focalPoint in focusPoints)
                {
                    Fader[] faders = focalPoint.GetComponentsInChildren<Fader>(true);
                    foreach (Fader fader in faders)
                    {
                        fader.EnableFade();
                    }

                    // start planets not orbiting the sun until all of the content has completed the transitions
                    if (focalPoint.TargetPoint != null && focalPoint.TargetPoint.transform.parent != null)
                    {
                        StartCoroutine(TransitionOrbitSpeed(
                            focalPoint.TargetPoint.transform.parent.gameObject,
                            SpeedType.SpeedUp,
                            POIOpacityChangeTimeEndTransition,
                            OrbitSpeedCurveStartTransition,
                            transitionTimeOffset: TransitionTimeOpeningScene));
                    }
                }
            }

            // fade in the new scene
            AnimationCurve opacityCurve = content.name.Contains("SolarSystemView")
                ? PlanetToSSTransitionOpacityCurveContentChange
                : BackTransitionOpacityCurveContentChange;
            StartCoroutine(FadeContent(
                content,
                FadeType.FadeIn,
                TransitionTimeOpeningScene,
                opacityCurve));

            // find the load source
            PointOfInterest[] possibleSources = content.GetComponentsInChildren<PointOfInterest>(true);
            foreach (PointOfInterest source in possibleSources)
            {
                if (source.TransitionScene == sceneToUnload)
                {
                    loadSource = source.gameObject;
                    break;
                }
            }

            // fit new content to our destination
            Vector3 desiredPosition;
            Quaternion desiredRotation;
            float desiredScale;
            GetOrientationFromView(out desiredPosition, out desiredRotation, out desiredScale);

            // get sun sources from the previous content and the next content
            GameObject prevSun = null;
            GameObject nextSun = null;
            GameObject nextPlanet = null;
            TargetSizer sourceSizer = loadSource.GetComponent<TargetSizer>();
            if (prevSceneLoaded != null && sourceSizer.TargetFillCollider != null)
            {
                SunLightReceiver tempReceiver = prevSceneLoaded.GetComponentInChildren<SunLightReceiver>();
                if (tempReceiver != null)
                {
                    tempReceiver.FindSunIfNeeded();
                    prevSun = tempReceiver.Sun.gameObject;

                    tempReceiver = sourceSizer.TargetFillCollider.gameObject.GetComponentInParent<SunLightReceiver>();
                    if (tempReceiver != null)
                    {
                        tempReceiver.FindSunIfNeeded();
                        nextSun = tempReceiver.Sun.gameObject;
                        nextPlanet = tempReceiver.GetComponentInParent<PlanetTransform>().gameObject;
                    }
                }
                else
                {
                    // the previous planet is the sun
                    prevSun = prevSceneLoaded.GetComponentInChildren<PlanetTransform>().gameObject;
                    nextSun = nextPlanet = sourceSizer.TargetFillCollider.gameObject.transform.parent.gameObject;
                }
            }

            // fade out the old content
            float fadeTime = GetClosingSceneVisibilityTime();
            StartCoroutine(FadeContent(
                prevSceneLoaded,
                FadeType.FadeUnload,
                fadeTime,
                OpacityCurveClosingScene));

            SceneSizer sceneSizer = content.GetComponent<SceneSizer>();
            SceneSizer prevSizer = prevSceneLoaded.GetComponent<SceneSizer>();
            AnimationCurve transitionCurve = GetContentTransitionCurve(content.name);
            AnimationCurve rotationCurve = GetContentRotationCurve(content.name);

            // planet view to solar system: position both of the suns (light sources) in the same spot for the new content but keep the
            // rotation of the planet matching the planet in the solar system
            Quaternion nextPlanetRotation = Quaternion.identity;
            Quaternion nextPlanetFlareLocalRotation = Quaternion.identity;
            if (prevSun != null && nextSun != null && nextPlanet != null)
            {
                PlanetTransform prevPlanet = prevSceneLoaded.GetComponentInChildren<PlanetTransform>();
                if (prevSun != prevPlanet.gameObject)
                {
                    // scale new content to match the target sizer to the previously loaded scene's sizer (reverse moving to the next scene to determine scale only)
                    Vector3 oldPosition = prevSizer.gameObject.transform.position;
                    Quaternion oldRotation = prevSizer.gameObject.transform.rotation;
                    Vector3 oldLocalScale = prevSizer.gameObject.transform.localScale;
                    float oldScale = Mathf.Max(prevSizer.gameObject.transform.localScale.x, prevSizer.gameObject.transform.localScale.y, prevSizer.gameObject.transform.localScale.z);
                    prevSizer.FitToTarget(sourceSizer, useCollider: prevSun == nextSun);
                    float newScale = Mathf.Max(prevSizer.gameObject.transform.localScale.x, prevSizer.gameObject.transform.localScale.y, prevSizer.gameObject.transform.localScale.z);
                    prevSizer.gameObject.transform.position = oldPosition;
                    prevSizer.gameObject.transform.rotation = oldRotation;
                    prevSizer.gameObject.transform.localScale = oldLocalScale;
                    float scale = Mathf.Max(content.transform.localScale.x, content.transform.localScale.y, content.transform.localScale.z) * oldScale / newScale;
                    content.transform.localScale = new Vector3(scale, scale, scale);
                }
                else
                {
                    float scale = sceneSizer.GetScalar(desiredScale) * prevSizer.GetScalar() / (sourceSizer.GetScalar() * sceneSizer.FullScreenFillPercentage);
                    content.transform.localScale = new Vector3(scale, scale, scale);
                }

                Vector3 prevToPlanet = prevPlanet.transform.position - prevSun.transform.position;
                Vector3 nextToPlanet = nextPlanet.transform.position - nextSun.transform.position;
                Quaternion matchContentRotation = Quaternion.FromToRotation(nextToPlanet, prevToPlanet);
                content.transform.rotation = matchContentRotation * content.transform.rotation;

                // the planet rotating may not be at the same level between scenes, so ensure that we are looking for the first rotating object to match rotation
                ConstantRotateAxis prevPlanetRotater = prevSceneLoaded.GetComponentInChildren<ConstantRotateAxis>();
                if (prevPlanetRotater != null)
                {
                    nextPlanetRotation = prevPlanetRotater.transform.rotation;
                }
                else
                {
                    nextPlanetRotation = prevPlanet.transform.rotation;
                }

                // the planet may have an additional rotation on it to make it more interesting- keep that rotation across transitions too
                ConstantRotate prevPlanetFlareRotater = prevSceneLoaded.GetComponentInChildren<ConstantRotate>();
                if (prevPlanetFlareRotater != null)
                {
                    nextPlanetFlareLocalRotation = prevPlanetFlareRotater.transform.localRotation;
                }

                if (prevSun != prevPlanet.gameObject)
                {
                    content.transform.position = desiredPosition + prevPlanet.transform.position -
                        (sourceSizer.TargetFillCollider == null ? sourceSizer.transform.position : sourceSizer.TargetFillCollider.transform.position);
                }
                else
                {
                    content.transform.position = desiredPosition + nextSun.transform.position - nextPlanet.transform.position;
                }

                prevPlanet.gameObject.SetActive(false);
                Fader nextPlanetFader = nextPlanet.GetComponent<Fader>();
                nextPlanetFader.SetAlpha(1.0f);
                nextPlanetFader.DisableFade();

                // parent the old and new scenes
                if (prevSun != prevPlanet.gameObject)
                {
                    prevSceneLoaded.transform.SetParent(sourceSizer.TargetFillCollider.transform.parent, true);
                }
                else
                {
                    prevSceneLoaded.transform.SetParent(content.transform, true);
                }
            }
            else
            {
                float scale = sceneSizer.GetScalar(desiredScale) * prevSizer.GetScalar() / (sourceSizer.GetScalar() * sceneSizer.FullScreenFillPercentage);
                content.transform.localScale = new Vector3(scale, scale, scale);
                content.transform.rotation = desiredRotation;
                content.transform.position = desiredPosition - sourceSizer.GetPosition(scale);

                // parent the old and new scenes
                prevSceneLoaded.transform.SetParent(content.transform, true);
            }

            // the rotation of the planet in the solar system is independent of its relationshipt with the sun (to align with the previous scene's planet)
            // and the rotation has to be set after it is parented to match rotations
            if (nextPlanet != null)
            {
                Quaternion finalRotation = desiredRotation * Quaternion.Inverse(content.transform.rotation) * nextPlanet.transform.rotation;
                nextPlanet.transform.rotation = nextPlanetRotation;

                if (nextPlanetFlareLocalRotation != Quaternion.identity)
                {
                    ConstantRotate nextPlanetFlare = nextPlanet.GetComponentInChildren<ConstantRotate>();
                    if (nextPlanetFlare != null)
                    {
                        nextPlanetFlare.transform.localRotation = nextPlanetFlareLocalRotation;
                    }
                }

                StartCoroutine(TransitionContent(
                    nextPlanet,
                    Vector3.zero,
                    finalRotation,
                    0.0f,
                    TransitionTimeOpeningScene,
                    null,
                    rotationCurve,
                    null));
            }

            // transition in new content
            StartCoroutine(TransitionContent(
                content,
                desiredPosition,
                desiredRotation,
                desiredScale,
                TransitionTimeOpeningScene,
                transitionCurve,
                rotationCurve,
                transitionCurve));

            doNotDisableScene = prevSceneLoaded;
            prevSceneLoaded = content;
        }

        public void LoadNextScene(string sceneName, GameObject sourceObject)
        {
            if (inTransition)
            {
                Debug.LogWarning("TransitionManager: Currently in a transition and cannot change view to '" + sceneName + "' until current transition completes.");
                return;
            }

            inTransition = true;
            StartTransitionForNewScene(sourceObject);

            SwitchAudioClips(sceneName);
            ViewLoader.Instance.LoadViewAsync(sceneName, true, NextSceneLoaded);
        }

        private void NextSceneLoaded(GameObject content, string oldSceneName)
        {
            sceneToUnload = oldSceneName;

            StartCoroutine(NextSceneLoadedCoroutine(content));
        }

        private IEnumerator NextSceneLoadedCoroutine(GameObject content)
        {
            WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();

            // wait until introduction animations are complete (fading out points of interest, slowing down orbit speeds, etc.) before transitioning content
            while (fadingPointsOfInterest)
            {
                yield return fixedUpdate;
            }

            // activate the content target but zero its scale, so we do not see it the first frame it is loaded
            GameObject contentTarget = content.transform.GetChild(0).gameObject;
            contentTarget.SetActive(true);
            Vector3 contentTargetLocalScale = contentTarget.transform.localScale;
            contentTarget.transform.localScale = Vector3.zero;

            yield return null;

            Collider[] colliders = content.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }

            yield return null;

            // all content has been loaded, so make it visible again
            contentTarget.transform.localScale = contentTargetLocalScale;

            ViewLoader.Instance.ActivateContent(content);

            SceneSizer sceneSizer = content.GetComponent<SceneSizer>();
            TargetSizer sourceSizer = null;

            if (!IsIntro)
            {
                // fade in points of interest for the new content (must happen before parenting scenes)
                float fadeOffset = TransitionTimeOpeningScene + EndTransitionTimeOffset;
                PointOfInterest[] focusPoints = content.GetComponentsInChildren<PointOfInterest>(true);
                foreach (PointOfInterest focalPoint in focusPoints)
                {
                    // new content points of interest start hidden, so we can fade them in from nothing
                    var focalPointFaders = focalPoint.GetComponentsInChildren<Fader>();
                    if (focalPointFaders != null && focalPointFaders.Length > 0)
                    {
                        foreach (var fader in focalPointFaders)
                        {
                            fader.Hide();
                        }
                    }

                    StartCoroutine(FadeContent(
                        focalPoint.gameObject,
                        FadeType.FadeIn,
                        POIOpacityChangeTimeEndTransition,
                        POIOpacityCurveEndTransition,
                        fadeTimeOffset: fadeOffset));

                    if (focalPoint.TargetPoint != null && focalPoint.TargetPoint.transform.parent != null)
                    {
                        StartCoroutine(TransitionOrbitSpeed(
                            focalPoint.TargetPoint.transform.parent.gameObject,
                            SpeedType.SpeedUp,
                            POIOpacityChangeTimeEndTransition,
                            OrbitSpeedCurveStartTransition,
                            transitionTimeOffset: fadeOffset));
                    }

                    fadeOffset += POIOpacityTimeOffsetEndTransition;
                }
            }

            // the first scene loaded, scale the galaxy from the full room view to the cube (default target size)
            if (prevSceneLoaded == null)
            {
                // place the box at the view
                ViewVolume.transform.position = ViewLoader.Instance.transform.position;
                ViewVolume.transform.rotation = ViewLoader.Instance.transform.rotation;

                // hide the skybox (does not show when we first go into the galaxy)
                if (holoCube != null)
                {
                    holoCube.SetActve(false);
                }

                // fade in new content
                StartCoroutine(FadeContent(
                    content,
                    FadeType.FadeIn,
                    TransitionTimeOpeningScene,
                    OpacityCurveFirstScene));

                // start the galaxy much larger than the start position
                content.transform.position = ViewLoader.Instance.transform.position;
                content.transform.rotation = ViewLoader.Instance.transform.rotation;
                content.transform.localScale = new Vector3(10.0f, 10.0f, 10.0f);
            }
            else if (loadSource != null)
            {
                sourceSizer = loadSource.GetComponent<TargetSizer>();
            }

            // get sun sources from the previous content and the next content
            Quaternion originalPlanetRotation = content.transform.rotation;
            GameObject prevSun = null;
            GameObject nextSun = null;
            GameObject nextPlanet = null;
            if (prevSceneLoaded != null)
            {
                SunLightReceiver tempReceiver = prevSceneLoaded.GetComponentInChildren<SunLightReceiver>();
                if (tempReceiver != null)
                {
                    tempReceiver.FindSunIfNeeded();
                    prevSun = tempReceiver.Sun.gameObject;

                    tempReceiver = content.GetComponentInChildren<SunLightReceiver>();
                    if (tempReceiver != null)
                    {
                        tempReceiver.FindSunIfNeeded();
                        nextSun = tempReceiver.Sun.gameObject;
                        nextPlanet = tempReceiver.GetComponentInParent<PlanetTransform>().gameObject;
                        originalPlanetRotation = nextPlanet.transform.rotation;
                    }
                    else
                    {
                        // this is the sun, so the planet and sun targets are the same
                        nextPlanet = nextSun = content.GetComponentInChildren<PlanetTransform>().gameObject;
                        originalPlanetRotation = nextPlanet.transform.rotation;
                    }
                }
            }

            // scale the newly loaded scene to the source
            if (sceneSizer != null && sourceSizer != null)
            {
                sceneSizer.FitToTarget(sourceSizer, useCollider: nextSun == nextPlanet);
            }

            // calculated the desired state of the transitioning content parent
            float desiredScale;
            Vector3 desiredPosition;
            Quaternion desiredRotation;
            GetOrientationFromView(out desiredPosition, out desiredRotation, out desiredScale);

            // parent the old and new scenes
            AnimationCurve transitionCurve = GetContentTransitionCurve(content.name);
            if (prevSceneLoaded != null)
            {
                // solar system to planet view: position both of the suns (light sources) in the same spot for the new content but keep the
                // rotation of the planet matching the planet in the solar system
                if (prevSun != null && nextSun != null)
                {
                    Quaternion desiredPlanetRotation = content.transform.rotation;
                    Quaternion matchContentRotation = Quaternion.identity;

                    if (nextSun != nextPlanet)
                    {
                        Vector3 prevToPlanet = content.transform.position - prevSun.transform.position;
                        Vector3 nextToPlanet = content.transform.position - nextSun.transform.position;
                        matchContentRotation = Quaternion.FromToRotation(nextToPlanet, prevToPlanet);
                    }
                    else
                    {
                        matchContentRotation = Quaternion.Inverse(content.transform.rotation);
                    }

                    content.transform.rotation = matchContentRotation * content.transform.rotation;
                    nextPlanet.transform.rotation = desiredPlanetRotation;

                    StartCoroutine(TransitionContent(
                        nextPlanet,
                        Vector3.zero,
                        originalPlanetRotation * Quaternion.Inverse(desiredRotation),
                        0.0f,
                        TransitionTimeOpeningScene,
                        null,
                        transitionCurve,
                        null));
                }

                prevSceneLoaded.transform.SetParent(content.transform, true);
            }

            // transition the new content to the view boundary
            StartCoroutine(TransitionContent(
                content,
                desiredPosition,
                desiredRotation,
                desiredScale,
                TransitionTimeOpeningScene,
                transitionCurve,
                transitionCurve,
                transitionCurve));

            // adjust the old view to size the source with the new content
            if (prevSceneLoaded != null)
            {
                // fade out the old content
                float fadeTime = GetClosingSceneVisibilityTime();
                StartCoroutine(FadeContent(
                    prevSceneLoaded,
                    FadeType.FadeUnload,
                    fadeTime,
                    OpacityCurveClosingScene));

                // hide the source since it is represented by the new content that is getting rendered
                if (sourceSizer != null && sourceSizer.TargetFillCollider != null)
                {
                    sourceSizer.TargetFillCollider.transform.parent.gameObject.SetActive(false);
                }
            }

            doNotDisableScene = prevSceneLoaded;
            prevSceneLoaded = content;
        }

        private void GetOrientationFromView(out Vector3 position, out Quaternion rotation, out float scale)
        {
            position = ViewLoader.Instance.transform.position;
            rotation = ViewLoader.Instance.transform.rotation;
            scale = Mathf.Max(ViewVolume.transform.lossyScale.x, ViewVolume.transform.lossyScale.y, ViewVolume.transform.lossyScale.z);
        }

        private AnimationCurve GetContentRotationCurve(string loadedSceneName)
        {
            if (prevSceneLoaded == null)
            {
                return IntroTransitionCurveContentChange;
            }

            if (loadedSceneName.Contains("GalaxyView"))
            {
                return SSToGalaxyTransitionCurveContentChange;
            }

            if (loadedSceneName.Contains("SolarSystemView"))
            {
                if (prevSceneLoaded.name.Contains("GalaxyView"))
                {
                    return GalaxyToSSTransitionCurveContentChange;
                }
                else
                {
                    return PlanetToSSRotationCurveContentChange;
                }
            }

            return SSToPlanetTransitionCurveContentChange;
        }

        private AnimationCurve GetContentTransitionCurve(string loadedSceneName)
        {
            if (prevSceneLoaded == null)
            {
                return IntroTransitionCurveContentChange;
            }

            if (loadedSceneName.Contains("GalaxyView"))
            {
                return SSToGalaxyTransitionCurveContentChange;
            }

            if (loadedSceneName.Contains("SolarSystemView"))
            {
                if (prevSceneLoaded.name.Contains("GalaxyView"))
                {
                    return GalaxyToSSTransitionCurveContentChange;
                }
                else
                {
                    return PlanetToSSPositionScaleCurveContentChange;
                }
            }

            return SSToPlanetTransitionCurveContentChange;
        }

        private float GetClosingSceneVisibilityTime()
        {
            if (prevSceneLoaded == null)
            {
                Debug.LogError("TransitionManager: Unable to find the time it takes to fade the last loaded scene because no previous loaded scene was found.");
                return 0.0f;
            }

            if (prevSceneLoaded.name.Contains("GalaxyView"))
            {
                return GalaxyVisibilityTimeClosingScene;
            }

            if (prevSceneLoaded.name.Contains("SolarSystemView"))
            {
                return SolarSystemVisibilityTimeClosingScene;
            }

            return PlanetVisibilityTimeClosingScene;
        }

        public event Action FadeComplete;
        public IEnumerator FadeContent(GameObject content, FadeType fadeType, float fadeDuration, AnimationCurve opacityCurve, float fadeTimeOffset = 0.0f, bool deactivateOnFadeout = true)
        {
            if (content == null)
            {
                yield break;
            }
            SpiralGalaxy[] galaxies = content.GetComponentsInChildren<SpiralGalaxy>(true); // galaxies are a special case
            Fader[] contentFaders = content.GetComponentsInChildren<Fader>(true);
            bool[] enabledFaders = new bool[contentFaders.Length];

            // prevent content from changing until the points of interest have completely faded out
            while (fadingPointsOfInterest)
            {
                yield return null;
            }

            // if a fader is already enabled, it is in the process of fading, so do not override those settings; otherwise, enable the fader
            for (int faderIndex = 0; faderIndex < contentFaders.Length; ++faderIndex)
            {
                Fader fader = contentFaders[faderIndex];
                bool canEnableFader = !fader.Enabled && galaxies.Length == 0;
                enabledFaders[faderIndex] = canEnableFader;

                if (canEnableFader)
                {
                    fader.EnableFade();
                }
            }

            // wait for the fade time offset to complete before alpha is changed on the faders
            float time = fadeTimeOffset;
            while (time > 0.0f)
            {
                time = Mathf.Clamp(time - Time.deltaTime, 0.0f, fadeTimeOffset);
                yield return null;
            }

            // activate content after the time offset has elapsed, so we can fade something in after it was faded out with an offset
            content.SetActive(true);

            // setup initial and final alpha values for the fade based on the type of fade
            Vector2 alpha = Vector2.one;
            switch (fadeType)
            {
                case FadeType.FadeIn:
                    alpha.x = 0.0f;
                    break;
                case FadeType.FadeOut:
                    alpha.y = 0.0f;
                    break;
                case FadeType.FadeUnload:
                    alpha.y = 0.0f;
                    break;
            }

            float timeFraction = 0.0f;
            do
            {
                time += Time.deltaTime;
                timeFraction = Mathf.Clamp01(time / fadeDuration);

                float alphaValue = opacityCurve != null
                    ? Mathf.Lerp(alpha.x, alpha.y, Mathf.Clamp01(opacityCurve.Evaluate(timeFraction)))
                    : timeFraction;

                for (int faderIndex = 0; faderIndex < contentFaders.Length; ++faderIndex)
                {
                    Fader fader = contentFaders[faderIndex];

                    if (enabledFaders[faderIndex] &&                              // the fader is not getting faded by this corouting (happening in a different one)
                        ((alpha.x < alpha.y && fader.GetAlpha() < alpha.y) ||     // fading in and prevent popping for faders that have more alpha than our fader
                         (alpha.x > alpha.y && fader.GetAlpha() > alpha.y)))      // fading out and prevent popping for faders that have less alpha than our fader
                    {
                        fader.SetAlpha(alphaValue);
                    }
                }

                foreach (var galaxy in galaxies)
                {
                    galaxy.TransitionAlpha = alphaValue;
                }

                if (alphaValue == 0.0f && (fadeType == FadeType.FadeOut || fadeType == FadeType.FadeUnload))
                {
                    break;
                }

                yield return null;
            }
            while (timeFraction < 1.0f && content != null);

            if (content == null)
            {
                yield break;
            }

            if (fadeType == FadeType.FadeOut || fadeType == FadeType.FadeUnload)
            {
                fadingPointsOfInterest = false;

                // hide the object since it is faded out; do not do this for a scene content object to sky box because disabling the active state of these objects kills other coroutines
                if (content != prevSceneLoaded && (holoCube == null || content != holoCube.gameObject))
                {
                    // the scene object must remain active because there are other coroutines running on it; instead, deactivate the children for perf
                    if (content == doNotDisableScene)
                    {
                        for (int childIndex = 0; childIndex < content.transform.childCount; ++childIndex)
                        {
                            content.transform.GetChild(childIndex).gameObject.SetActive(!deactivateOnFadeout);
                        }
                    }
                    else
                    {
                        // this content is safe to make inactive for perf
                        content.SetActive(!deactivateOnFadeout);
                    }
                }
            }

            if (fadeType == FadeType.FadeUnload)
            {
                ViewLoader.Instance.UnloadView(sceneToUnload);
                yield break;
            }

            // content has not been unloaded, so turn off fading
            for (int faderIndex = 0; faderIndex < contentFaders.Length; ++faderIndex)
            {
                if (enabledFaders[faderIndex])
                {
                    contentFaders[faderIndex].DisableFade();
                }
            }
            if (FadeComplete != null)
            {
                FadeComplete();
            }
        }

        private IEnumerator TransitionContent(GameObject content, Vector3 targetPosition, Quaternion targetRotation, float targetSize,
            float transitionDuration, AnimationCurve positionCurve, AnimationCurve rotationCurve, AnimationCurve scaleCurve, float transitionTimeOffset = 0.0f, bool animateInLocalSpace = false)
        {
            // Disable all colliders during a transition to workaround physics issues with scaling box and mesh colliders down to really small numbers (~0)
            Collider[] colliders = content.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }

            // Prevent content from changing until the points of interest have completely faded out
            while (fadingPointsOfInterest)
            {
                yield return null;
            }

            SceneSizer contentSizer = content.GetComponent<SceneSizer>();
            Vector3 startPosition = animateInLocalSpace ? content.transform.localPosition : content.transform.position;
            Quaternion startRotation = animateInLocalSpace ? content.transform.localRotation : content.transform.rotation;
            Vector3 startScale = content.transform.localScale;

            if (contentSizer != null)
            {
                targetSize = contentSizer.GetScalar(targetSize);
            }

            Vector3 finalScale = new Vector3(targetSize, targetSize, targetSize);

            float time = -transitionTimeOffset;
            float timeFraction = 0.0f;
            do
            {
                time += Time.deltaTime;
                timeFraction = Mathf.Clamp01(time / transitionDuration);

                if (positionCurve != null)
                {
                    if (animateInLocalSpace)
                    {
                        content.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, Mathf.Clamp01(positionCurve.Evaluate(timeFraction)));
                    }
                    else
                    {
                        content.transform.position = Vector3.Lerp(startPosition, targetPosition, Mathf.Clamp01(positionCurve.Evaluate(timeFraction)));
                    }
                }

                if (rotationCurve != null)
                {
                    if (animateInLocalSpace)
                    {
                        content.transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, Mathf.Clamp01(rotationCurve.Evaluate(timeFraction)));
                    }
                    else
                    {
                        content.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, Mathf.Clamp01(rotationCurve.Evaluate(timeFraction)));
                    }
                }

                if (scaleCurve != null)
                {
                    content.transform.localScale = Vector3.Lerp(startScale, finalScale, Mathf.Clamp01(scaleCurve.Evaluate(timeFraction)));
                }

                yield return null;
            }
            while (timeFraction < 1.0f && content != null);

            if (content == null)
            {
                yield break;
            }

            if (ContentLoaded != null && content == prevSceneLoaded)
            {
                ContentLoaded();
            }

            inTransition = false;

            // Reenable colliders at the end of the transition
            foreach (Collider collider in colliders)
            {
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }

            if (!IsIntro)
            {
                ToolManager.Instance.ShowTools();
            }
        }

        private IEnumerator TransitionOrbitSpeed(GameObject content, SpeedType speedType, float duration, AnimationCurve speedCurve, float transitionTimeOffset = 0.0f)
        {
            OrbitUpdater[] orbits = content.GetComponentsInChildren<OrbitUpdater>();

            if (orbits.Length > 0)
            {
                // If the orbit is speeding up, start with zeroed speed
                if (speedType == SpeedType.SpeedUp)
                {
                    foreach (OrbitUpdater orbit in orbits)
                    {
                        orbit.TransitionSpeedMultiplier = 0.0f;
                    }
                }

                // Prevent content from changing until the points of interest have completely faded out
                while (fadingPointsOfInterest)
                {
                    yield return null;
                }

                Vector2 scalarRange = Vector2.one;

                switch (speedType)
                {
                    case SpeedType.SpeedUp:
                        scalarRange.x = 0.0f;
                        break;
                    case SpeedType.SlowDown:
                        scalarRange.y = 0.0f;
                        break;
                }

                float time = -transitionTimeOffset;
                float timeFraction = 0.0f;
                do
                {
                    time += Time.deltaTime;
                    timeFraction = Mathf.Clamp01(time / duration);

                    foreach (OrbitUpdater orbit in orbits)
                    {
                        orbit.TransitionSpeedMultiplier = speedCurve != null
                            ? Mathf.Lerp(scalarRange.x, scalarRange.y, Mathf.Clamp01(speedCurve.Evaluate(timeFraction)))
                            : timeFraction;
                    }

                    yield return null;
                }
                while (timeFraction < 1.0f && orbits != null);
            }
        }

        private void SwitchAudioClips(string sceneName, bool forwardNavigation = true)
        {
            AudioClip staticClip = null;
            AudioClip movingClip = null;

            if (!forwardNavigation)
            {
                staticClip = BackClips.StaticClip;
                movingClip = BackClips.MovingClip;
            }
            else if (sceneName == "SolarSystemView")
            {
                staticClip = SolarSystemClips.StaticClip;
                movingClip = SolarSystemClips.MovingClip;
            }
            else if (!IsIntro)
            {
                staticClip = PlanetClips.StaticClip;
                movingClip = PlanetClips.MovingClip;
            }

            ViewLoader.Instance.SetTransitionSFX(staticClip, movingClip);
        }
    }
}