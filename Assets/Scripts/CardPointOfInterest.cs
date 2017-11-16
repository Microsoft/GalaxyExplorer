// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;

namespace GalaxyExplorer
{
    // The card point of interest can be selected, which will animate the text
    // next to the description card. This is complicated because the point of
    // interest is a parent in the POI hierarchy to the description text and
    // fading out all of the POIs on selection would then fade out the text.
    // Also complicated is that the text needs to move with the magic window
    // that is shown when the POI is selected, and this is not parented to the
    // POI (shown when POI is hidden and is displaced). To get all of these
    // systems to work together with clean animations between states, the
    // description's parent changes.
    public class CardPointOfInterest : PointOfInterest
    {
        public Animator CardAnimator;
        public AudioClip SelectSound;
        public AudioClip DeselectSound;
        private Transform descriptionOriginalParenting; // this is the parent's parent because the description's parent (face camera object) is moved
        private Vector3 descriptionStoppedLocalPosition; // if sliding is interrupted, the description is cleared to this
        private bool isCardSelected = false;
        private PointOfInterest[] pointsOfInterest; // used to fade all of the points of interest when the card is selected

        // the description moves by the magic window but is scaled by distance when the magic window isn't, which causes the offset to change with distance;
        // this object is not scaled and is the target position for the magic window when the description is attached
        private GameObject targetOffsetObject;
        private float targetDescriptionScale;
        private Vector3 hiddenDescriptionLocalScale;

        private bool cardAnimatorVisible = false;

        private void Awake()
        {
            if (CardAnimator == null)
            {
                Debug.LogError("CardPointOfInterest: No card animator was specified for the CardPointOfInterest, '" + name +
                    "', so the component was removed (can replace with PointOfInterest if this is desired).");
                Destroy(this);
                return;
            }

            HideCard();
        }

        protected override void Start()
        {
            base.Start();

            if (Description == null)
            {
                Debug.LogError("CardPointOfInterest: No Description was set for '" + name + "'- unable to recover.");
                Destroy(this);
                return;
            }

            descriptionStoppedLocalPosition = Description.transform.localPosition;
            descriptionOriginalParenting = Description.transform.parent.parent;
            hiddenDescriptionLocalScale = Description.transform.localScale;

            Transform contentParent = transform.parent;
            while (contentParent.parent != null)
            {
                contentParent = contentParent.parent;
            }

            pointsOfInterest = contentParent.GetComponentsInChildren<PointOfInterest>(true);

            if (CardPOIManager.Instance == null)
            {
                Debug.LogWarning("CardPointOfInterest: No CardPOIManager was found, so card points of interest will not be hidden when selecting off of them.");
            }
        }

        private void OnDestroy()
        {
            if (isCardSelected && CardPOIManager.Instance != null)
            {
                CardPOIManager.Instance.CanTapCard = true;
            }
        }

        protected override void OnEnable()
        {
            if (initialized && CardAnimator != null)
            {
                CardAnimator.SetBool("CardVisible", cardAnimatorVisible);
            }

            base.OnEnable();
        }

        private void OnDisable()
        {
            if (CardAnimator != null)
            {
                cardAnimatorVisible = CardAnimator.GetBool("CardVisible");
            }
        }

        private new void LateUpdate()
        {
            base.LateUpdate();

            if (isCardSelected && targetOffsetObject != null)
            {
                Description.transform.position = targetOffsetObject.transform.position;
                Description.transform.localScale = Description.transform.localScale * targetDescriptionScale /
                    Mathf.Max(Description.transform.lossyScale.x, Description.transform.lossyScale.y, Description.transform.lossyScale.z);
            }
            else if (!isCardSelected)
            {
                Description.transform.localScale = hiddenDescriptionLocalScale;
            }
        }

        public override void OnGazeSelect()
        {
            if (!isCardSelected)
            {
                Description.transform.localPosition = descriptionStoppedLocalPosition;
                base.OnGazeSelect();
            }
        }

        public override void OnGazeDeselect()
        {
            if (!isCardSelected)
            {
                base.OnGazeDeselect();
            }
        }

        public override bool OnTapped()
        {
            // if a card is already up and this was tapped before selection could be faded out, then hide all of the cards (deselection)
            if (!CardPOIManager.Instance.CanTapCard)
            {
                CardPOIManager.Instance.HideAllCards();
            }
            else
            {
                // prevent other cards from being selected while this one is selected
                CardPOIManager.Instance.CanTapCard = false;

                if (CardAnimator)
                {
                    if (CardAnimator.GetBool("CardVisible"))
                    {
                        HideCard();
                    }
                    else
                    {
                        CardPOIManager.Instance.HideAllCards();
                        UnhideCard();
                    }
                }
            }

            return true;
        }

        public void HideCard()
        {
            if (CardAnimator && CardAnimator.GetBool("CardVisible"))
            {
                isCardSelected = false;
                CardPOIManager.Instance.CanTapCard = true;
                descriptionAnimator.SetBool("selected", false);

                if (targetOffsetObject != null)
                {
                    Destroy(targetOffsetObject);
                    targetOffsetObject = null;
                }

                CardAnimator.SetBool("CardVisible", false);

                AudioSource cardAudioSource = CardAnimator.GetComponent<AudioSource>();
                if (DeselectSound != null && cardAudioSource != null)
                {
                    VOManager.Instance.Stop(clearQueue: true);
                    cardAudioSource.PlayOneShot(DeselectSound);
                }

                // slide in the description
                StopAllCoroutines();
                Vector3 startWorldPosition = Description.transform.position;
                Description.transform.parent.SetParent(descriptionOriginalParenting, true);
                Description.transform.parent.localPosition = Vector3.zero;
                Description.transform.parent.localScale = Vector3.one;
                StartCoroutine(SlideCardIn(startWorldPosition));

                // fade in the points of interest when the card is unselected
                if (TransitionManager.Instance != null)
                {
                    foreach (PointOfInterest pointOfInterest in pointsOfInterest)
                    {
                        if (pointOfInterest != null)
                        {
                            // if faders has their coroutines killed, then need to be initialized to a disabled state
                            Fader[] faders = pointOfInterest.GetComponentsInChildren<Fader>(true);
                            foreach (Fader fader in faders)
                            {
                                fader.DisableFade();
                            }

                            if (pointOfInterest == this)
                            {
                                BillboardLine.LineFader fader = GetComponentInChildren<BillboardLine.LineFader>(true);
                                if (fader != null)
                                {
                                    StartCoroutine(TransitionManager.Instance.FadeContent(
                                        fader.gameObject,
                                        TransitionManager.FadeType.FadeIn,
                                        CardPOIManager.Instance.POIFadeOutTime,
                                        CardPOIManager.Instance.POIOpacityCurve));
                                }
                            }
                            else
                            {
                                StartCoroutine(TransitionManager.Instance.FadeContent(
                                    pointOfInterest.gameObject,
                                    TransitionManager.FadeType.FadeIn,
                                    CardPOIManager.Instance.POIFadeOutTime,
                                    CardPOIManager.Instance.POIOpacityCurve));
                            }
                        }
                    }
                }

                // we can hide the text again
                GazeSelectionTarget selectionTarget = GazeSelectionManager.Instance.SelectedTarget;
                if (selectionTarget != this || // same selection target
                    (selectionTarget != null && selectionTarget is PointOfInterestReference && (selectionTarget as PointOfInterestReference).pointOfInterest != this)) // same target hidden by a reference
                {
                    OnGazeDeselect();
                }
            }
        }

        public void UnhideCard()
        {
            if (CardAnimator && !CardAnimator.GetBool("CardVisible"))
            {
                isCardSelected = true;
                CardAnimator.SetBool("CardVisible", true);
                descriptionAnimator.SetBool("selected", true);

                // update the position of the image and face the camera
                CardAnimator.transform.parent.position = IndicatorLine.points[0].position;
                if (Camera.main != null)
                {
                    Vector3 forwardDirection = transform.position - Camera.main.transform.position;
                    CardAnimator.transform.parent.rotation = Quaternion.LookRotation(forwardDirection.normalized, Camera.main.transform.up);
                }

                AudioSource cardAudioSource = CardAnimator.GetComponent<AudioSource>();
                if (AirtapSound != null && cardAudioSource != null)
                {
                    VOManager.Instance.Stop(clearQueue: true);
                    VOManager.Instance.PlayClip(AirtapSound);
                    cardAudioSource.PlayOneShot(SelectSound);
                }

                // slide out the description
                StopAllCoroutines();
                Description.transform.localPosition = descriptionStoppedLocalPosition;
                targetDescriptionScale = Mathf.Max(Description.transform.lossyScale.x, Description.transform.lossyScale.y, Description.transform.lossyScale.z);
                ScaleWithDistance distanceScaler = CardAnimator.transform.parent.GetComponentInChildren<ScaleWithDistance>();
                Description.transform.parent.SetParent(distanceScaler.transform, true);
                targetOffsetObject = new GameObject("DescriptionOffset");
                targetOffsetObject.transform.SetParent(CardAnimator.transform.parent);

                StartCoroutine(SlideCardOut());

                // fade out the points of interest when the card is selected
                if (TransitionManager.Instance != null)
                {
                    foreach (PointOfInterest pointOfInterest in pointsOfInterest)
                    {
                        if (pointOfInterest != null)
                        {
                            // if faders has their coroutines killed, then need to be initialized to a disabled state
                            Fader[] faders = pointOfInterest.GetComponentsInChildren<Fader>(true);
                            foreach (Fader fader in faders)
                            {
                                fader.DisableFade();
                            }

                            if (pointOfInterest == this)
                            {
                                BillboardLine.LineFader fader = GetComponentInChildren<BillboardLine.LineFader>(true);
                                if (fader != null)
                                {
                                    StartCoroutine(TransitionManager.Instance.FadeContent(
                                        fader.gameObject,
                                        TransitionManager.FadeType.FadeOut,
                                        CardPOIManager.Instance.POIFadeOutTime,
                                        CardPOIManager.Instance.POIOpacityCurve));
                                }
                            }
                            else
                            {
                                StartCoroutine(TransitionManager.Instance.FadeContent(
                                    pointOfInterest.gameObject,
                                    TransitionManager.FadeType.FadeOut,
                                    CardPOIManager.Instance.POIFadeOutTime,
                                    CardPOIManager.Instance.POIOpacityCurve));
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator SlideCardOut()
        {
            if (Camera.main == null)
            {
                Debug.LogError("CardPointOfInterest: There is no main camera present, to the card description cannot slide out with the hydration of the card magic window.");
                yield break;
            }

            float time = 0.0f;
            targetOffsetObject.transform.position = Description.transform.position;
            Vector3 startPosition = targetOffsetObject.transform.localPosition;
            Vector3 endPosition = targetOffsetObject.transform.localPosition +
                (CardPOIManager.Instance.DescriptionSlideDirection *
                    MyAppPlatformManager.MagicWindowScaleFactor / 2.0f);

            do
            {
                time += Time.deltaTime;

                float timeFraction = Mathf.Clamp01(time / CardPOIManager.Instance.DescriptionSlideOutTime);
                float tValue = CardPOIManager.Instance.DescriptionSlideCurve.Evaluate(timeFraction);
                targetOffsetObject.transform.localPosition = Vector3.Lerp(startPosition, endPosition, tValue);

                yield return null;
            }
            while (time < CardPOIManager.Instance.DescriptionSlideOutTime);
        }

        private IEnumerator SlideCardIn(Vector3 startWorldPosition)
        {
            float time = 0.0f;

            Description.transform.parent.localPosition = Vector3.zero;
            Description.transform.position = startWorldPosition;
            Vector3 startLocalPosition = Description.transform.localPosition;

            do
            {
                time += Time.deltaTime;

                float timeFraction = Mathf.Clamp01(time / CardPOIManager.Instance.DescriptionSlideInTime);
                float tValue = CardPOIManager.Instance.DescriptionSlideCurve.Evaluate(timeFraction);
                Description.transform.localPosition = Vector3.Lerp(startLocalPosition, descriptionStoppedLocalPosition, tValue);

                yield return null;
            }
            while (time < CardPOIManager.Instance.DescriptionSlideInTime);

            Description.transform.localPosition = descriptionStoppedLocalPosition;
        }
    }
}