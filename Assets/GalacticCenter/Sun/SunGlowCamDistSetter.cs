// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GalaxyExplorer
{
    using UnityEngine;

    public class SunGlowCamDistSetterGC : MonoBehaviour
    {
        public float DistanceFromCamera = 0;

        public AnimationCurve ColorADistanceFromDistance;
        public AnimationCurve ColorASmoothnessFromDistance;

        private Material mat;
        private Vector4 colorAParams;

        private void Start()
        {
            var r = gameObject.GetComponent<MeshRenderer>();
            r.material = mat = r.material;
        }

        [ContextMenu("SetDistance")]
        private void UpdateDistance()
        {
            DistanceFromCamera = (Camera.main.transform.position - transform.position).magnitude;
        }

        private void Update()
        {
            if (Camera.main != null)
            {
                DistanceFromCamera = (Camera.main.transform.position - transform.position).magnitude;

                float distanceA = ColorADistanceFromDistance.Evaluate(DistanceFromCamera);
                float smoothnessA = 1 / ColorASmoothnessFromDistance.Evaluate(DistanceFromCamera);

                colorAParams = mat.GetVector("_ColorAParams");
                colorAParams.x = distanceA;
                colorAParams.y = smoothnessA;
                mat.SetVector("_ColorAParams", colorAParams);
            }
        }
    }
}