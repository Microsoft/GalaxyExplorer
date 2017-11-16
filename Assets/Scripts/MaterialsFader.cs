// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class MaterialsFader : Fader
    {
        public Material[] materials;

        private MaterialSettings[] settings;

        private void Start()
        {
            settings = new MaterialSettings[materials.Length];

            for (int materialIndex = 0; materialIndex < materials.Length; ++materialIndex)
            {
                Material material = materials[materialIndex];
                settings[materialIndex].material = material;

                if (material != null)
                {
                    settings[materialIndex].originalSourceBlend = material.HasProperty("_SRCBLEND") ? material.GetInt("_SRCBLEND") : -1;
                    settings[materialIndex].originalDestinationBlend = material.HasProperty("_DSTBLEND") ? material.GetInt("_DSTBLEND") : -1;
                }
                else
                {
                    Debug.LogWarning("MaterialsFader: Material #" + materialIndex + " of '" + name + "' is missing - delete from list.");
                }
            }
        }

        private void OnDestroy()
        {
            // since the material is shared our settings will persist; loaded scenes should have full transition alpha
            for (int materialIndex = 0; materialIndex < materials.Length; ++materialIndex)
            {
                if (materials[materialIndex] != null && settings != null)
                {
                    MaterialSettings matset = settings[materialIndex];
                    if (matset.material != null)
                    {
                        matset.material.SetFloat("_TransitionAlpha", 1.0f);

                        if (matset.originalSourceBlend != -1)
                        {
                            matset.material.SetInt("_SRCBLEND", matset.originalSourceBlend);
                        }

                        if (matset.originalDestinationBlend != -1)
                        {
                            matset.material.SetInt("_DSTBLEND", matset.originalDestinationBlend);
                        }
                    }
                }
            }
        }

        protected override MaterialSettings[] Materials
        {
            get { return settings ?? new MaterialSettings[0]; }
        }
    }
}