// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class GazeSelection : MonoBehaviour
    {
        [HeaderAttribute("Gaze Search")]
        [Tooltip("Distance along the gaze vector to search for valid targets to select.")]
        public float GazeDistance = 30.0f;
        [HeaderAttribute("Spherical Cone Search")]
        public bool UseSphericalConeSearch = true;
        [Tooltip("If no objects are found along the gaze vector, the average position of objects found within this angle of the gaze vector are selected.")]
        public float GazeSpreadDegrees = 30.0f;

        // ordered from closest to gaze to farthest
        private SortedList<float, RaycastHit> selectedTargets;

        public IList<RaycastHit> SelectedTargets
        {
            get { return selectedTargets != null ? selectedTargets.Values : null; }
        }

        private float targetSpreadMinValue;
        private PlacementControl placementControl;

        private Ray gazeRay;

        private void Start()
        {
            if (Camera.main == null)
            {
                Debug.LogError(" GazeSelection:No main camera exists in the scene, unable to use GazeSelection.", this);
                GameObject.Destroy(this);
                return;
            }

            if (Cursor.Instance == null)
            {
                Debug.LogError("GazeSelection: no target layer masks can be used because the Cursor was not found.", this);
                GameObject.Destroy(this);
                return;
            }

            if (TransitionManager.Instance == null)
            {
                Debug.LogWarning("GazeSelection: No TransitionManager found, so input is not disabled during transitions.");
            }
            else if (TransitionManager.Instance.ViewVolume != null)
            {
                placementControl = TransitionManager.Instance.ViewVolume.GetComponentInChildren<PlacementControl>();
            }

            selectedTargets = new SortedList<float, RaycastHit>();

            if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD &&
                GazeSpreadDegrees < 10f)
            {
                GazeSpreadDegrees = 10f;
            }

            targetSpreadMinValue = Mathf.Cos(Mathf.Deg2Rad * GazeSpreadDegrees);

            if (!UnityEngine.XR.XRDevice.isPresent)
            {
#if !UNITY_EDITOR
                // Unity 2017.2.0b8 changed something. Commenting out this line
                // allows you to select things other than the center of the
                // galaxy using the mouse in a 2d Xaml app running on a desktop
                //UseSphericalConeSearch = false;
#endif
            }
        }

        public void Update()
        {
            selectedTargets.Clear();

            // disable GazeSelection if we are in the introduction, in the
            // middle of a transition, or we are in placement mode.
            if (TransitionManager.Instance != null &&
                !TransitionManager.Instance.InTransition &&
                !TransitionManager.Instance.IsIntro &&
                (placementControl == null || !placementControl.IsHolding))                                                                       
            {
                Transform camTrans = Camera.main.transform;

                if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD &&
                    MotionControllerInput.Instance &&
                    MotionControllerInput.Instance.UseAlternateGazeRay)
                {
                    gazeRay = MotionControllerInput.Instance.AlternateGazeRay;
                }
#if !UNITY_EDITOR
                else if (!UnityEngine.XR.XRDevice.isPresent)
                {
                    gazeRay = Camera.main.ScreenPointToRay(InputRouter.Instance.XamlMousePosition);
                    gazeRay.origin += (Camera.main.nearClipPlane * gazeRay.direction);
                }
#endif
                else
                {
                    gazeRay.origin = camTrans.position + (Camera.main.nearClipPlane * camTrans.forward);
                    gazeRay.direction = camTrans.forward;
                }

                foreach (Cursor.PriorityLayerMask priorityMask in Cursor.Instance.prioritizedCursorMask)
                {
                    switch (priorityMask.collisionType)
                    {
                        case Cursor.CursorCollisionSearch.RaycastSearch:
                            RaycastHit info;
                            if (Physics.Raycast(gazeRay, out info, GazeDistance, priorityMask.layers))
                            {
                                selectedTargets.Add(0.0f, info);
                            }

                            break;

                        case Cursor.CursorCollisionSearch.SphereCastSearch:
                            if (UseSphericalConeSearch)
                            {
                                // calculate radius of sphere to cast based on GazeSpreadDegrees at GazeDistance
                                float sphereRadius = GazeDistance * Mathf.Tan(Mathf.Deg2Rad * (GazeSpreadDegrees / 2.0f));
                                // get all target objects in a sphere from the camera
                                RaycastHit[] hitTargets = Physics.SphereCastAll(gazeRay, sphereRadius, GazeDistance, priorityMask.layers);

                                // only consider target objects that are within the target spread angle specified on start
                                foreach (RaycastHit target in hitTargets)
                                {
                                    Vector3 toTarget = Vector3.Normalize(target.transform.position - gazeRay.origin);
                                    float dotProduct = Vector3.Dot(gazeRay.direction.normalized, toTarget.normalized);

                                    // The dotProduct of our two normalized vectors is equivalent to the cosine
                                    // of the angle between them. If it is larger than the targetSpreadValue
                                    // established in Start(), that means the hit occurred within the
                                    // cone and the hitTarget should be added to our list of selectedTargets.
                                    if (dotProduct >= targetSpreadMinValue)
                                    {
                                        selectedTargets[-dotProduct] = target;
                                    }
                                }
                            }

                            break;
                    }

                    if (selectedTargets.Count > 0)
                    {
                        break;
                    }
                }
            }
        }
    }
}