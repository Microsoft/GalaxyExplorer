// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace GalaxyExplorer
{
    public class GamepadInput : SingleInstance<GamepadInput>
    {
        [Tooltip("Game pad button to press for air tap.")]
        public string GamePadButtonA = "XBOX_A";

        [Tooltip("Game pad button to press to navigate back.")]
        public string GamePadButtonB = "XBOX_B";

        [Tooltip("Change this value to give a different source id to your controller.")]
        public uint GamePadId = 50000;

        [Tooltip("Elapsed time for hold started gesture in seconds.")]
        public float HoldStartedInterval = 2.0f;
        [Tooltip("Elapsed time for hold completed gesture in seconds.")]
        public float HoldCompletedInterval = 3.0f;

        [Tooltip("Name of the joystick axis that navigates around X.")]
        public string NavigateAroundXAxisName = "CONTROLLER_LEFT_STICK_HORIZONTAL";

        [Tooltip("Name of the joystick axis that navigates around Y.")]
        public string NavigateAroundYAxisName = "CONTROLLER_LEFT_STICK_VERTICAL";

        bool isAPressed = false;
        bool holdStarted = false;
        bool raiseOnce = false;
        bool navigationStarted = false;
        bool navigationCompleted = false;

        enum GestureState
        {
            APressed,
            NavigationStarted,
            NavigationCompleted,
            HoldStarted,
            HoldCompleted,
            HoldCanceled
        }

        GestureState currentGestureState;

        private void Update()
        {
            HandleGamepadAPressed();
            HandleGamepadBPressed();
        }

        private bool backButtonPressed = false;
        private void HandleGamepadBPressed()
        {
            if (Input.GetButtonDown(GamePadButtonB))
            {
                backButtonPressed = true;
            }
            if (backButtonPressed && Input.GetButtonUp(GamePadButtonB) && ToolManager.Instance)
            {
                var backButton = ToolManager.Instance.FindButtonByType(ButtonType.Back);
                if (backButton)
                {
                    backButton.ButtonAction();
                }
            }
        }

        private void HandleGamepadAPressed()
        {
            if (Input.GetButtonDown(GamePadButtonA))
            {
                //Debug.Log("Gamepad: A pressed");
                isAPressed = true;
                navigationCompleted = false;
                currentGestureState = GestureState.APressed;
                InputRouter.Instance.PressedSources.Add(InteractionSourceKind.Controller);
            }

            if (isAPressed)
            {
                HandleNavigation();

                if (!holdStarted && !raiseOnce && !navigationStarted)
                {
                    // Raise hold started when user has held A down for certain interval.
                    Invoke("HandleHoldStarted", HoldStartedInterval);
                }

                // Check if we get a subsequent release on A.
                HandleGamepadAReleased();
            }
        }

        private void HandleNavigation()
        {
            if (navigationCompleted)
            {
                return;
            }

            float displacementAlongX = 0.0f;
            float displacementAlongY = 0.0f;

            try
            {
                displacementAlongX = Input.GetAxis(NavigateAroundXAxisName);
                displacementAlongY = Input.GetAxis(NavigateAroundYAxisName);
            }
            catch (Exception)
            {
                Debug.LogWarningFormat("Ensure you have Edit > ProjectSettings > Input > Axes set with values: {0} and {1}",
                    NavigateAroundXAxisName, NavigateAroundYAxisName);
            }

            if (displacementAlongX != 0.0f || displacementAlongY != 0.0f || navigationStarted)
            {
                if (!navigationStarted)
                {
                    //Raise navigation started event.
                    //Debug.Log("GamePad: Navigation started");
                    InputRouter.Instance.OnNavigationStartedWorker(InteractionSourceKind.Controller, Vector3.zero, new Ray());
                    navigationStarted = true;
                    currentGestureState = GestureState.NavigationStarted;
                }

                Vector3 normalizedOffset = new Vector3(displacementAlongX,
                    displacementAlongY,
                    0);

                //Raise navigation updated event.
                //inputManager.RaiseNavigationUpdated(this, GamePadId, normalizedOffset);
                InputRouter.Instance.OnNavigationUpdatedWorker(InteractionSourceKind.Controller, normalizedOffset, new Ray());
            }
        }

        private void HandleGamepadAReleased()
        {
            if (Input.GetButtonUp(GamePadButtonA))
            {
                InputRouter.Instance.PressedSources.Remove(InteractionSourceKind.Controller);

                switch (currentGestureState)
                {
                    case GestureState.NavigationStarted:
                        navigationCompleted = true;
                        CancelInvoke("HandleHoldStarted");
                        CancelInvoke("HandleHoldCompleted");
                        InputRouter.Instance.OnNavigationCompletedWorker(InteractionSourceKind.Controller, Vector3.zero, new Ray());
                        Reset();
                        break;

                    case GestureState.HoldStarted:
                        CancelInvoke("HandleHoldCompleted");
                        Reset();
                        break;

                    case GestureState.HoldCompleted:
                        Reset();
                        break;

                    default:
                        CancelInvoke("HandleHoldStarted");
                        CancelInvoke("HandleHoldCompleted");
                        InputRouter.Instance.InternalHandleOnTapped();
                        Reset();
                        break;
                }
            }
        }

        private void Reset()
        {
            isAPressed = false;
            holdStarted = false;
            raiseOnce = false;
            navigationStarted = false;
        }

        private void HandleHoldStarted()
        {
            if (raiseOnce || currentGestureState == GestureState.HoldStarted || currentGestureState == GestureState.NavigationStarted)
            {
                return;
            }

            holdStarted = true;
            
            currentGestureState = GestureState.HoldStarted;
            raiseOnce = true;

            Invoke("HandleHoldCompleted", HoldCompletedInterval);
        }

        private void HandleHoldCompleted()
        {
            currentGestureState = GestureState.HoldCompleted;
        }
    }
}