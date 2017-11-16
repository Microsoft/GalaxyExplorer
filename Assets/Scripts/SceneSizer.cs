// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class SceneSizer : MonoBehaviour
    {
        [Tooltip("The percentage of space within a boundary that the target collider will fill.")]
        public float FullScreenFillPercentage = 0.75f;
        [Tooltip("A representative collider that fills the entirety of the scene.")]
        public Collider TargetFillCollider;

        private Vector3 defaultSize;

        public void Awake()
        {
            if (TargetFillCollider == null)
            {
                Debug.LogError("SceneSizer: The scene, '" + gameObject.scene.name + "', was loaded with no specified TargetFillCollider - no way to know how big the content should scale.");
                Destroy(this);
                return;
            }

            // if the scene starts out hidden, the collider bounds size may not be calculated, so search for the renderer and use that instead
            defaultSize = TargetFillCollider.bounds.size;
            if (defaultSize == Vector3.zero)
            {
                Renderer targetRenderer = TargetFillCollider.gameObject.GetComponent<Renderer>();

                if (targetRenderer != null)
                {
                    defaultSize = targetRenderer.bounds.size;
                }
            }

            // if there is no transition manager, the scene is running by itself (not from main)
            if (TransitionManager.Instance == null)
            {
                // force the child object active in case the scene starts out hidden
                gameObject.transform.GetChild(0).gameObject.SetActive(true);
            }
        }

        public void Start()
        {
            // the scene was loaded as part of the galaxy explorer flow- notify the transition manager that new content was loaded
            if (TransitionManager.Instance != null)
            {
                // the scene was loaded as part of the galaxy explorer flow- notify the transition manager that new content was loaded
                TransitionManager.Instance.InitializeLoadedContent(gameObject);
            }
        }

        public void FitToTarget(TargetSizer targetSizer, bool useCollider = false)
        {
            PointOfInterest interestPoint = targetSizer.GetComponent<PointOfInterest>();
            Vector3 targetSize = new Vector3(0.001f, 0.001f, 0.001f);
            Vector3 targetPosition = targetSizer.gameObject.transform.position;
            Quaternion targetRotation = TransitionManager.Instance.ViewVolume.transform.rotation;

            if (targetSizer.TargetFillCollider != null)
            {
                targetSize = useCollider
                    ? targetSizer.TargetFillCollider.bounds.size
                    : targetSizer.TargetFillCollider.transform.lossyScale;
                targetPosition = targetSizer.TargetFillCollider.transform.parent.position;
                targetRotation = targetSizer.TargetFillCollider.transform.parent.rotation;
            }
            else if (interestPoint != null)
            {
                targetPosition -= interestPoint.IndicatorOffset;
            }

            gameObject.transform.position = targetPosition;
            gameObject.transform.rotation = targetRotation;

            float parentScale = useCollider
                ? Mathf.Max(TargetFillCollider.bounds.size.x, TargetFillCollider.bounds.size.y, TargetFillCollider.bounds.size.z) /
                    Mathf.Max(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z)
                : Mathf.Max(TargetFillCollider.transform.lossyScale.x, TargetFillCollider.transform.lossyScale.y, TargetFillCollider.transform.lossyScale.z) /
                    Mathf.Max(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            gameObject.transform.localScale = targetSize / parentScale;
        }

        public float GetScalar(float targetSize)
        {
            return targetSize * FullScreenFillPercentage / Mathf.Max(defaultSize.x, defaultSize.y, defaultSize.z);
        }

        public float GetScalar()
        {
            return Mathf.Max(transform.lossyScale.x * defaultSize.x, transform.lossyScale.y * defaultSize.y, transform.lossyScale.z * defaultSize.z);
        }
    }
}