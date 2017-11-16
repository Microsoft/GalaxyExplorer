// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class ConstantFaceDirection : FaceCamera
    {
        private Vector3 faceDirection;

        private void OnEnable()
        {
            if (Camera.main != null)
            {
                faceDirection = transform.position - Camera.main.transform.position;
            }
        }

        // this needs to happen after all positions have been updated
        protected override void LateUpdate()
        {
            if (Camera.main != null)
            {
                FaceDirection(faceDirection);
            }
        }
    }
}