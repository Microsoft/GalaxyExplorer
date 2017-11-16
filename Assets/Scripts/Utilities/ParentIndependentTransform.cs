// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class ParentIndependentTransform : MonoBehaviour
    {
        protected void LateUpdate()
        {
            // do not let the points of interest scale or rotate with the solar system
            float currentScale = Mathf.Max(gameObject.transform.lossyScale.x, gameObject.transform.lossyScale.y, gameObject.transform.lossyScale.z);
            float localScale = Mathf.Max(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            if (currentScale != 1.0f && currentScale != 0.0f && localScale != 0.0f)
            {
                float desiredScale = localScale / currentScale;
                gameObject.transform.localScale = new Vector3(desiredScale, desiredScale, desiredScale) *
                    MyAppPlatformManager.MagicWindowScaleFactor;
            }
        }
    }
}