// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class ConstantRotateAxis : MonoBehaviour
    {
        public Vector3 axis;
        public float speed;

        private void Update()
        {
            if (axis.magnitude > 0)
            {
                transform.localRotation *= Quaternion.AngleAxis(speed * Time.deltaTime, axis.normalized);
            }
        }
    }
}