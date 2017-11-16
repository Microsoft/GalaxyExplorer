// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace GalaxyExplorer
{
    public class OrbitScalePointOfInterest : PointOfInterest
    {
        public AnimationCurve OrbitScaleHydrationCurve;
        public AnimationCurve OrbitScaleDeHydrationCurve;

        public AudioClip HydrationAudioFx;
        public AudioClip DeHydrationAudioFx;
        public AudioSource FxAudioSource;

        public GameObject AlternateDescription;

        public Texture RealIcon;
        public Texture SimplifiedIcon;

        public float SimpleViewMaxScale = 13.5f;
        public float RealisticViewMaxScale = 2350.0f;

        private bool IsReal = false;
        private bool IsAnimating = false;

        private GameObject defaultDescription;
        private MeshRenderer indicatorRenderer;

        protected override void Start()
        {
            base.Start();
            indicatorRenderer = Indicator.GetComponentInChildren<MeshRenderer>();
        }

        private IEnumerator AnimateUsingCurve(AnimationCurve curve, Action onComplete)
        {
            if (FxAudioSource && HydrationAudioFx && DeHydrationAudioFx)
            {
                FxAudioSource.PlayOneShot(IsReal ? DeHydrationAudioFx : HydrationAudioFx);
            }

            if (curve != null)
            {
                var trueScale = FindObjectOfType<TrueScaleSetting>();

                var duration = curve.keys.Last().time;
                float currentTime = 0;

                while (currentTime <= duration)
                {
                    var currentValue = curve.Evaluate(currentTime);
                    trueScale.CurrentRealismScale = currentValue;

                    currentTime += Time.deltaTime;

                    yield return null;
                }

                var lastValue = curve.Evaluate(duration);
                trueScale.CurrentRealismScale = lastValue;
            }

            if (onComplete != null)
            {
                onComplete();
            }

            IsAnimating = false;
        }

        public override bool OnTapped()
        {
            if (audioSource)
            {
                audioSource.PlayOneShot(AirtapSound);
            }

            VOManager.Instance.Stop(clearQueue: true);
            VOManager.Instance.PlayClip(VOManager.Instance.TrueScale);

            if (!IsAnimating)
            {
                indicatorRenderer.material.mainTexture = IsReal ? SimplifiedIcon : RealIcon;

                // Each view of the solar system has a different max zoom size.
                if (IsReal)
                {
                    // Set simplified view max zoom
                    ToolManager.Instance.LargestZoom = SimpleViewMaxScale;
                }
                else
                {
                    // Set realistic view max zoom
                    ToolManager.Instance.LargestZoom = RealisticViewMaxScale;
                }

                IsAnimating = true;

                TransitionManager.Instance.ResetView();

                StartCoroutine(AnimateUsingCurve(IsReal ? OrbitScaleDeHydrationCurve : OrbitScaleHydrationCurve, () => { IsReal = !IsReal; }));

                if (AlternateDescription != null)
                {
                    if (descriptionAnimator != null)
                    {
                        descriptionAnimator.SetBool("hover", false);
                    }

                    GameObject tempDescription = Description;
                    Description = AlternateDescription;
                    AlternateDescription = tempDescription;

                    descriptionAnimator = Description.GetComponent<Animator>();
                    if (descriptionAnimator != null)
                    {
                        Description.SetActive(true);
                        descriptionAnimator.SetBool("hover", true);
                    }
                }
            }

            return true;
        }
    }
}