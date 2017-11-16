// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace GalaxyExplorer
{
    public class HandInput : SingleInstance<HandInput>
    {
        public struct HandState
        {
            public bool valid;
            public uint id;
            public bool isTappingDown;
            public float tapPressedStartTime;
            public Vector3 latestTapStartPosition;
            public Vector3 latestObservedPosition;
        }

        private HandState[] handStates = new HandState[2];

        private enum QueuedActionType
        {
            HandEnter,
            HandExit,
            HandMoved,
            TapPressed,
            TapReleased
        }

        private struct QueuedAction
        {
            public QueuedActionType actionType;
            public uint id;
            public Vector3 position;
            public float timestamp;
        }

        private Queue<QueuedAction> actionQueue = new Queue<QueuedAction>();

        public bool HandsInFOV
        {
            get
            {
                for (int i = 0; i < handStates.Length; ++i)
                {
                    if (handStates[i].valid)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public event System.Action<bool> HandInFOVChanged;

        protected void Awake()
        {
            if (MyAppPlatformManager.Platform != MyAppPlatformManager.PlatformId.HoloLens)
            {
                DestroyObject(this);
                return;
            }
            // Start out with a clean state by resetting both hand structures.
            for (int i = 0; i < handStates.Length; ++i)
            {
                UnregisterHandByIndex(i);
            }
        }

        private bool eventsAreRegistered = false;

        private void TryToRegisterEvents()
        {
            if (!eventsAreRegistered)
            {
                InteractionManager.InteractionSourceDetected += OtherThread_OnInteractionSourceDetected;
                InteractionManager.InteractionSourceUpdated += OtherThread_OnInteractionSourceUpdated;
                InteractionManager.InteractionSourceLost += OtherThread_OnInteractionSourceLost;
                InteractionManager.InteractionSourcePressed += OtherThread_InteractionSourcePressed;
                InteractionManager.InteractionSourceReleased += OtherThread_InteractionSourceReleased;
                eventsAreRegistered = true;
            }
        }

        private void TryToUnregisterEvents()
        {
            if (eventsAreRegistered)
            {
                InteractionManager.InteractionSourceDetected -= OtherThread_OnInteractionSourceDetected;
                InteractionManager.InteractionSourceUpdated -= OtherThread_OnInteractionSourceUpdated;
                InteractionManager.InteractionSourceLost -= OtherThread_OnInteractionSourceLost;
                InteractionManager.InteractionSourcePressed -= OtherThread_InteractionSourcePressed;
                InteractionManager.InteractionSourceReleased -= OtherThread_InteractionSourceReleased;
                eventsAreRegistered = false;
            }
        }

        private void Start()
        {
            if (PlayerInputManager.Instance == null)
            {
                Debug.LogWarning("No active PlayerInputManager found. Not all HandInput functionality will be available.");
            }

            TryToRegisterEvents();
        }

        protected override void OnDestroy()
        {
            TryToUnregisterEvents();

            // Reset active state for both hands.
            for (int i = 0; i < handStates.Length; ++i)
            {
                UnregisterHandByIndex(i);
            }
            base.OnDestroy();
        }

        private void OnEnable()
        {
            TryToRegisterEvents();
        }

        private void OnDisable()
        {
            TryToUnregisterEvents();
        }

        private void Update()
        {
            lock (actionQueue)
            {
                while (actionQueue.Count > 0)
                {
                    QueuedAction queuedAction = actionQueue.Dequeue();

                    switch (queuedAction.actionType)
                    {
                        case QueuedActionType.HandEnter:
                            OnHandEnter(queuedAction.id, queuedAction.position);
                            break;

                        case QueuedActionType.HandExit:
                            OnHandExit(queuedAction.id, queuedAction.position);
                            break;

                        case QueuedActionType.HandMoved:
                            OnHandMoved(queuedAction.id, queuedAction.position, queuedAction.timestamp);
                            break;

                        case QueuedActionType.TapPressed:
                            OnHandTapPressed(queuedAction.id, queuedAction.position);
                            break;

                        case QueuedActionType.TapReleased:
                            OnHandTapReleased(queuedAction.id, queuedAction.position);
                            break;
                    }
                }
            }
        }

        private bool GetHandState(int index, ref HandState handState)
        {
            if (index >= 0 && index < handStates.Length)
            {
                handState = handStates[index];
                return true;
            }

            return false;
        }

        private void OnHandEnter(uint id, Vector3 position)
        {
            if (HandInFOVChanged != null && !HandsInFOV)
            {
                HandInFOVChanged(true);
            }

            RegisterHand(id, position);
        }

        private void OnHandExit(uint id, Vector3 position)
        {
            int index = RegisterHand(id, position);
            if (index != -1)
            {
                if (handStates[index].isTappingDown && PlayerInputManager.Instance != null)
                {
                    PlayerInputManager.Instance.TriggerTapRelease();
                }
            }

            UnregisterHandById(id);

            if (HandInFOVChanged != null && !HandsInFOV)
            {
                HandInFOVChanged(false);
            }
        }

        private void OnHandMoved(uint id, Vector3 position, float timestamp)
        {
            RegisterHand(id, position);
        }

        private void OnHandTapPressed(uint id, Vector3 position)
        {
            int index = RegisterHand(id, position);
            if (index != -1)
            {
                handStates[index].isTappingDown = true;
                handStates[index].tapPressedStartTime = Time.time;
                handStates[index].latestTapStartPosition = position;

                if (PlayerInputManager.Instance != null)
                {
                    PlayerInputManager.Instance.TriggerTapPress();
                }
            }
            else
            {
                Debug.Log("HandInput.OnHandTapPressed() - RegisterHand() failed!");
            }
        }

        private void OnHandTapReleased(uint id, Vector3 position)
        {
            int index = RegisterHand(id, position);
            if (index != -1)
            {
                if (handStates[index].isTappingDown)
                {
                    handStates[index].isTappingDown = false;

                    if (PlayerInputManager.Instance != null)
                    {
                        PlayerInputManager.Instance.TriggerTapRelease();
                    }
                }
            }
            else
            {
                Debug.Log("HandInput.OnHandTapReleased() - RegisterHand() failed!");
            }
        }

        // Hand State utilities
        #region Hand Registration
        private int RegisterHand(uint id, Vector3 position)
        {
            int handIndex = GetRegisteredHandIndex(id);

            if (handIndex != -1)
            {
                // Item already exists.  Update its properties.
                handStates[handIndex].latestObservedPosition = position;
            }
            else
            {
                // New item.  Find the first available slot and add it.
                for (int i = 0; i < handStates.Length; ++i)
                {
                    if (!handStates[i].valid)
                    {
                        handStates[i].valid = true;
                        handStates[i].id = id;
                        handStates[i].isTappingDown = false;
                        handStates[i].tapPressedStartTime = 0.0f;
                        handStates[i].latestTapStartPosition = Vector3.zero;
                        handStates[i].latestObservedPosition = position;

                        handIndex = i;

                        break;
                    }
                }

                if (handIndex == -1)
                {
                    Debug.LogError("HandInput failed to find open slot for id " + id + " (existing ids: " + handStates[0].id + " and " + handStates[1].id + ")");
                }
            }

            return handIndex;
        }

        private void UnregisterHandById(uint id)
        {
            for (int i = 0; i < handStates.Length; ++i)
            {
                if (handStates[i].id == id)
                {
                    UnregisterHandByIndex(i);
                    break;
                }
            }
        }

        private void UnregisterHandByIndex(int index)
        {
            if (index >= 0 && index < handStates.Length)
            {
                handStates[index].valid = false;
                handStates[index].id = uint.MaxValue;
                handStates[index].isTappingDown = false;
                handStates[index].latestTapStartPosition = Vector3.zero;
                handStates[index].latestObservedPosition = Vector3.zero;
            }
        }

        private int GetRegisteredHandIndex(uint id)
        {
            int index = -1;

            for (int i = 0; i < handStates.Length; ++i)
            {
                if (handStates[i].valid && handStates[i].id == id)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        #endregion

        #region OtherThread

        // IMPORTANT
        // Everything below this point is executed on a separate thread.  For thread safety,
        // the only code these functions should execute is pushing a task into a synchronized
        // queue for later analysis on the main thread.
        private void OtherThread_OnInteractionSourceDetected(InteractionSourceDetectedEventArgs args)
        {
            Vector3 handPosition;

            if (args.state.source.kind == InteractionSourceKind.Hand && args.state.sourcePose.TryGetPosition(out handPosition))
            {
                Vector3 position = Camera.main.transform.rotation * handPosition;

                QueuedAction newAction = new QueuedAction { actionType = QueuedActionType.HandEnter, id = args.state.source.id, position = position, timestamp = Time.time };

                lock (actionQueue)
                {
                    actionQueue.Enqueue(newAction);
                }
            }
        }

        private void OtherThread_OnInteractionSourceUpdated(InteractionSourceUpdatedEventArgs args)
        {
            Vector3 handPosition;

            if (args.state.source.kind == InteractionSourceKind.Hand && args.state.sourcePose.TryGetPosition(out handPosition))
            {
                Vector3 position = Camera.main.transform.rotation * handPosition;
                QueuedAction newAction = new QueuedAction { actionType = QueuedActionType.HandMoved, id = args.state.source.id, position = position, timestamp = Time.time };

                lock (actionQueue)
                {
                    actionQueue.Enqueue(newAction);
                }
            }
        }

        private void OtherThread_OnInteractionSourceLost(InteractionSourceLostEventArgs args)
        {
            Vector3 handPosition;

            if (args.state.source.kind == InteractionSourceKind.Hand && args.state.sourcePose.TryGetPosition(out handPosition))
            {
                Vector3 position = Camera.main.transform.rotation * handPosition;
                QueuedAction newAction = new QueuedAction { actionType = QueuedActionType.HandExit, id = args.state.source.id, position = position, timestamp = Time.time };

                lock (actionQueue)
                {
                    actionQueue.Enqueue(newAction);
                }
            }
        }

        private void OtherThread_InteractionSourcePressed(InteractionSourcePressedEventArgs args)
        {
            Vector3 handPosition;

            if (args.state.source.kind == InteractionSourceKind.Hand && args.state.sourcePose.TryGetPosition(out handPosition))
            {
                QueuedAction newAction = new QueuedAction { actionType = QueuedActionType.TapPressed, id = args.state.source.id, position = handPosition, timestamp = Time.time };

                lock (actionQueue)
                {
                    actionQueue.Enqueue(newAction);
                }
            }
        }

        private void OtherThread_InteractionSourceReleased(InteractionSourceReleasedEventArgs args)
        {
            Vector3 handPosition;

            if (args.state.source.kind == InteractionSourceKind.Hand && args.state.sourcePose.TryGetPosition(out handPosition))
            {
                QueuedAction newAction = new QueuedAction { actionType = QueuedActionType.TapReleased, id = args.state.source.id, position = handPosition, timestamp = Time.time };

                lock (actionQueue)
                {
                    actionQueue.Enqueue(newAction);
                }
            }
        }

        #endregion
    }
}