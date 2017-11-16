// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    /// <summary>
    /// Orbits the main camera with given orbit scale and camera position offsets
    /// </summary>
    public class CameraOrbit : MonoBehaviour
    {
        public float Speed = 5.0f;
        public Vector3 cameraOffset = Vector3.zero;
        public Vector2 orbitScale = Vector2.one;

        // Radians
        private float currentAngle = 0.0f;
        private Transform cameraTransform;

        private void Start()
        {
            cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            Vector3 newPos = cameraTransform.position + cameraOffset;
            newPos += new Vector3(Mathf.Sin(currentAngle) * orbitScale.x, 0, Mathf.Cos(currentAngle) * orbitScale.y);

            transform.position = newPos;

            currentAngle += Time.deltaTime * Speed;
        }
    }
}