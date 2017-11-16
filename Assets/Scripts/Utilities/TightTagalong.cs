// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using UnityEngine;
using System.Collections;

namespace GalaxyExplorer
{
    [RequireComponent(typeof(Interpolator))]
    public class TightTagalong : MonoBehaviour
    {
        // Distance between this component and camera/head
        public float distanceToHead = 1.2f;
        [Tooltip("If less than from 'Distance to Head', the SR mesh will prevent the object from getting placed beyond the SR mesh up to this distance from the viewer.")]
        public float minCollisionDistanceToHead = 1.2f;
        [Tooltip("If specified, the object will attach to the world until it is closer than 'Min Collision Distance to Head'.")]
        public LayerMask collisionPhysicsLayer;

        // Local offset to apply
        public Vector3 offset = Vector3.zero;
        public Vector3 rotationOffset = Vector3.zero;

        // Whether or not to flatten the rotation (yaw only)
        public bool flattenRotation = false;

        // Doesn't use interpolation
        public bool hardLock = false;

        public Interpolator interpolator { get; private set; }

        public bool initialSnapToTarget = true;
        private bool snapToTarget;

        private Transform head;

        [HideInInspector]
        public bool FollowMotionControllerIfAvailable = false;

        private void Awake()
        {
            interpolator = GetComponent<Interpolator>();
        }

        private void OnEnable()
        {
            // Snap to target is wrapped with this public variable logic
            // because it's set to false during the update transform function.
            if (initialSnapToTarget)
            {
                snapToTarget = true;
            }

            // coroutines are ended when the component is disabled, so look for
            // the head transform on enable
            if (head == null)
            {
                StartCoroutine(FindHeadTransform());
            }
        }

        private void Update()
        {
            if (FollowMotionControllerIfAvailable &&
                MotionControllerInput.Instance &&
                MotionControllerInput.Instance.UseAlternateGazeRay)
            {
                interpolator.enabled = false;
                SetTransform();
            }
            else if (head)
            {
                if (hardLock)
                {
                    interpolator.enabled = false;
                    SetTransform();
                }
                else
                {
                    interpolator.enabled = true;
                    UpdateTransform();
                }
            }
        }

        private IEnumerator FindHeadTransform()
        {
            while (!Camera.main)
            {
                yield return null;
            }

            // Reference to the StereoCamera head for easy access during Update()
            head = Camera.main.transform;
        }

        public void SetTransform()
        {
            if (FollowMotionControllerIfAvailable &&
                MotionControllerInput.Instance &&
                MotionControllerInput.Instance.UseAlternateGazeRay &&
                MotionControllerInput.Instance.AlternateGazeRayControlerInformation != null)
            {
                var ci = MotionControllerInput.Instance.AlternateGazeRayControlerInformation;
                ci.accumulatedY = Mathf.Clamp(ci.accumulatedY, -distanceToHead, Cursor.Instance.defaultCursorDistance * 5f);
                var distance = distanceToHead + (ci.accumulatedY / 10f);

                Ray ray = MotionControllerInput.Instance.AlternateGazeRay;
                transform.position = ray.origin + (ray.direction * distance);

                Vector3 look = ray.origin - transform.position;
                Quaternion rotation = Quaternion.LookRotation(look, Vector3.up);
                transform.rotation = Quaternion.Euler(rotation.eulerAngles) * Quaternion.Euler(rotationOffset);
            }
            else
            {
                transform.position = head.position + (head.forward * distanceToHead);
                transform.rotation = Quaternion.Euler(head.rotation.eulerAngles) * Quaternion.Euler(rotationOffset);
            }
            transform.localPosition += offset;
        }

        public void UpdateTransform()
        {
            Vector3 cameraPosition = head.position;
            Quaternion cameraRotation = head.rotation;
            Vector3 cameraForward = cameraRotation * Vector3.forward;

            Vector3 targetPosition = cameraPosition + (cameraForward * distanceToHead) + (cameraRotation * offset);
            Vector3 targetDirection = (targetPosition - cameraPosition).normalized;

            RaycastHit rayCastHit;

            // Do a initial simple ray cast against the world and adjust the
            // target position if it hits something
            if (collisionPhysicsLayer != 0 && Physics.Raycast(cameraPosition, targetDirection, out rayCastHit, distanceToHead, collisionPhysicsLayer))
            {
                // the collision point is too close, so push it back min
                // distance away along the target direction
                if ((cameraPosition - rayCastHit.point).magnitude < minCollisionDistanceToHead)
                {
                    targetPosition = cameraPosition + (targetDirection * minCollisionDistanceToHead);
                }
                else
                {
                    targetPosition = rayCastHit.point;
                }
            }
            else
            {
                targetPosition += cameraRotation * offset;
            }

            // Determine the final direction and flatten if needed
            Vector3 targetToCameraDirection = cameraPosition - targetPosition;
            if (flattenRotation)
            {
                targetToCameraDirection.y = 0.0f;
            }

            Quaternion targetRotation = Quaternion.LookRotation(targetToCameraDirection.normalized, GetSafeUp(targetToCameraDirection.normalized)) * Quaternion.Euler(rotationOffset);

            interpolator.SetTargetPosition(targetPosition);
            interpolator.SetTargetRotation(targetRotation);

            if (snapToTarget)
            {
                snapToTarget = false;
                interpolator.SnapToTarget();
            }
        }

        private Vector3 GetSafeUp(Vector3 hitNormal)
        {
            return Mathf.Abs(Vector3.Dot(Vector3.up, hitNormal)) > 0.99f ? Vector3.right : Vector3.up;
        }
    }
}