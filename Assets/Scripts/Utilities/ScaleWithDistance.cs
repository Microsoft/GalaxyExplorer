// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class ScaleWithDistance : MonoBehaviour
    {
        private Transform scaleTarget;
        private float lastRescaleAmount = 1f;

        [Tooltip("Distance at which the object is scaled normally. (At other distances it will scale up or down to maintain an equivalent portion of the FOV.)")]
        [Range(0.1f, 100.0f)]
        public float IntendedViewDistance = 5f;

        [Tooltip("Minimum scale factor. (Getting too close will make the object fill more of the FOV.)")]
        [Range(0.01f, 1000.0f)]
        public float MinScale = 0.1f;

        [Tooltip("Maximum scale factor. (Getting too far will make the object use less of the FOV.)")]
        [Range(0.01f, 1000.0f)]
        public float MaxScale = 10f;

        [Tooltip("Check this to constantly rescale. (Otherwise the rescale will just happen once.)")]
        public bool RescaleEveryFrame = false;

        [Tooltip("This scales the rescale effect. (Lower makes this script do less, higher exaggerates what this script does.)")]
        [Range(0f, 100f)]
        public float RescaleWarp = 1f;

        private bool rescalingEveryFrame = false;

        private void Awake()
        {
            // insert a new GameObject above this one with the same transform, except a default scale
            scaleTarget = new GameObject("ScaleWithDistance target").transform;
            scaleTarget.SetParent(transform.parent, false);
            scaleTarget.localPosition = Vector3.zero;
            scaleTarget.localScale = Vector3.one;
            scaleTarget.localRotation = Quaternion.identity;

            // move this transform under that new one
            transform.SetParent(scaleTarget, true);

            float scale = GetScaleFromPositionWithNoCamera(transform.position);
            scaleTarget.localScale = Vector3.one * scale;
            lastRescaleAmount = scale;
        }

        private void OnEnable()
        {
            if (RescaleEveryFrame)
            {
                StartCoroutine(RescaleCoroutine());
            }
            else
            {
                Rescale();
            }
        }

        private IEnumerator RescaleCoroutine()
        {
            rescalingEveryFrame = true;
            yield return null;

            while (RescaleEveryFrame)
            {
                Rescale();
                yield return null;
            }

            rescalingEveryFrame = false;
        }

        public float GetScaleFromPositionWithNoCamera(Vector3 position)
        {
            float currentDist = position.magnitude;

            // use the warp setting to scale the distance relative to the intended view distance
            float warpedDist = ((currentDist - IntendedViewDistance) * RescaleWarp) + IntendedViewDistance;

            return Mathf.Clamp(warpedDist / IntendedViewDistance, MinScale, MaxScale);
        }

        public float GetScaleFromPosition(Vector3 position)
        {
            if (Camera.main == null)
            {
                return 1.0f;
            }

            float currentDist = (Camera.main.transform.position - position).magnitude;

            // use the warp setting to scale the distance relative to the intended view distance
            float warpedDist = ((currentDist - IntendedViewDistance) * RescaleWarp) + IntendedViewDistance;

            return Mathf.Clamp(warpedDist / IntendedViewDistance, MinScale, MaxScale);
        }

        public void Rescale()
        {
            // only do this if the script is running
            if (enabled && gameObject.activeInHierarchy)
            {
                float scale = GetScaleFromPosition(transform.position);

                // don't set localScale unless it has changed enough
                if (Mathf.Abs(scale - lastRescaleAmount) > Mathf.Epsilon)
                {
                    scaleTarget.localScale = Vector3.one * scale;
                    lastRescaleAmount = scale;
                }
            }
        }

        public void SetRescaleEveryFrame(bool everyFrame)
        {
            RescaleEveryFrame = everyFrame;

            if (everyFrame && rescalingEveryFrame == false)
            {
                StartCoroutine(RescaleCoroutine());
            }
            else if (rescalingEveryFrame == true && !everyFrame)
            {
                StopCoroutine("RescaleCoroutine");
                rescalingEveryFrame = false;
            }
        }
    }
}