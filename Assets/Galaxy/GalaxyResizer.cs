// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using UnityEngine;

namespace GalaxyExplorer
{
    public class GalaxyResizer : SingleInstance<GalaxyResizer>
    {
        private BoxCollider cursorCollisionBackground = null;

        void Awake()
        {
            transform.localScale = transform.localScale * MyAppPlatformManager.GalaxyScaleFactor;

            SpiralGalaxy[] spirals = GetComponentsInChildren<SpiralGalaxy>();
            foreach (var spiral in spirals)
            {
                if (spiral.tintMult < 1)
                {
                    spiral.tintMult = MyAppPlatformManager.SpiralGalaxyTintMultConstant;
                    break;
                }
            }

            // disable the script if we aren't on an immersive HMD
            // if we are, we want update to be called so we can do some layer adjusting...
            enabled = (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD);
            if (enabled)
            {
                cursorCollisionBackground = GetComponentInChildren<BoxCollider>();
                if (cursorCollisionBackground == null ||
                    !cursorCollisionBackground.gameObject.name.Equals("CursorCollisionBackground"))
                {
                    Debug.Log("Couldn't find CursorCollisionBackground...");
                    enabled = false;
                }
            }
        }

        void Update()
        {
            if (MotionControllerInput.Instance)
            {
                cursorCollisionBackground.enabled = !MotionControllerInput.Instance.UseAlternateGazeRay;
            }
        }
    }
}