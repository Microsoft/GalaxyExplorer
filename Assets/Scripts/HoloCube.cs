// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class HoloCube : SharedMaterialFader
    {
        private int fadingRefCount = 0;

        public void SetActve(bool isActive)
        {
            for (int childIndex = 0; childIndex < transform.childCount; ++childIndex)
            {
                transform.GetChild(childIndex).gameObject.SetActive(isActive);
            }
        }

        public void EnableSkybox(float timeOffset = 0.0f)
        {
            ++fadingRefCount;
            StartCoroutine(EnableSkyBoxCR(timeOffset));
        }

        private IEnumerator EnableSkyBoxCR(float timeOffset)
        {
            WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

            float time = 0.0f;
            while (time < timeOffset)
            {
                time += Time.deltaTime;
                yield return endOfFrame;
            }

            SetActve(true);

            StartCoroutine(TransitionManager.Instance.FadeContent(
                gameObject,
                TransitionManager.FadeType.FadeIn,
                TransitionManager.Instance.TransitionTimeSkyboxFadeIn,
                TransitionManager.Instance.OpacityCurveSkyboxFadeIn));

            --fadingRefCount;
        }

        public void DisableSkybox(float timeOffset = 0.0f)
        {
            ++fadingRefCount;
            StartCoroutine(DisableSkyBoxCR(timeOffset));
        }

        private IEnumerator DisableSkyBoxCR(float timeOffset)
        {
            WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

            float time = 0.0f;
            while (time < timeOffset)
            {
                time += Time.deltaTime;
                yield return endOfFrame;
            }

            SetActve(true);

            StartCoroutine(TransitionManager.Instance.FadeContent(
                gameObject,
                TransitionManager.FadeType.FadeOut,
                TransitionManager.Instance.TransitionTimeSkyboxFadeOut,
                TransitionManager.Instance.OpacityCurveSkyboxFadeOut));

            time = 0.0f;
            while (time < TransitionManager.Instance.TransitionTimeSkyboxFadeOut)
            {
                time += Time.deltaTime;
                yield return endOfFrame;
            }

            // if there is not a fade in our future, we can deactivate the game object; otherwise, fades
            // will not work because Update() is not called on inactive objects
            if (--fadingRefCount == 0)
            {
                SetActve(false);
            }
        }
    }
}