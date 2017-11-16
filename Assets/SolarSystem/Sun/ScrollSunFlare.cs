// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;

namespace GalaxyExplorer
{
    public class ScrollSunFlare : MonoBehaviour
    {
        public Vector2 InitialOffset = Vector2.zero;
        public Vector2 FinalOffset = Vector2.zero;

        public float InitialScale = 1;
        public float FinalScale = 1;

        private Vector2 FlareTimeRange = new Vector2(8f, 15f);
        private Vector2 TimeRangeBetweenFlares = new Vector2(0f, 5f);

        private Material mat;
        private Vector2 currentOffset;

        private void OnEnable()
        {
            var mr = gameObject.GetComponent<MeshRenderer>();
            mr.material = mat = mr.material;

            StartCoroutine(Flares(new Vector2(0, 1)));
        }

        private void OnDisable()
        {
            this.StopAllCoroutines();
        }

        private IEnumerator Flares(Vector2 initialTimeRange)
        {
            SetUpForFlare();
            float startTimeToWait = Random.Range(initialTimeRange.x, initialTimeRange.y);
            yield return new WaitForSeconds(startTimeToWait);

            while (true)
            {
                SetUpForFlare();
                float timeToWait = Random.Range(TimeRangeBetweenFlares.x, TimeRangeBetweenFlares.y);
                yield return new WaitForSeconds(timeToWait);

                float FlareTime = Random.Range(FlareTimeRange.x, FlareTimeRange.y);
                yield return StartCoroutine(DoSunFlare(FlareTime));
            }
        }

        private void SetUpForFlare()
        {
            if (mat != null)
            {
                mat.SetTextureOffset("_MainTex", InitialOffset);
            }
        }

        private IEnumerator DoSunFlare(float FlareTime)
        {
            if (FlareTime != 0)
            {
                float t = 0;

                while (t < FlareTime * 0.5f)
                {
                    float i = t / FlareTime;

                    Vector2 newOffset = Vector2.Lerp(InitialOffset, FinalOffset, i);
                    mat.SetTextureOffset("_MainTex", newOffset);

                    float newScale = Mathf.Lerp(InitialScale, FinalScale, Mathf.SmoothStep(0, 1, i * 2f));
                    transform.localScale = Vector3.one * newScale;

                    t += Time.deltaTime;
                    yield return null;
                }

                while (t < FlareTime)
                {
                    float i = t / FlareTime;

                    Vector2 newOffset = Vector2.Lerp(InitialOffset, FinalOffset, i);
                    mat.SetTextureOffset("_MainTex", newOffset);

                    float newScale = Mathf.Lerp(FinalScale, InitialScale, Mathf.SmoothStep(0, 1, (i - 0.5f) * 2f));
                    transform.localScale = Vector3.one * newScale;

                    t += Time.deltaTime;
                    yield return null;
                }
            }
        }
    }
}