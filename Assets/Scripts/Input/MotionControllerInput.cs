// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace GalaxyExplorer
{
    public class MotionControllerInput : SingleInstance<MotionControllerInput>
    {
        public bool UseAlternateGazeRay
        {
            get { return graspedHand != null; }
        }
        public Ray AlternateGazeRay
        {
            get
            {
                Debug.Assert(graspedHand != null, "ERROR: Don't use AlternateGazeRay without checking UseAlternateGazeRay first");
                return new Ray(graspedHand.position, graspedHand.forward);
            }
        }

        public ControllerInformation AlternateGazeRayControlerInformation
        {
            get { return graspedHand; }
        }

        public class ControllerInformation
        {
            public uint id = 0;
            public Vector3 position = Vector3.zero;
            public Vector3 forward = Vector3.zero;
            public bool grasped = false;
            public InteractionSourceHandedness handedness = InteractionSourceHandedness.Unknown;
            public float accumulatedY = 0f;
            public float accumulatedX = 0f;
        }

        // Using the grasp button will cause GE to replace the gaze cursor with
        // the pointer ray from the grasped controller. Since GE (currently)
        // only can handle input from a single source, we will only track one
        // controller at a time. The first one in wins.
        private ControllerInformation graspedHand = null;

        private Dictionary<uint, ControllerInformation> controllerDictionary = new Dictionary<uint, ControllerInformation>();

        private bool eventsRegistered = false;

        void Start()
        {
            Debug.Log("MotionControllerInput.Start()");
            if (MyAppPlatformManager.Platform != MyAppPlatformManager.PlatformId.ImmersiveHMD)
            {
                enabled = false;
                return;
            }

            InteractionManager.InteractionSourceDetected += InteractionManager_OnInteractionSourceDetected;
            InteractionManager.InteractionSourceLost += InteractionManager_OnInteractionSourceLost;
            InteractionManager.InteractionSourcePressed += InteractionManager_OnInteractionSourcePressed;
            InteractionManager.InteractionSourceReleased += InteractionManager_OnInteractionSourceReleased;
            InteractionManager.InteractionSourceUpdated += InteractionManager_OnInteractionSourceUpdated;
            eventsRegistered = true;
        }

        private void ValidateGraspStateTracking(ControllerInformation ci, InteractionSourceUpdatedEventArgs args)
        {
            Debug.Assert(ci != null);
            ci.grasped = args.state.grasped;

            if (ci.grasped && graspedHand == null)
            {
                graspedHand = ci;
            }
            else if (!ci.grasped && graspedHand != null && ci.id == graspedHand.id)
            {
                graspedHand = null;
            }
        }

        private void InteractionManager_OnInteractionSourceUpdated(InteractionSourceUpdatedEventArgs obj)
        {
            if (obj.state.source.kind != InteractionSourceKind.Controller) return;

            ControllerInformation ci = null;
            if (!controllerDictionary.TryGetValue(obj.state.source.id, out ci))
            {
                ci = AddNewControllerToDictionary(obj.state);
                if (ci == null) return;
            }

            // Update the grasp state for the current controller and the tracked
            // grasped hand if we didn't already have one.
            ValidateGraspStateTracking(ci, obj);

            // Update position and forward
            if (obj.state.sourcePose.TryGetPosition(out ci.position, InteractionSourceNode.Pointer))
            {
                // convert local position into world position
                ci.position = CameraCache.Main.transform.parent.TransformPoint(ci.position);
            }
            if (obj.state.sourcePose.TryGetForward(out ci.forward, InteractionSourceNode.Pointer))
            {
                // convert local rotation into world rotation
                ci.forward = CameraCache.Main.transform.parent.TransformDirection(ci.forward);
            }


            HandleNavigation(ci, obj);

            // Update the x/y accumulators for the grasped controler
            if (graspedHand != null &&
                graspedHand.handedness == ci.handedness)
            {
                float x = obj.state.thumbstickPosition.x;
                float y = obj.state.thumbstickPosition.y;
                if (Mathf.Abs(x) >= 0.1f) ci.accumulatedX += x;
                if (Mathf.Abs(y) >= 0.1f) ci.accumulatedY += y;
            }
        }

        private ControllerInformation navigatingHand = null;

        private void HandleNavigation(ControllerInformation ci, InteractionSourceUpdatedEventArgs obj)
        {
            float displacementAlongX = obj.state.thumbstickPosition.x;
            float displacementAlongY = obj.state.thumbstickPosition.y;

            if (Mathf.Abs(displacementAlongX) >= 0.1f ||
                Mathf.Abs(displacementAlongY) >= 0.1f ||
                navigatingHand != null)
            {
                if (navigatingHand == null)
                {
                    navigatingHand = ci;

                    //Raise navigation started event.
                    InputRouter.Instance.OnNavigationStartedWorker(InteractionSourceKind.Controller, Vector3.zero, new Ray());
                }

                if (navigatingHand.id == ci.id)
                {
                    Vector3 thumbValues = new Vector3(
                        displacementAlongX,
                        displacementAlongY,
                        0f);

                    InputRouter.Instance.OnNavigationUpdatedWorker(InteractionSourceKind.Controller, thumbValues, new Ray());
                }
            }
        }

        private void InteractionManager_OnInteractionSourceReleased(InteractionSourceReleasedEventArgs obj)
        {
            if (obj.state.source.kind != InteractionSourceKind.Controller) return;

            ControllerInformation ci = null;
            if (!controllerDictionary.TryGetValue(obj.state.source.id, out ci))
            {
                ci = AddNewControllerToDictionary(obj.state);
                if (ci == null) return;
            }

            switch (obj.pressType)
            {
                case InteractionSourcePressType.Select:
                    if (ci.handedness != InteractionSourceHandedness.Unknown)
                    {
                        if (navigatingHand != null &&
                            navigatingHand.id == ci.id)
                        {
                            navigatingHand = null;
                            InputRouter.Instance.OnNavigationCompletedWorker(InteractionSourceKind.Controller, Vector3.zero, new Ray());
                        }
                        else
                        {
                            PlayerInputManager.Instance.TriggerTapRelease();
                        }
                    }
                    break;

                case InteractionSourcePressType.Grasp:
                    if (graspedHand != null &&
                        graspedHand.id == ci.id)
                    {
                        ci.grasped = false;
                        ci.accumulatedX = 0f;
                        ci.accumulatedY = 0f;
                        graspedHand = null;
                    }
                    break;

                case InteractionSourcePressType.Menu:
                    if (ToolManager.Instance)
                    {
                        if (ToolManager.Instance.ToolsVisible)
                        {
                            ToolManager.Instance.UnselectAllTools();
                            ToolManager.Instance.HideTools(false);
                        }
                        else
                        {
                            ToolManager.Instance.ShowTools();
                        }
                    }
                    break;
            }
        }

        private void InteractionManager_OnInteractionSourcePressed(InteractionSourcePressedEventArgs obj)
        {
            if (obj.state.source.kind != InteractionSourceKind.Controller) return;

            ControllerInformation ci = null;
            if (!controllerDictionary.TryGetValue(obj.state.source.id, out ci))
            {
                ci = AddNewControllerToDictionary(obj.state);
                if (ci == null) return;
            }

            if (ci.handedness != InteractionSourceHandedness.Unknown)
            {
                switch (obj.pressType)
                {
                    case InteractionSourcePressType.Select:
                        if (PlayerInputManager.Instance)
                        {
                            PlayerInputManager.Instance.TriggerTapPress();
                        }
                        break;

                    case InteractionSourcePressType.Grasp:
                        if (graspedHand == null)
                        {
                            graspedHand = ci;
                        }
                        break;
                }
            }
        }

        #region Source_Lost_Detected
        private void InteractionManager_OnInteractionSourceLost(InteractionSourceLostEventArgs obj)
        {
            // update controllerDictionary
            if (obj.state.source.kind == InteractionSourceKind.Controller)
            {
                ControllerInformation ci;
                if (controllerDictionary.TryGetValue(obj.state.source.id, out ci))
                {
                    if (graspedHand != null && graspedHand.id == ci.id)
                    {
                        graspedHand = null;
                    }
                    Debug.LogFormat("Lost InteractionSource with controllerId={0}", ci.id);
                    controllerDictionary.Remove(ci.id);
                }

                if (controllerDictionary.Count == 0 &&
                    GamepadInput.Instance &&
                    !GamepadInput.Instance.enabled)
                {
                    // if we lost all (Motion)Controllers, enable the GamePad script
                    Debug.Log("Enabling GamepadInput instance");
                    GamepadInput.Instance.enabled = true;
                    InputRouter.Instance.EnableHoldAndNavigationGestures(true);
                }
            }
        }

        private ControllerInformation AddNewControllerToDictionary(InteractionSourceState sourceState)
        {
            ControllerInformation ci;
            if (controllerDictionary.TryGetValue(sourceState.source.id, out ci))
            {
                Debug.LogWarningFormat("controllerDictionary already tracking controller with id {0}", ci.id);
                return ci;
            }

            if (sourceState.source.kind == InteractionSourceKind.Controller &&
                sourceState.source.supportsGrasp &&
                sourceState.source.supportsPointing)
            {
                ci = new ControllerInformation();
                ci.id = sourceState.source.id;
                ci.grasped = sourceState.grasped;
                ci.handedness = sourceState.source.handedness;

                Debug.LogFormat("Acquired InteractionSource with controllerId={0}", ci.id);
                controllerDictionary.Add(ci.id, ci);

                if (GamepadInput.Instance &&
                    GamepadInput.Instance.enabled &&
                    InputRouter.Instance)
                {
                    // if we detected a (Motion)Controller, disable the GamePad script
                    Debug.Log("Disabling GamepadInput instance");
                    GamepadInput.Instance.enabled = false;
                    InputRouter.Instance.EnableHoldAndNavigationGestures(false);
                }

                return ci;
            }

            return null;
        }

        private void InteractionManager_OnInteractionSourceDetected(InteractionSourceDetectedEventArgs obj)
        {
            // update controllerDictionary
            AddNewControllerToDictionary(obj.state);
        }
        #endregion // Source_Lost_Detected

        protected override void OnDestroy()
        {
            if (eventsRegistered)
            {
                InteractionManager.InteractionSourceDetected -= InteractionManager_OnInteractionSourceDetected;
                InteractionManager.InteractionSourceLost -= InteractionManager_OnInteractionSourceLost;
                InteractionManager.InteractionSourcePressed -= InteractionManager_OnInteractionSourcePressed;
                InteractionManager.InteractionSourceReleased -= InteractionManager_OnInteractionSourceReleased;
                InteractionManager.InteractionSourceUpdated -= InteractionManager_OnInteractionSourceUpdated;
            }
            base.OnDestroy();
        }
    }
}