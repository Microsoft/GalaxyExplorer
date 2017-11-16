// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class SaturnVisual : MonoBehaviour
    {
        public float OuterRingRadius = 1;
        private float originalOuterRingRadius;
        public float InnerRingRadius = .7f;
        private float originalInnerRingRadius;
        public float GlobalScale = 1;

        // We get the scale from the root of the view
        private ContentView contentRoot;

        private MeshRenderer contentRenderer;

        private void Awake()
        {
            contentRenderer = GetComponent<MeshRenderer>();
            if (!contentRenderer)
            {
                Destroy(this);
            }
            else
            {
                originalInnerRingRadius = contentRenderer.sharedMaterial.GetFloat("_InnerRingRadius");
                originalOuterRingRadius = contentRenderer.sharedMaterial.GetFloat("_OuterRingRadius");
            }

            contentRoot = GetComponentInParent<ContentView>();
        }

        private void LateUpdate()
        {
            var ringsScale = contentRoot.transform.lossyScale.x * GlobalScale;
            var contentMaterial = contentRenderer.sharedMaterial;

            contentMaterial.SetFloat("_OuterRingRadius", OuterRingRadius * ringsScale);
            contentMaterial.SetFloat("_InnerRingRadius", InnerRingRadius * ringsScale);

            contentMaterial.SetVector("_PlanetUp", transform.parent.up);
            contentMaterial.SetVector("_PlanetRight", transform.parent.up);
            contentMaterial.SetVector("_PlanetCenter", transform.position);
        }

        private void OnDestroy()
        {
            if (contentRenderer)
            {
                contentRenderer.sharedMaterial.SetFloat("_InnerRingRadius", originalInnerRingRadius);
                contentRenderer.sharedMaterial.SetFloat("_OuterRingRadius", originalOuterRingRadius);
            }
        }
    }
}