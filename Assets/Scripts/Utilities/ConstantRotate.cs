// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class ConstantRotate : MonoBehaviour
    {
        public Vector3 rotateSpeed;

        public bool RandomRotationSpeed = false;
        public bool RandomRotationDirection = false;
        public Vector3 RotationSpeedMin = Vector3.zero;
        public Vector3 RotationSpeedMax = Vector3.zero;

        public bool RandomStartOrientation = false;
        public Vector3 StartOrientationMin = Vector3.zero;
        public Vector3 StartOrientationMax = Vector3.zero;

        private void Start()
        {
            if (RandomStartOrientation)
            {
                float x = Random.Range(StartOrientationMin.x, StartOrientationMax.x);
                float y = Random.Range(StartOrientationMin.y, StartOrientationMax.y);
                float z = Random.Range(StartOrientationMin.z, StartOrientationMax.z);

                transform.localRotation = Quaternion.Euler(x, y, z);
            }

            if (RandomRotationSpeed)
            {
                float x = Random.Range(RotationSpeedMin.x, RotationSpeedMin.x);
                float y = Random.Range(RotationSpeedMin.y, RotationSpeedMin.y);
                float z = Random.Range(RotationSpeedMin.z, RotationSpeedMin.z);

                rotateSpeed = new Vector3(x, y, z);
            }

            if (RandomRotationDirection)
            {
                if (Random.value > 0.5f)
                {
                    rotateSpeed = -rotateSpeed;
                }
            }
        }

        private void Update()
        {
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + (rotateSpeed * Time.deltaTime));
        }
    }
}