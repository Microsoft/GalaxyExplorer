// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using UnityEngine;

namespace GalaxyExplorer
{
    public class WorldAnchorHandler : SingleInstance<WorldAnchorHandler>
    {
        private UnityEngine.XR.WSA.WorldAnchor viewLoaderAnchor;
        private bool viewLoaderAnchorActivelyTracking = true;

        private const float placeViewLoaderWaitTime = 5.0f; // seconds
        private float timeToReplaceViewLoader = placeViewLoaderWaitTime;

        private PlacementControl placementControl;

        private void Start()
        {
            placementControl = TransitionManager.Instance.ViewVolume.GetComponentInChildren<PlacementControl>();

            if (placementControl != null)
            {
                placementControl.ContentHeld += PlacementControl_ContentHeld;
                placementControl.ContentPlaced += PlacementControl_ContentPlaced;
            }

            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.ResetStarted += ResetStarted;
            }
        }

        private void Update()
        {
            // Update will be suspended if the app is suspended or if the device is not tracking
            if (viewLoaderAnchor != null && !viewLoaderAnchorActivelyTracking)
            {
                timeToReplaceViewLoader -= Time.deltaTime;

                if (timeToReplaceViewLoader <= 0.0f)
                {
                    placementControl.TogglePinnedState();
                }
            }
        }

        public void CreateWorldAnchor()
        {
            GameObject sourceObject = ViewLoader.Instance.gameObject;

            viewLoaderAnchor = sourceObject.AddComponent<UnityEngine.XR.WSA.WorldAnchor>();

            viewLoaderAnchor.OnTrackingChanged += GalaxyWorldAnchor_OnTrackingChanged;

            timeToReplaceViewLoader = placeViewLoaderWaitTime;
        }

        public void DestroyWorldAnchor()
        {
            if (viewLoaderAnchor != null)
            {
                viewLoaderAnchor.OnTrackingChanged -= GalaxyWorldAnchor_OnTrackingChanged;
                DestroyImmediate(viewLoaderAnchor);
            }
        }

        private void SetViewLoaderActive(bool active)
        {
            if (viewLoaderAnchor != null)
            {
                for (int i = 0; i < viewLoaderAnchor.transform.childCount; i++)
                {
                    viewLoaderAnchor.transform.GetChild(i).gameObject.SetActive(active);
                }
            }
        }

        #region Callbacks
        private void PlacementControl_ContentHeld()
        {
            // Make sure our content is active/shown
            SetViewLoaderActive(true);

            // Destroy our galaxy WorldAnchor if we are moving it
            DestroyWorldAnchor();
        }

        private void PlacementControl_ContentPlaced()
        {
            if (ViewLoader.Instance != null)
            {
                CreateWorldAnchor();
            }
        }

        private void GalaxyWorldAnchor_OnTrackingChanged(UnityEngine.XR.WSA.WorldAnchor self, bool located)
        {
            viewLoaderAnchorActivelyTracking = located;

            SetViewLoaderActive(located);
        }

        private void ResetStarted()
        {
            if (viewLoaderAnchor != null)
            {
                DestroyWorldAnchor();

                TransitionManager.Instance.ResetFinished += ResetFinished;
            }
        }

        private void ResetFinished()
        {
            CreateWorldAnchor();

            TransitionManager.Instance.ResetFinished -= ResetFinished;
        }
        #endregion
    }
}