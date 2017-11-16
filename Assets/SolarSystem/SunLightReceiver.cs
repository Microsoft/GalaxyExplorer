// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class SunLightReceiver : MonoBehaviour
    {
        public Transform Sun;
        private MeshRenderer currentRenderer;
        private Material moonMaterial = null;

        private void Awake()
        {
            FindSunIfNeeded();

            currentRenderer = gameObject.GetComponent<MeshRenderer>();
            if (currentRenderer.sharedMaterial.name.Equals("Moon"))
            {
                // Somewhere, the Moon's currentRenderer.sharedMaterial is getting
                // changed. Cache the original material here so we can properly
                // clean it up later in OnDestroy.
                moonMaterial = currentRenderer.sharedMaterial;
            }
        }

        public bool FindSunIfNeeded()
        {
            if (!Sun)
            {
                var sunGo = GameObject.Find("Sun");
                if (sunGo)
                {
                    Sun = sunGo.transform;
                    return true;
                }
            }

            return Sun;
        }

        private void LateUpdate()
        {
            if (FindSunIfNeeded())
            {
                Vector3 dir = (Sun.position - transform.position).normalized;
                currentRenderer.sharedMaterial.SetVector("_SunDirection", new Vector4(dir.x, dir.y, dir.z, 0));
            }
        }

        private void OnDestroy()
        {
            if (currentRenderer.sharedMaterial.HasProperty("_SunDirection") || moonMaterial)
            {
                if (moonMaterial)
                {
                    moonMaterial.SetVector("_SunDirection", Vector4.zero);
                }
                else
                {
                    currentRenderer.sharedMaterial.SetVector("_SunDirection", Vector4.zero);
                }
            }
        }
    }
}