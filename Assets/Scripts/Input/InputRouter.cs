// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace GalaxyExplorer
{
    public class InputRouter : SingleInstance<InputRouter>
    {
        // These fields are used for fake input manipulation in the editor.
        // They aren't defined in an #if UNITY_EDITOR block so serialization
        // doesn't get messed up.
        public Vector3 fakeInput;
        public bool enableFakeInput = false;
        public bool FakeTapUpdate;

        public bool HandsVisible { get; private set; }

        [HideInInspector()]
        public Vector2 XamlMousePosition = new Vector2(0, 0);

        /// <summary>
        /// Inputs that were started and that are currently active
        /// </summary>
        public HashSet<InteractionSourceKind> PressedSources { get; private set; }

        public event Action<InteractionSourceKind, Vector3, Ray> InputStarted;
        public event Action<InteractionSourceKind, Vector3, Ray> InputUpdated;
        public event Action<InteractionSourceKind, Vector3, Ray> InputCompleted;
        public event Action<InteractionSourceKind> InputCanceled;

        public event Action InputTapped;

        /// <summary>
        /// May be called several times if the event is handled by several objects
        /// </summary>
        public event Action Tapped;

        private GestureRecognizer gestureRecognizer;
        private bool eventsAreRegistered = false;

        private bool ctrlKeyIsDown = false;
        private bool lCtrlKeyIsDown = false;
        private bool rCtrlKeyIsDown = false;

        private void TryToRegisterEvents()
        {
            if (!eventsAreRegistered)
            {
                if (gestureRecognizer != null)
                {
                    gestureRecognizer.NavigationStarted += OnNavigationStarted;
                    gestureRecognizer.NavigationUpdated += OnNavigationUpdated;
                    gestureRecognizer.NavigationCompleted += OnNavigationCompleted;
                    gestureRecognizer.NavigationCanceled += OnNavigationCanceled;
                    gestureRecognizer.Tapped += OnTapped;
                }

                if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.HoloLens ||
                    MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD)
                {
                    InteractionManager.InteractionSourceDetected += SourceManager_OnInteractionSourceDetected;
                    InteractionManager.InteractionSourceLost += SourceManager_OnInteractionSourceLost;
                    InteractionManager.InteractionSourcePressed += SourceManager_OnInteractionSourcePressed;
                    InteractionManager.InteractionSourceReleased += SourceManager_OnInteractionSourceReleased;
                }

                KeyboardInput kbd = KeyboardInput.Instance;
                if (kbd != null)
                {
                    KeyboardInput.KeyEvent keyEvent = KeyboardInput.KeyEvent.KeyDown;
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Equals, keyEvent), HandleKeyboardZoomIn);
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Plus, keyEvent), HandleKeyboardZoomIn);
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.KeypadPlus, keyEvent), HandleKeyboardZoomIn);
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Minus, keyEvent), HandleKeyboardZoomOut);
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.KeypadMinus, keyEvent), HandleKeyboardZoomOut);

                    keyEvent = KeyboardInput.KeyEvent.KeyReleased;
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Space, keyEvent), FakeTapKeyboardHandler);
                    kbd.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Backspace, keyEvent), HandleBackButtonFromKeyboard);
                }

                eventsAreRegistered = true;
            }
        }

        private void TryToUnregisterEvents()
        {
            if (eventsAreRegistered)
            {
                if (gestureRecognizer != null)
                {
                    gestureRecognizer.NavigationStarted -= OnNavigationStarted;
                    gestureRecognizer.NavigationUpdated -= OnNavigationUpdated;
                    gestureRecognizer.NavigationCompleted -= OnNavigationCompleted;
                    gestureRecognizer.NavigationCanceled -= OnNavigationCanceled;
                    gestureRecognizer.Tapped -= OnTapped;
                }

                if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.HoloLens ||
                    MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD)
                {
                    InteractionManager.InteractionSourceDetected -= SourceManager_OnInteractionSourceDetected;
                    InteractionManager.InteractionSourceLost -= SourceManager_OnInteractionSourceLost;
                    InteractionManager.InteractionSourcePressed -= SourceManager_OnInteractionSourcePressed;
                    InteractionManager.InteractionSourceReleased -= SourceManager_OnInteractionSourceReleased;
                }

                KeyboardInput kbd = KeyboardInput.Instance;
                if (kbd != null)
                {
                    KeyboardInput.KeyEvent keyEvent = KeyboardInput.KeyEvent.KeyDown;
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Equals, keyEvent), HandleKeyboardZoomIn);
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Plus, keyEvent), HandleKeyboardZoomIn);
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.KeypadPlus, keyEvent), HandleKeyboardZoomIn);
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Minus, keyEvent), HandleKeyboardZoomOut);
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.KeypadMinus, keyEvent), HandleKeyboardZoomOut);

                    keyEvent = KeyboardInput.KeyEvent.KeyReleased;
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Space, keyEvent), FakeTapKeyboardHandler);
                    kbd.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Backspace, keyEvent), HandleBackButtonFromKeyboard);
                }
                eventsAreRegistered = false;
            }
        }

        private void Awake()
        {
            PressedSources = new HashSet<InteractionSourceKind>();
        }

        private void Start()
        {
            if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.HoloLens ||
                MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD)
            {
                gestureRecognizer = new GestureRecognizer();
                gestureRecognizer.SetRecognizableGestures(GestureSettings.Hold | GestureSettings.Tap |
                                                          GestureSettings.NavigationY | GestureSettings.NavigationX);

                gestureRecognizer.StartCapturingGestures();
            }

            TryToRegisterEvents();
        }

        public void EnableHoldAndNavigationGestures(bool enabled)
        {
            if (gestureRecognizer == null) return;

            if (enabled)
            {
                gestureRecognizer.SetRecognizableGestures(GestureSettings.Hold | GestureSettings.Tap |
                                                          GestureSettings.NavigationY | GestureSettings.NavigationX);
            }
            else
            {
                gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap);
            }
        }

        private void FakeTapKeyboardHandler(KeyboardInput.KeyCodeEventPair keyCodeEvent)
        {
            SendFakeTap();
        }

        private void HandleBackButtonFromKeyboard(KeyboardInput.KeyCodeEventPair keyCodeEvent)
        {
            var backButton = ToolManager.Instance.FindButtonByType(ButtonType.Back);
            if (backButton != null)
            {
                backButton.ButtonAction();
            }
        }

        public void SendFakeTap()
        {
            OnTapped(new TappedEventArgs());
        }

        private void HandleKeyboardZoomOut(KeyboardInput.KeyCodeEventPair keyCodeEvent)
        {
            HandleKeyboardZoom(-1);
        }
        private void HandleKeyboardZoomIn(KeyboardInput.KeyCodeEventPair keyCodeEvent)
        {
            HandleKeyboardZoom(1);
        }
        private void HandleKeyboardZoom(int direction)
        {
            if (ctrlKeyIsDown)
            {
                Instance.HandleZoomFromXaml(1 + (direction * 0.03f));
            }
        }

        private bool ReadyForXamlInput
        {
            get
            {
                // Ignore input fromXaml until the introduction flow has
                // gotten us to GalaxyView.
                return !IntroductionFlow.Instance.enabled || (
                    IntroductionFlow.Instance.enabled &&
                    IntroductionFlow.Instance.currentState == IntroductionFlow.IntroductionState.IntroductionStateComplete);
            }
        }

        public void HandleZoomFromXaml(float delta)
        {
            if (ReadyForXamlInput)
            {
                ToolManager.Instance.UpdateZoomFromXaml(delta);
            }
        }

        public void HandleRotationFromXaml(float delta)
        {
            if (ReadyForXamlInput)
            {
                ToolManager.Instance.UpdateRotationFromXaml(Math.Sign(delta));
            }
        }

        public void HandleTranslateFromXaml(Vector2 delta)
        {
            if (ReadyForXamlInput)
            {
                if (ctrlKeyIsDown)
                {
                    // if a control key is down, perform a rotation instead of translation
                    HandleRotationFromXaml(delta.y);
                }
                else
                {
                    delta *= 0.001f;
                    Camera.main.transform.parent.position += new Vector3(delta.x, delta.y, 0);
                }
            }
        }

        public void HandleResetFromXaml()
        {
            Button resetButton = ToolManager.Instance.FindButtonByType(ButtonType.Reset);
            if (resetButton &&
                TransitionManager.Instance &&
                !TransitionManager.Instance.InTransition)
            {
                // tell the camera to go back to (0,0,0)
                StartCoroutine(ResetCameraToOrigin());

                // reset everything else
                resetButton.ButtonAction();
            }
        }

        public void HandleAboutFromXaml()
        {
            Tool aboutTool = ToolManager.Instance.FindToolByType(ToolType.About);
            if (aboutTool &&
                TransitionManager.Instance &&
                !TransitionManager.Instance.InTransition)
            {
                aboutTool.Select();
            }
        }

        private IEnumerator ResetCameraToOrigin()
        {
            // TODO: Consider moving this code into TransitionManager
            Vector3 startPosition = Camera.main.transform.parent.position;

            float time = 0.0f;
            float timeFraction = 0.0f;
            do
            {
                time += Time.deltaTime;
                timeFraction = Mathf.Clamp01(time / TransitionManager.Instance.TransitionTimeCube);

                Vector3 newPosition = Vector3.Lerp(
                    startPosition,
                    Vector3.zero,
                    Mathf.Clamp01(TransitionManager.Instance.TransitionCurveCube.Evaluate(timeFraction)));

                Camera.main.transform.parent.position = newPosition;
                yield return null;

            } while (timeFraction < 1f);

            Camera.main.transform.parent.position = Vector3.zero;

            while (TransitionManager.Instance.InTransition)
            {
                // Wait for the TransitionManager to finish...
                yield return null;
            }
            // Resetting the view changes the content's lookRotation which might
            // be confused if the camera was moving at the same time.
            // Since the camera is now done moving, re-reset the content to
            // get the final lookRotation just right.
            ToolManager.Instance.FindButtonByType(ButtonType.Reset).ButtonAction();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (enableFakeInput)
            {
                if (fakeInput == Vector3.zero)
                {
                    OnNavigationCompletedWorker(
                        InteractionSourceKind.Controller,
                        fakeInput,
                        new Ray());
                }
                else
                {
                    OnNavigationUpdatedWorker(
                        InteractionSourceKind.Controller,
                        fakeInput,
                        new Ray());
                }

                if (FakeTapUpdate)
                {
                    InternalHandleOnTapped();
                    FakeTapUpdate = false;
                }
            }
#endif
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                lCtrlKeyIsDown = true;
            }
            if (Input.GetKeyDown(KeyCode.RightControl))
            {
                rCtrlKeyIsDown = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                lCtrlKeyIsDown = false;
            }
            if (Input.GetKeyUp(KeyCode.RightControl))
            {
                rCtrlKeyIsDown = false;
            }
            ctrlKeyIsDown = lCtrlKeyIsDown || rCtrlKeyIsDown;
        }

        protected override void OnDestroy()
        {
            if (gestureRecognizer != null)
            {
                gestureRecognizer.StopCapturingGestures();
                gestureRecognizer.Dispose();
            }
            if (eventsAreRegistered)
            {
                TryToUnregisterEvents();
            }
            base.OnDestroy();
        }

#region EventCallbacks

        private void SourceManager_OnInteractionSourceLost(InteractionSourceLostEventArgs args)
        {
            if (args.state.source.kind == InteractionSourceKind.Hand)
            {
                HandsVisible = false;
            }
        }

        private void SourceManager_OnInteractionSourceDetected(InteractionSourceDetectedEventArgs args)
        {
            if (args.state.source.kind == InteractionSourceKind.Hand)
            {
                HandsVisible = true;
            }
        }

        private void SourceManager_OnInteractionSourcePressed(InteractionSourcePressedEventArgs args)
        {
            PressedSources.Add(args.state.source.kind);
        }

        private void SourceManager_OnInteractionSourceReleased(InteractionSourceReleasedEventArgs args)
        {
            PressedSources.Remove(args.state.source.kind);
        }

        private void OnNavigationStarted(NavigationStartedEventArgs args)
        {
        }
        public void OnNavigationStartedWorker(InteractionSourceKind kind, Vector3 normalizedOffset, Ray headRay)
        { 
            bool handled = false;
            if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
            {
                handled = GazeSelectionManager.Instance.SelectedTarget.OnNavigationStarted(kind, normalizedOffset, headRay);
            }

            if (!handled && InputStarted != null)
            {
                InputStarted(kind, normalizedOffset, headRay);
            }
        }

        private bool TryGetInteractionSourcePoseRay(InteractionSourcePose pose, out Ray ray)
        {
            Vector3 forward;
            Vector3 position;
            if (pose.TryGetForward(out forward) &&
                pose.TryGetPosition(out position))
            {
                ray = new Ray(position, forward);
                return true;
            }
            ray = new Ray();
            return false;
        }

        private void OnNavigationUpdated(NavigationUpdatedEventArgs args)
        {
            Ray ray = new Ray();
            OnNavigationUpdatedWorker(args.source.kind, args.normalizedOffset, ray);
        }

        public void OnNavigationUpdatedWorker(InteractionSourceKind kind, Vector3 normalizedOffset, Ray headRay)
        {
            bool handled = false;
            if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
            {
                handled = GazeSelectionManager.Instance.SelectedTarget.OnNavigationUpdated(kind, normalizedOffset, headRay);
            }

            if (!handled && InputUpdated != null)
            {
                InputUpdated(kind, normalizedOffset, headRay);
            }
        }

        private void OnNavigationCompleted(NavigationCompletedEventArgs args)
        {
            Ray ray;
            TryGetInteractionSourcePoseRay(args.sourcePose, out ray);
            OnNavigationCompletedWorker(args.source.kind, args.normalizedOffset, ray);
        }

        public void OnNavigationCompletedWorker(InteractionSourceKind kind, Vector3 normalizedOffset, Ray headRay)
        {
            bool handled = false;
            if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
            {
                handled = GazeSelectionManager.Instance.SelectedTarget.OnNavigationCompleted(kind, normalizedOffset, headRay);
            }

            if (!handled && InputCompleted != null)
            {
                InputCompleted(kind, normalizedOffset, headRay);
            }
        }

        public void OnNavigationCanceled(NavigationCanceledEventArgs args)
        {
            bool handled = false;
            if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
            {
                handled = GazeSelectionManager.Instance.SelectedTarget.OnNavigationCanceled(args.source.kind);
            }

            if (!handled && InputCanceled != null)
            {
                InputCanceled(args.source.kind);
            }
        }

        private void OnTapped(TappedEventArgs args)
        {
            InternalHandleOnTapped();
        }

        public void InternalHandleOnTapped()
        {
            if (TransitionManager.Instance != null && !TransitionManager.Instance.InTransition)
            {
                bool handled = false;
                if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
                {
                    handled = GazeSelectionManager.Instance.SelectedTarget.OnTapped();
                }
                else
                {
                    PlacementControl placementControl = TransitionManager.Instance.ViewVolume.GetComponentInChildren<PlacementControl>();

                    if (placementControl != null && placementControl.IsHolding)
                    {
                        handled = placementControl.OnTapped();
                        if (ToolSounds.isInitialized &&         // Starts out on an inactive object so Instance will be null
                            ToolSounds.Instance &&              // Make sure we have an instance
                            ToolManager.Instance &&             // Make sure we have an instance
                            ToolManager.Instance.ToolsVisible)  // Can we start a Co-routine on this object?
                        {
                            ToolSounds.Instance.PlaySelectSound();
                        }
                    }
                }

                if (!handled && InputTapped != null)
                {
                    InputTapped();
                }

                if (Tapped != null)
                {
                    Tapped();
                }
            }
        }

#endregion
    }
}