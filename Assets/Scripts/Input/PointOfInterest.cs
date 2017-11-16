// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class PointOfInterest : GazeSelectionTarget
    {
        public class POIFader : Fader
        {
            private MeshFilter filter;
            private List<Color> colors;

            public PointOfInterest parent;

            private void Awake()
            {
                filter = GetComponent<MeshFilter>();
                colors = new List<Color>(new Color[filter.sharedMesh.vertexCount]);
            }

            protected override bool CanAddMaterialsFromRenderer(Renderer renderer, Fader[] faders)
            {
                return false;
            }

            public override bool SetAlpha(float alphaValue)
            {
                if (parent && parent.MaterialToFade)
                {
                    parent.MaterialToFade.SetFloat("_TransitionAlpha", alphaValue);
                }
                else
                {
                    for (int i = 0; i < colors.Count; i++)
                    {
                        colors[i] = new Color(alphaValue, alphaValue, alphaValue, alphaValue);
                    }

                    filter.sharedMesh.SetColors(colors);
                }

                alpha = alphaValue;

                return true;
            }
        }

        public Collider TargetPoint;
        public float TargetExtensionSize;

        public GameObject Indicator;
        public BillboardLine IndicatorLine;
        public Vector3 IndicatorOffset;
        public Color IndicatorDefaultColor;
        public float IndicatorHighlightWidth;
        private float IndicatorDefaultWidth;

        public GameObject Description;
        protected Animator descriptionAnimator;
        public string TransitionScene;

        public string HighlightSound;
        public AudioClip AirtapSound;

        public Material MaterialToFade;
        private float originalTransitionAlpha;

        protected AudioSource audioSource;

        // these are only used if there is no indicator line to determine the world position of the point of
        // interest (uses targetPosition) with scale, rotation, and offset and targetOffset to maintain the same
        // distance from that target
        private Vector3 targetPosition;
        private Vector3 targetOffset;
        protected bool initialized = false;

        private void Awake()
        {
            if (MaterialToFade)
            {
                // We don't want to change the material in the Editor, but a copy of it.
                originalTransitionAlpha = MaterialToFade.GetFloat("_TransitionAlpha");
            }
        }

        protected virtual void OnEnable()
        {
            if (!initialized)
            {
                initialized = true;
                Indicator.AddComponent<NoAutomaticFade>();

                if (Description != null)
                {
                    descriptionAnimator = Description.GetComponent<Animator>();
                }

                MeshFilter meshFilter = Indicator.GetComponentInChildren<MeshFilter>();
                if (meshFilter != null)
                {
                    meshFilter.gameObject.AddComponent<POIFader>().parent = this;
                }
                else
                {
                    Debug.LogWarning("PointOfInterest: No mesh filter object was found on the point of interest Indicator, so no fader could be added to the indicator.");
                }

                // do this before start because orbit points of interest need to override the target position (with the orbit)
                if (Indicator != null && IndicatorLine != null && IndicatorLine.points.Length < 2)
                {
                    IndicatorDefaultWidth = IndicatorLine.width;
                    IndicatorLine.points = new Transform[2];
                    IndicatorLine.points[0] = TargetPoint.gameObject.transform;
                    IndicatorLine.points[1] = gameObject.transform;
                    IndicatorLine.material.color = IndicatorDefaultColor;

                    Collider indicatorCollider = Indicator.GetComponent<Collider>();
                    if (indicatorCollider != null)
                    {
                        Vector3 IndicatorOffset = Vector3.up * (indicatorCollider.bounds.size.y / 2.0f);
                        Indicator.transform.localPosition = Indicator.transform.localPosition + IndicatorOffset;
                        Description.transform.localPosition = Description.transform.localPosition + IndicatorOffset;
                    }
                }
                else
                {
                    targetPosition = new Vector3(transform.localPosition.x, 0.0f, transform.localPosition.z);
                    targetOffset = new Vector3(0.0f, transform.localPosition.y, 0.0f) * MyAppPlatformManager.PoiMoveFactor;
                }
            }

            HideDescription();
        }

        protected virtual void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("PointOfInterest object \"" + gameObject.name + "\" is missing an AudioSource component.");
            }
        }

        protected virtual void HideDescription()
        {
            if (Description != null)
            {
                if (descriptionAnimator != null)
                {
                    descriptionAnimator.SetBool("hover", false);
                }

                if (IndicatorLine != null)
                {
                    IndicatorLine.width = IndicatorDefaultWidth;
                }
            }
        }

        protected virtual void ShowDescription()
        {
            if (Description != null)
            {
                if (descriptionAnimator != null)
                {
                    descriptionAnimator.SetBool("hover", true);
                }

                if (IndicatorLine != null)
                {
                    IndicatorLine.width = IndicatorHighlightWidth;
                }
            }
        }

        protected void LateUpdate()
        {
            // do not let the points of interest scale or rotate with the solar system
            float currentScale = Mathf.Max(gameObject.transform.lossyScale.x, gameObject.transform.lossyScale.y, gameObject.transform.lossyScale.z);
            float localScale = Mathf.Max(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            if (currentScale != 1.0f && currentScale != 0.0f && localScale != 0.0f)
            {
                float desiredScale = localScale / currentScale;
                gameObject.transform.localScale = new Vector3(desiredScale, desiredScale, desiredScale);
            }

            if (IndicatorLine != null && IndicatorLine.points != null && IndicatorLine.points.Length > 0)
            {
                gameObject.transform.position = IndicatorLine.points[0].position + IndicatorOffset;
            }
            else
            {
                Vector3 scaledTargetPosition = new Vector3(
                    gameObject.transform.parent.lossyScale.x * targetPosition.x,
                    gameObject.transform.parent.lossyScale.y * targetPosition.y,
                    gameObject.transform.parent.lossyScale.z * targetPosition.z);
                gameObject.transform.position = gameObject.transform.parent.position + (transform.parent.rotation * scaledTargetPosition) + targetOffset;
            }
        }

        private Vector3 GetWorldPositionFromOffset(Vector3 offset)
        {
            Vector3 worldPositionVerticalComponent = gameObject.transform.TransformPoint(offset.y * Vector3.up / gameObject.transform.localScale.x);

            return worldPositionVerticalComponent + (Camera.main.transform.forward * offset.z) + (Camera.main.transform.right * offset.x);
        }

        public override void OnGazeSelect()
        {
            ShowDescription();

            if (!string.IsNullOrEmpty(HighlightSound))
            {
                UAudioManager.Instance.PlayEvent(HighlightSound);
            }
        }

        public override void OnGazeDeselect()
        {
            HideDescription();
        }

        public override bool OnTapped()
        {
            if (audioSource)
            {
                audioSource.PlayOneShot(AirtapSound);
            }

            if (CardPOIManager.Instance != null)
            {
                CardPOIManager.Instance.HideAllCards();
            }

            GoToScene();
            return true;
        }

        [ContextMenu("GoToScene")]
        public void GoToScene()
        {
            if (!string.IsNullOrEmpty(TransitionScene) && TransitionManager.Instance != null)
            {
                TransitionManager.Instance.LoadNextScene(TransitionScene, gameObject);
            }
        }

        private void OnDestroy()
        {
            if (MaterialToFade)
            {
                MaterialToFade.SetFloat("_TransitionAlpha", originalTransitionAlpha);
            }
        }
    }
}