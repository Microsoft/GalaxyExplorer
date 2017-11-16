// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using UnityEngine;

namespace GalaxyExplorer
{
    public class LSRPlaneModifier : SingleInstance<LSRPlaneModifier>
    {
        public LayerMask TargetCollisionLayers;
        public LayerMask FallbackCollisionLayers;

        public bool IgnoreToolBox = true;

        public float MinDistanceInMeters = 1f;
        public float MaxDistanceInMeters = 2.5f;

        private Transform head;
        private float lastDistance = 2;

        private void Start()
        {
            // Cache the head transform
            head = Camera.main.transform;
        }

        private void Update()
        {
            // If we should update the LSR plane and have a valid transform, set the LSR plane
            var cam = Camera.main;
            var camPos = cam.transform.position;

            var foundLsrTarget = false;
            var raycastResults = Physics.RaycastAll(camPos, cam.transform.forward, float.MaxValue, TargetCollisionLayers);
            if (raycastResults.Length > 0)
            {
                foreach (var result in raycastResults)
                {
                    if (result.collider.GetComponent<SceneSizer>() == null && (!IgnoreToolBox || result.collider.GetComponentInParent<ToolManager>() == null))
                    {
                        SetLSRPointByWorldPosition(result.point);
#if UNITY_EDITOR
                        Debug.DrawLine(camPos, result.point);
#endif
                        foundLsrTarget = true;

                        break;
                    }
                }
            }

            if (!foundLsrTarget)
            {
                RaycastHit lsrHit;
                if (Physics.Raycast(camPos, cam.transform.forward, out lsrHit, float.MaxValue, FallbackCollisionLayers))
                {
                    SetLSRPointByWorldPosition(lsrHit.point);
#if UNITY_EDITOR
                    Debug.DrawLine(camPos, lsrHit.point);
#endif
                    foundLsrTarget = true;
                }
            }

            if (!foundLsrTarget)
            {
                // No LSR plane found, just keep the same distance
                ApplyCurrentDistanceToFocusPoint();
            }
        }

        private void SetLSRPointByWorldPosition(Vector3 position)
        {
            // Get a vector from the user to this position
            // TODO: LSR 
            // Does this function expect coordinates in world space?

            // Scale our head forward vector by our distance to the LSR target
            lastDistance = Mathf.Clamp(Vector3.Distance(position, head.position), MinDistanceInMeters, MaxDistanceInMeters);

            ApplyCurrentDistanceToFocusPoint();
        }

        private void ApplyCurrentDistanceToFocusPoint()
        {
            Vector3 scaledForward = head.forward * lastDistance;
            UnityEngine.XR.WSA.HolographicSettings.SetFocusPointForFrame(head.position + scaledForward, -head.forward);
        }
    }
}