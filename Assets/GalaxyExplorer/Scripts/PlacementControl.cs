﻿// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class PlacementControl : MonoBehaviour, IInputHandler
    {
        public delegate void ContentPlacedCallback(Vector3 position);
        public ContentPlacedCallback OnContentPlaced;

        private Tagalong volumeTagalong;
        private Interpolator volumeInterpolator;

        private void Start()
        {
            volumeTagalong = gameObject.GetComponent<Tagalong>();
            volumeInterpolator = gameObject.GetComponent<Interpolator>();

            // if platform is desktop or immersive headset then disable tag along
            GalaxyExplorerManager manager = FindObjectOfType<GalaxyExplorerManager>();
            if (manager && (GalaxyExplorerManager.IsDesktop))
            {
                StartCoroutine(ReleaseContent());
            }
        }

        private IEnumerator ReleaseContent()
        {
            // Wait for 1 sec so previous transition finishes
            yield return new WaitForSeconds(1);

            // Disable Tagalong and interpolator
            volumeTagalong.enabled = false;
            volumeInterpolator.enabled = false;

            if (OnContentPlaced != null)
            {
                OnContentPlaced.Invoke(transform.position);
            }

            yield return null;
        }

        public void OnInputDown(InputEventData eventData)
        {
        }

        public void OnInputUp(InputEventData eventData)
        {
            StartCoroutine(ReleaseContent());
        }
    }
}