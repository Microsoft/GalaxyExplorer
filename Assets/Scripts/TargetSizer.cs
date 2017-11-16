// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class TargetSizer : MonoBehaviour
    {
        [Tooltip("A representative collider that a loaded scene fills to its entirety.")]
        public Collider TargetFillCollider;

        private PointOfInterest interestPoint;

        private void Start()
        {
            interestPoint = gameObject.GetComponent<PointOfInterest>();
            if (interestPoint == null)
            {
                Debug.LogError("TargetSizer: There is no point of interest for the TargetSizer on '" + gameObject.name + "'");
                Destroy(gameObject);
                return;
            }
        }

        public void FitToScene(SceneSizer sceneSizer)
        {
            Vector3 targetExtents = new Vector3(0.001f, 0.001f, 0.001f);
            Vector3 targetPosition = sceneSizer.gameObject.transform.position;
            Quaternion targetRotation = sceneSizer.gameObject.transform.rotation;

            if (TargetFillCollider != null)
            {
                targetExtents = TargetFillCollider.bounds.extents;
                targetPosition = TargetFillCollider.transform.position;
                targetRotation = transform.rotation;
            }

            gameObject.transform.position = targetPosition;
            gameObject.transform.rotation = targetRotation;
            gameObject.transform.localScale = gameObject.transform.localScale *
                Mathf.Max(TargetFillCollider.bounds.extents.x, TargetFillCollider.bounds.extents.y, TargetFillCollider.bounds.extents.z) /
                Mathf.Max(targetExtents.x, targetExtents.y, targetExtents.z);
        }

        public float GetScalar()
        {
            float size = 0.001f;

            if (TargetFillCollider != null)
            {
                size = Mathf.Max(TargetFillCollider.bounds.size.x, TargetFillCollider.bounds.size.y, TargetFillCollider.bounds.size.z);
            }

            return size;
        }

        public Vector3 GetPosition(float scaleOffset = 1.0f)
        {
            if (TargetFillCollider != null)
            {
                return TargetFillCollider.transform.position;
            }

            Vector3 position = transform.position;

            if (interestPoint != null)
            {
                position -= interestPoint.IndicatorOffset * scaleOffset;
            }

            return position;
        }
    }
}