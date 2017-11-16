// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class SunLensFlareSetterGC : Fader
    {
        public Material flaresMaterial;

        private MaterialSettings[] materialsWrapper;

        protected override MaterialSettings[] Materials
        {
            get
            {
                if (materialsWrapper == null || materialsWrapper.Length < 1)
                {
                    materialsWrapper = new MaterialSettings[1];
                }

                materialsWrapper[0].material = flaresMaterial;
                materialsWrapper[0].originalSourceBlend = flaresMaterial.HasProperty("_SRCBLEND") ? flaresMaterial.GetInt("_SRCBLEND") : -1;
                materialsWrapper[0].originalDestinationBlend = flaresMaterial.HasProperty("_DSTBLEND") ? flaresMaterial.GetInt("_DSTBLEND") : -1;

                return materialsWrapper;
            }
        }

        private void OnDestroy()
        {
            // since the material is shared our settings will persist; loaded scenes should have full transition alpha
            if (flaresMaterial != null)
            {
                flaresMaterial.SetFloat("_TransitionAlpha", 1.0f);
            }
        }

        protected override bool CanAddMaterialsFromRenderer(Renderer renderer, Fader[] faders)
        {
            return false;
        }

        private void Awake()
        {
            if (!flaresMaterial)
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            if (flaresMaterial && Camera.main)
            {
                flaresMaterial.SetFloat("_DistFromCamera", (Camera.main.transform.position - transform.position).magnitude);
                flaresMaterial.SetFloat("_CurrentScale", transform.lossyScale.x);
            }
        }
    }
}