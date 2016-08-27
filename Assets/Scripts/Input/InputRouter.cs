// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using HoloToolkit.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

public interface IInputHandler
{
    bool OnNavigationStarted(InteractionSourceKind source, Vector3 relativePosition, Ray ray);

    bool OnNavigationUpdated(InteractionSourceKind source, Vector3 relativePosition, Ray ray);

    bool OnNavigationCompleted(InteractionSourceKind source, Vector3 relativePosition, Ray ray);

    bool OnNavigationCanceled(InteractionSourceKind source, Vector3 relativePosition, Ray ray);

    bool OnTapped(InteractionSourceKind source, int tapCount, Ray ray);
}

public class InputRouter : Singleton<InputRouter>
{
    public Vector3 fakeInput;
    public bool enableFakeInput = false;
    public bool FakeTapUpdate;

    public bool HandsVisible { get; private set; }

    /// <summary>
    /// Inputs that were started and that are currently active
    /// </summary>
    public HashSet<InteractionSourceKind> PressedSources { get; private set; }

    public Button BackButton;

    public event Action<InteractionSourceKind, Vector3, Ray> InputStarted;

    public event Action<InteractionSourceKind, Vector3, Ray> InputUpdated;

    public event Action<InteractionSourceKind, Vector3, Ray> InputCompleted;

    public event Action<InteractionSourceKind, Vector3, Ray> InputCanceled;

    public event Action<InteractionSourceKind, int, Ray> InputTapped;

    /// <summary>
    /// May be called several times if the event is handled by several objects
    /// </summary>
    public event Action<InteractionSourceKind, int, Ray> Tapped;

    private GestureRecognizer gestureRecognizer;
    private bool eventsAreRegistered = false;

    private void TryToRegisterEvents()
    {
        if (!eventsAreRegistered && gestureRecognizer != null)
        {
            gestureRecognizer.NavigationStartedEvent += OnNavigationStarted;
            gestureRecognizer.NavigationUpdatedEvent += OnNavigationUpdated;
            gestureRecognizer.NavigationCompletedEvent += OnNavigationCompleted;
            gestureRecognizer.NavigationCanceledEvent += OnNavigationCanceled;
            gestureRecognizer.TappedEvent += OnTapped;

            InteractionManager.SourceDetected += SourceManager_SourceDetected;
            InteractionManager.SourceLost += SourceManager_SourceLost;

            InteractionManager.SourcePressed += SourceManager_SourcePressed;
            InteractionManager.SourceReleased += SourceManager_SourceReleased;

            eventsAreRegistered = true;
        }
    }

    private void TryToUnregisterEvents()
    {
        if (eventsAreRegistered && gestureRecognizer != null)
        {
            gestureRecognizer.NavigationStartedEvent -= OnNavigationStarted;
            gestureRecognizer.NavigationUpdatedEvent -= OnNavigationUpdated;
            gestureRecognizer.NavigationCompletedEvent -= OnNavigationCompleted;
            gestureRecognizer.NavigationCanceledEvent -= OnNavigationCanceled;
            gestureRecognizer.TappedEvent -= OnTapped;

            InteractionManager.SourceDetected -= SourceManager_SourceDetected;
            InteractionManager.SourceLost -= SourceManager_SourceLost;

            InteractionManager.SourcePressed -= SourceManager_SourcePressed;
            InteractionManager.SourceReleased -= SourceManager_SourceReleased;

            eventsAreRegistered = false;
        }
    }

    private void Awake()
    {
        PressedSources = new HashSet<InteractionSourceKind>();
    }

    private void Start()
    {
        if (KeyboardInput.Instance)
        {
            KeyboardInput.Instance.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Space, KeyboardInput.KeyEvent.KeyReleased), FakeTapKeyboardHandler);
            KeyboardInput.Instance.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Backspace, KeyboardInput.KeyEvent.KeyReleased), FakeBackKeyboardHandler);
        }

        gestureRecognizer = new GestureRecognizer();
        gestureRecognizer.SetRecognizableGestures(GestureSettings.Hold | GestureSettings.Tap |
                                                  GestureSettings.NavigationY | GestureSettings.NavigationX);

        gestureRecognizer.StartCapturingGestures();

        TryToRegisterEvents();
    }

    private void FakeTapKeyboardHandler(KeyboardInput.KeyCodeEventPair keyCodeEvent)
    {
        SendFakeTap();
    }

    private void FakeBackKeyboardHandler(KeyboardInput.KeyCodeEventPair keyCodeEvent)
    {
        if (BackButton != null)
        {
            BackButton.ButtonAction();
        }
    }

    private void SendFakeTap()
    {
        OnTapped(InteractionSourceKind.Other, 0, new Ray(Vector3.zero, Vector3.forward));
    }

    private void Update()
    {
        if (enableFakeInput)
        {
            if (fakeInput == Vector3.zero)
            {
                OnNavigationCompleted(InteractionSourceKind.Controller, fakeInput, new Ray(Vector3.zero, Vector3.zero));
            }
            else
            {
                OnNavigationUpdated(InteractionSourceKind.Controller, fakeInput, new Ray(Vector3.zero, Vector3.zero));
            }

            if (FakeTapUpdate)
            {
                OnTapped(InteractionSourceKind.Other, 0, new Ray(Vector3.zero, Vector3.forward));
                FakeTapUpdate = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (gestureRecognizer != null)
        {
            gestureRecognizer.StopCapturingGestures();
            TryToUnregisterEvents();
            gestureRecognizer.Dispose();
        }

        if (KeyboardInput.Instance)
        {
            KeyboardInput.Instance.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Space, KeyboardInput.KeyEvent.KeyReleased), FakeTapKeyboardHandler);
            KeyboardInput.Instance.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.Backspace, KeyboardInput.KeyEvent.KeyReleased), FakeBackKeyboardHandler);
        }
    }

    #region EventCallbacks

    private void SourceManager_SourceLost(InteractionSourceState state)
    {
        if (state.source.kind == InteractionSourceKind.Hand)
        {
            HandsVisible = false;
        }
    }

    private void SourceManager_SourceDetected(InteractionSourceState state)
    {
        if (state.source.kind == InteractionSourceKind.Hand)
        {
            HandsVisible = true;
        }
    }

    private void SourceManager_SourcePressed(InteractionSourceState state)
    {
        PressedSources.Add(state.source.kind);
    }

    private void SourceManager_SourceReleased(InteractionSourceState state)
    {
        PressedSources.Remove(state.source.kind);
    }

    public void OnNavigationStarted(InteractionSourceKind source, Vector3 relativePosition, Ray ray)
    {
        bool handled = false;
        if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
        {
            handled = GazeSelectionManager.Instance.SelectedTarget.OnNavigationStarted(source, relativePosition, ray);
        }

        if (!handled && InputStarted != null)
        {
            InputStarted(source, relativePosition, ray);
        }
    }

    public void OnNavigationUpdated(InteractionSourceKind source, Vector3 relativePosition, Ray ray)
    {
        bool handled = false;
        if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
        {
            handled = GazeSelectionManager.Instance.SelectedTarget.OnNavigationUpdated(source, relativePosition, ray);
        }

        if (!handled && InputUpdated != null)
        {
            InputUpdated(source, relativePosition, ray);
        }
    }

    public void OnNavigationCompleted(InteractionSourceKind source, Vector3 relativePosition, Ray ray)
    {
        bool handled = false;
        if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
        {
            handled = GazeSelectionManager.Instance.SelectedTarget.OnNavigationCompleted(source, relativePosition, ray);
        }

        if (!handled && InputCompleted != null)
        {
            InputCompleted(source, relativePosition, ray);
        }
    }

    public void OnNavigationCanceled(InteractionSourceKind source, Vector3 relativePosition, Ray ray)
    {
        bool handled = false;
        if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
        {
            handled = GazeSelectionManager.Instance.SelectedTarget.OnNavigationCanceled(source, relativePosition, ray);
        }

        if (!handled && InputCanceled != null)
        {
            InputCanceled(source, relativePosition, ray);
        }
    }

    public void OnTapped(InteractionSourceKind source, int tapCount, Ray ray)
    {
        if (TransitionManager.Instance != null && !TransitionManager.Instance.InTransition)
        {
            bool handled = false;
            if (GazeSelectionManager.Instance && GazeSelectionManager.Instance.SelectedTarget)
            {
                handled = GazeSelectionManager.Instance.SelectedTarget.OnTapped(source, tapCount, ray);
            }
            else
            {
                PlacementControl placementControl = TransitionManager.Instance.ViewVolume.GetComponentInChildren<PlacementControl>();

                if (placementControl != null && placementControl.IsHolding)
                {
                    handled = placementControl.OnTapped(source, tapCount, ray);
                    if (ToolSounds.Instance)
                    {
                    ToolSounds.Instance.PlaySelectSound();
                }
            }
            }

            if (!handled && InputTapped != null)
            {
                InputTapped(source, tapCount, ray);
            }

            if (Tapped != null)
            {
                Tapped(source, tapCount, ray);
            }
        }
    }

    #endregion
}
