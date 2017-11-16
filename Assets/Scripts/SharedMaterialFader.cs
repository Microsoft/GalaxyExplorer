// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class SharedMaterialFader : Fader
    {
        private MaterialSettings[] SharedMaterial;

        private void Start()
        {
            SharedMaterial = new MaterialSettings[1];

            Fader[] childrenFaders = GetComponentsInChildren<Fader>(true);
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (CanAddMaterialsFromRenderer(renderer, childrenFaders))
                {
                    if (SharedMaterial[0].material == null)
                    {
                        SharedMaterial[0].material = renderer.material;
                        SharedMaterial[0].originalSourceBlend = renderer.material.HasProperty("_SRCBLEND") ? renderer.material.GetInt("_SRCBLEND") : -1;
                        SharedMaterial[0].originalDestinationBlend = renderer.material.HasProperty("_DSTBLEND") ? renderer.material.GetInt("_DSTBLEND") : -1;
                    }

                    renderer.material = SharedMaterial[0].material;
                }
            }
        }

        private void OnDestroy()
        {
            // since the material is shared our settings will persist; loaded scenes should have full transition alpha
            if (SharedMaterial != null && SharedMaterial[0].material != null)
            {
                SharedMaterial[0].material.SetFloat("_TransitionAlpha", 1.0f);
            }
        }

        protected override MaterialSettings[] Materials
        {
            get { return SharedMaterial; }
        }
    }
}