// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using UnityEngine;

namespace GalaxyExplorer
{
    public class IntroductionFlow : SingleInstance<IntroductionFlow>
    {
        public GameObject Logo;
        public float IntroSlateTime = 1.0f;
        public float IntroLogoTime = 4.5f;
        public float MinTimeInForDialog = 2.0f;
        public float DialogExitTime = 2.0f;
        [Tooltip("Causes the flow to skip placing the earth. This is set to true in Awake if the experience is not holographic.")]
        public bool SkipPlaceEarth = false;
        public float SecondsToFadeOutEarth;
        public float SecondsOnEarth;
        public float SecondsOnSolarSystem;
        public float SecondsOnGalaxy;
        public float SecondsPerLevel;
        public string IntroSound;
        public InstructionSlate InstructionSlate;

        public AudioClip Title;
        public AudioClip Description;
        public AudioClip Goal;
        public AudioClip Invitation;
        public AudioClip CenterEarth;
        public AudioClip EarthCentered;
        public AudioClip Earth;
        public AudioClip SolarSystem;
        public AudioClip LogoHydrate;
        public AudioClip EarthPlacement;
        public AudioClip EarthHydrate;

        private PlacementControl placementControl;
        private IntroEarth introEarth;
        private Animator logoAnimator;
        private AudioSource audioSource;

        public enum IntroductionState
        {
            IntroductionStateAppDescription,
            IntroductionStateDevelopers,
            IntroductionStateCommunity,
            IntroductionStateSlateFadeout,
            IntroductionStateLogo,
            IntroductionStateLogoFadeout,
            IntroductionStatePreloadSolarSystem,
            IntroductionStateEarthHydrate,
            IntroductionStatePlaceEarth,
            IntroductionStateEarthFadeout,
            IntroductionEarth,
            IntroductionSolarSystem,
            IntroductionGalaxy,
            IntroductionStateComplete
        }

        public IntroductionState currentState = IntroductionState.IntroductionStateSlateFadeout;

        private float timeInState = 0.0f;
        private bool coreSystemsLoaded = false;

        private void Awake()
        {
#if !UNITY_EDITOR
        // Skip placing the earth if we aren't in a VR device.
        // We do this check in a !UNITY_EDITOR block to allow for testing
        // from inside the editor without having to change code.
            SkipPlaceEarth = !UnityEngine.XR.XRDevice.isPresent;
#endif
            DontDestroyOnLoad(this);
        }
        private void Start()
        {
            if (Logo == null)
            {
                Debug.LogError("IntroductionFlow: Logo is not defined - unable to start because there will be no cube to place or logo during the introduction");
                Destroy(this);
                return;
            }

            logoAnimator = Logo.GetComponent<Animator>();

            if (InstructionSlate == null)
            {
                Debug.LogError("IntroductionFlow: No InstructionSlate defined to outline instructions for the viewer - unable to proceed");
                Destroy(this);
                return;
            }

            ViewLoader.Instance.CoreSystemsLoaded += CoreSystemsLoaded;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("IntroductionFlow: Missing AudioSource, will be unable to play SFX for introduction beats.");
            }
        }

        protected override void OnDestroy()
        {
            if (coreSystemsLoaded)
            {
                if (InputRouter.Instance != null)
                {
                    InputRouter.Instance.InputTapped -= OnTapped;
                }
            }
            else if (ViewLoader.Instance != null)
            {
                ViewLoader.Instance.CoreSystemsLoaded -= CoreSystemsLoaded;
            }
            base.OnDestroy();
        }

        private void Update()
        {
            switch (currentState)
            {
                case IntroductionState.IntroductionStateAppDescription:
                case IntroductionState.IntroductionStateDevelopers:
                case IntroductionState.IntroductionStateCommunity:
                    if (timeInState >= IntroSlateTime)
                    {
                        AdvanceIntroduction();
                    }

                    break;

                case IntroductionState.IntroductionStateLogo:
                    if (timeInState >= IntroLogoTime)
                    {
                        AdvanceIntroduction();
                    }

                    break;

                case IntroductionState.IntroductionStateSlateFadeout:
                case IntroductionState.IntroductionStateLogoFadeout:
                    if (timeInState >= DialogExitTime)
                    {
                        AdvanceIntroduction();
                    }

                    break;

                case IntroductionState.IntroductionStatePreloadSolarSystem:
                    if (TransitionManager.Instance.InTransition == false)
                    {
                        if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD &&
                            StarBackgroundManager.Instance)
                        {
                            StarBackgroundManager.Instance.FadeInOut(false);
                        }
                        AdvanceIntroduction();
                    }

                    break;

                case IntroductionState.IntroductionStateEarthHydrate:
                    if (SkipPlaceEarth || (timeInState >= DialogExitTime))
                    {
                        AdvanceIntroduction();
                    }

                    break;

                case IntroductionState.IntroductionStatePlaceEarth:
                    if (introEarth == null)
                    {
                        GameObject currentContent = ViewLoader.Instance.GetCurrentContent();

                        if (currentContent)
                        {
                            introEarth = currentContent.GetComponent<IntroEarth>();

                            if (introEarth)
                            {
                                introEarth.SetIntroMode(true);
                                PlayOneShot(EarthPlacement);
                            }
                        }
                    }
                    else
                    {
                        if (SkipPlaceEarth && TransitionManager.Instance.InTransition == false)
                        {
                            placementControl.TogglePinnedState();
                        }
                    }

                    break;

                case IntroductionState.IntroductionStateEarthFadeout:
                    if (timeInState > SecondsToFadeOutEarth)
                    {
                        if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD &&
                            StarBackgroundManager.Instance)
                        {
                            StarBackgroundManager.Instance.FadeInOut(true);
                        }
                        if (!SkipPlaceEarth)
                        {
                            // If we placed the Earth, play the VO "Great!" to
                            // give feedback to the user that they did something
                            // important.
                            VOManager.Instance.PlayClip(EarthCentered);
                        }
                        VOManager.Instance.PlayClip(Earth);
                        AdvanceIntroduction();
                    }

                    break;

                case IntroductionState.IntroductionEarth:
                    if (timeInState > SecondsOnEarth)
                    {
                        VOManager.Instance.PlayClip(SolarSystem);
                        TransitionManager.Instance.LoadPrevScene("SolarSystemView");
                        AdvanceIntroduction();
                    }

                    break;

                case IntroductionState.IntroductionSolarSystem:
                    if (timeInState > SecondsOnSolarSystem)
                    {
                        TransitionManager.Instance.IsIntro = false;
                        TransitionManager.Instance.LoadPrevScene("GalaxyView");
                        AdvanceIntroduction();
                    }

                    break;

                case IntroductionState.IntroductionGalaxy:
                    if (timeInState > SecondsOnGalaxy)
                    {
                        AdvanceIntroduction();
                    }

                    break;
            }

            timeInState += Time.deltaTime;
        }

        private void CoreSystemsLoaded()
        {
            ViewLoader.Instance.CoreSystemsLoaded -= CoreSystemsLoaded;
            InputRouter.Instance.InputTapped += OnTapped;
            coreSystemsLoaded = true;

            placementControl = TransitionManager.Instance.ViewVolume.GetComponentInChildren<PlacementControl>();

            MusicManager.Instance.FindSnapshotAndTransition(MusicManager.Instance.Welcome);
            VOManager.Instance.Stop(clearQueue: true);
            //VOManager.Instance.PlayClip(Title);
            //VOManager.Instance.PlayClip(Description);
            //VOManager.Instance.PlayClip(Goal);
            //VOManager.Instance.PlayClip(Invitation);

            UpdateInstructions();
        }

        private void OnTapped()
        {
            switch (currentState)
            {
                case IntroductionState.IntroductionStatePlaceEarth:
                    if (!SkipPlaceEarth)
                    {
                        AdvanceIntroduction();
                    }
                    break;

                case IntroductionState.IntroductionStateLogo:
                    AdvanceIntroduction();
                    break;

                case IntroductionState.IntroductionStateAppDescription:
                case IntroductionState.IntroductionStateDevelopers:
                case IntroductionState.IntroductionStateCommunity:
                    // skip intro
                    currentState = IntroductionState.IntroductionStateCommunity;
                    VOManager.Instance.Stop(clearQueue: true);
                    AdvanceIntroduction();
                    break;
            }
        }

        private void AdvanceIntroduction()
        {
            if (coreSystemsLoaded)
            {
                // change settings
                switch (currentState)
                {
                    case IntroductionState.IntroductionStateSlateFadeout:
                        InstructionSlate.gameObject.SetActive(false);

                        Logo.gameObject.SetActive(true);
                        Tagalong tagalong = Logo.gameObject.GetComponent<Tagalong>();
                        float distance = 2.0f;

                        if (tagalong != null)
                        {
                            distance = tagalong.TagalongDistance;
                        }

                        // position the logo and orient it towards the user
                        Logo.gameObject.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * distance);

                        Vector3 forwardDirection = Logo.gameObject.transform.position - Camera.main.transform.position;
                        Logo.gameObject.transform.rotation = Quaternion.LookRotation(forwardDirection.normalized);

                        logoAnimator.SetTrigger("FadeIn");
                        PlayOneShot(LogoHydrate);
                        break;

                    case IntroductionState.IntroductionStateLogo:
                        logoAnimator.SetTrigger("FadeOut");
                        break;

                    case IntroductionState.IntroductionStateLogoFadeout:
                        Logo.gameObject.SetActive(false);
                        ViewLoader.Instance.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 2.0f);
                        ViewLoader.Instance.transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - ViewLoader.Instance.transform.position);
                        StartCoroutine(ViewLoader.Instance.LoadStartingView());
                        TransitionManager.Instance.ContentLoaded += ContentLoaded;
                        placementControl.ContentPlaced += ContentPlaced;
                        break;

                    case IntroductionState.IntroductionStatePreloadSolarSystem:
                        if (!SkipPlaceEarth)
                        {
                            VOManager.Instance.PlayClip(CenterEarth);
                        }
                        break;
                }

                currentState = (IntroductionState)(currentState + 1);
                if (currentState == IntroductionState.IntroductionStateComplete)
                {
                    TransitionManager.Instance.ShowToolsAndCursor();
                    enabled = false;
                    return;
                }

                UpdateInstructions();
                timeInState = 0.0f;
            }
        }

        private void ContentLoaded()
        {
            TransitionManager.Instance.ContentLoaded -= ContentLoaded;
            if (ViewLoader.Instance.StartingView.Equals("EarthView"))
            {
                introEarth.TurnOnIntroEarth();
            }
            placementControl.TogglePinnedState();
        }

        private void ContentPlaced()
        {
            PlayOneShot(EarthHydrate);
            placementControl.ContentPlaced -= ContentPlaced;

            if (introEarth)
            {
                introEarth.TransitionFromIntroToReal();
            }

            AdvanceIntroduction();
        }

        private void UpdateInstructions()
        {
            switch (currentState)
            {
                case IntroductionState.IntroductionStateAppDescription:
                    InstructionSlate.gameObject.SetActive(true);

                    // position the slate in front of the user
                    Tagalong tagalong = InstructionSlate.gameObject.GetComponent<Tagalong>();
                    float distance = 2.0f;

                    if (tagalong != null)
                    {
                        distance = tagalong.TagalongDistance;
                    }

                    InstructionSlate.gameObject.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * distance);
                    InstructionSlate.DisplayMessage(InstructionSlate.InstructionText.AppDescription);
                    break;

                case IntroductionState.IntroductionStateDevelopers:
                    InstructionSlate.DisplayMessage(InstructionSlate.InstructionText.Developers);
                    break;

                case IntroductionState.IntroductionStateCommunity:
                    InstructionSlate.DisplayMessage(InstructionSlate.InstructionText.Community);
                    break;

                case IntroductionState.IntroductionStateSlateFadeout:
                    //InstructionSlate.Hide();
                    break;
            }
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (audioSource)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}