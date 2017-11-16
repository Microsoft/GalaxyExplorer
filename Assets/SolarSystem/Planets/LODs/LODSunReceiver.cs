// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class LODSunReceiver : MonoBehaviour
    {
        public Transform Sun;
        public Material planetLod;
        private Vector4 originalSunPosition;

        private void Awake()
        {
            originalSunPosition = planetLod.GetVector("_SunPosition");
        }

        private void Update()
        {
            if (Sun)
            {
                planetLod.SetVector("_SunPosition", Sun.position);
            }
        }

        private void OnDestroy()
        {
            planetLod.SetVector("_SunPosition", originalSunPosition);
        }
    }
}