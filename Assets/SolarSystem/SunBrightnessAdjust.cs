// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class SunBrightnessAdjust : MonoBehaviour
    {
        [ColorUsage(true, true, 0, 100, 0, 100)]
        public Color NormalFresnel;
        [ColorUsage(true, true, 0, 100, 0, 100)]
        public Color BrightFresnel;

        public float WhenVisibleAdjustmentSpeed = 1;
        public float WhenNotVisibleAdjustmentSpeed = 1;

        private Material sunMaterial;
        private float currentLevel = 0;
        private MeshRenderer sunRenderer;

        private void Start()
        {
            sunRenderer = GetComponent<MeshRenderer>();
            sunRenderer.material = sunMaterial = Instantiate(sunRenderer.sharedMaterial);
        }

        private void Update()
        {
            if (!sunRenderer)
            {
                return;
            }

            currentLevel = Mathf.Lerp(currentLevel, sunRenderer.isVisible ? 0 : 1, Time.deltaTime * (sunRenderer.isVisible ? WhenVisibleAdjustmentSpeed : WhenNotVisibleAdjustmentSpeed));

            sunMaterial.SetColor("_FresnelColor", Color.Lerp(NormalFresnel, BrightFresnel, currentLevel));
        }
    }
}