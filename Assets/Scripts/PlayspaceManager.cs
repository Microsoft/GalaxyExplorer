// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR;
using UnityEngine.XR;

namespace GalaxyExplorer
{
    public class PlayspaceManager : SingleInstance<PlayspaceManager>
    {
        public AnimationCurve FloorFadeCurve;
        public float FadeInOutTime = 1.0f;
        public GameObject SpaceBackground;

        [Tooltip("Drag the MotionControllers prefab here.")]
        public GameObject MotionControllers;

        public Material PlayspaceBoundsMaterial;

        [Tooltip("If true, the floor grid is rendered, even if the device isn't an Opaque HMD; Useful for screenshots.")]
        public bool useFakeFloor = false;
        public GameObject FloorQuad;
        private bool floorVisible = false;
        private bool recalculateFloor = false;

        private KeywordManager keywordManager = null;

        private void Awake()
        {
            keywordManager = GetComponent<KeywordManager>();

            // parent the MotionControllers to the Camera rigs
            if (MotionControllers != null)
            {
                Debug.LogFormat("Moving MotionControllers parent from {0}...", MotionControllers.transform.parent.gameObject.name);
                MotionControllers.transform.SetParent(CameraCache.Main.transform.parent, true);
                Debug.LogFormat("... to {0}", MotionControllers.transform.parent.gameObject.name);
            }

            // Grab the MixedRealityTeleport component from here and use it as
            // a template for a new one added to CameraRigs
            var mrt = GetComponent<MixedRealityTeleport>();
            if (mrt)
            {
                var mrtNew = CameraCache.Main.transform.parent.gameObject.AddComponent<MixedRealityTeleport>();
                mrtNew.gameObject.AddComponent<SetGlobalListener>();

                mrtNew.LeftThumbstickX = mrt.LeftThumbstickX;
                mrtNew.LeftThumbstickY = mrt.LeftThumbstickY;
                mrtNew.RightThumbstickX = mrt.RightThumbstickX;
                mrtNew.RightThumbstickY = mrt.RightThumbstickY;
                mrtNew.EnableTeleport = mrt.EnableTeleport;
                mrtNew.EnableRotation = mrt.EnableRotation;
                mrtNew.EnableStrafe = mrt.EnableStrafe;
                mrtNew.RotationSize = mrt.RotationSize;
                mrtNew.StrafeAmount = mrt.StrafeAmount;
                mrtNew.TeleportMarker = mrt.TeleportMarker;
                mrtNew.enabled = true;

                var sgl = GetComponent<SetGlobalListener>();
                if (sgl)
                {
                    DestroyImmediate(sgl);
                }
                DestroyImmediate(mrt);
            }
        }

        // Use this for initialization
        private IEnumerator Start()
        {
            // Check to see if we are on an occluded HMD
            if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD)
            {
                floorVisible = XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
                if (floorVisible)
                {
                    // Position our floor at (0,0,0) which should be where the
                    // shell says it is supposed to be from OOBE calibration
                    FloorQuad.transform.position = Vector3.zero;
                    useFakeFloor = false;
                }
                else
                {
                    // Theoretically, Unity does this automatically...
                    XRDevice.SetTrackingSpaceType(TrackingSpaceType.Stationary);
                    InputTracking.Recenter();
                    floorVisible = useFakeFloor = true;
                }
            }

            if (!floorVisible)
            {
                // If not, disable the playspace manager
                gameObject.SetActive(false);
                if (keywordManager.enabled)
                {
                    keywordManager.StopKeywordRecognizer();
                }
                yield break;
            }

            // Move the starfield out of the hierarchy
            SpaceBackground.transform.SetParent(null);

            // parent the FloorQuad to the Camera rigs so it stays "locked" to the real world
            FloorQuad.transform.SetParent(CameraCache.Main.transform.parent, true);
            FloorQuad.SetActive(true);
            FadeInOut(floorVisible);
            recalculateFloor = true;
        }

        private void OnDrawGizmos()
        {
            Vector3 lossyScale = FloorQuad.transform.lossyScale;
            FloorQuad.GetComponent<Renderer>().sharedMaterial.SetVector("_WorldScale", new Vector4(lossyScale.x, lossyScale.y, lossyScale.z, 0));
        }

        private void Update()
        {
            if (recalculateFloor)
            {
                if (useFakeFloor)
                {
                    var floorPosition = FloorQuad.transform.position;
                    floorPosition.y = -0.5f;
                    FloorQuad.transform.position = floorPosition;
                    FloorQuad.transform.localScale = Vector3.one * 10f;
                    FloorQuad.GetComponent<Renderer>().sharedMaterial.SetInt("_LinesPerMeter", 10);
                    FloorQuad.GetComponent<Renderer>().sharedMaterial.SetFloat("_LineScale", 0.00075f);
                    recalculateFloor = false;
                }
                else
                {
                    Vector3 newScale = FloorQuad.transform.localScale;
                    // TODO: TryGetDimensions always returns false on Unity 2017.2.0b9
                    if (Boundary.TryGetDimensions(out newScale) || true)
                    {
                        // inflate bounds by 1 meter all around
                        newScale.x += 2.0f;
                        newScale.y += 2.0f;
                        FloorQuad.transform.localScale = newScale;
                        recalculateFloor = false;
                    }
                    Debug.Log(string.Format("FloorQuad.localScale  is: {0}", FloorQuad.transform.localScale.ToString()));

                    Vector3 lossyScale = FloorQuad.transform.lossyScale;
                    FloorQuad.GetComponent<Renderer>().sharedMaterial.SetVector("_WorldScale", new Vector4(lossyScale.x, lossyScale.y, lossyScale.z, 0));
                }
            }
        }

        private void FadeInOut(bool fadeIn)
        {
            if (fadeIn)
            {
                FloorQuad.SetActive(true);
            }
            StartCoroutine(TransitionManager.Instance.FadeContent(
                FloorQuad,
                fadeIn ? TransitionManager.FadeType.FadeIn : TransitionManager.FadeType.FadeOut,
                Instance.FadeInOutTime,
                Instance.FloorFadeCurve));
        }

        public void ToggleFloor()
        {
            floorVisible = !floorVisible;
            FadeInOut(floorVisible);
        }
    }
}