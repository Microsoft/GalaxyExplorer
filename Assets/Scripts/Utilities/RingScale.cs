// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class RingScale : MonoBehaviour
    {
        public float MinScale = 1;
        public float MaxScale = 1;

        public float ObjectSpaceRadius = 1;

        private void Update()
        {
            if (Camera.main)
            {
                var absRadius = ObjectSpaceRadius * transform.parent.lossyScale.x;

                if (Mathf.Abs(absRadius) <= float.Epsilon)
                {
                    return;
                }

                var centerToCam = Camera.main.transform.position - transform.position;

                var angleAmount = Mathf.Atan2(centerToCam.magnitude, absRadius);
                var normalizedAmount = Mathf.Abs(angleAmount / (Mathf.PI * .5f));

                var desiredScale = Mathf.Lerp(MinScale, MaxScale, normalizedAmount);
                transform.localScale = new Vector3(desiredScale, desiredScale, desiredScale);
            }
        }
    }
}