// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;

namespace GalaxyExplorer
{
    public class AudioHelper : MonoBehaviour
    {
        public static bool FadingOut
        {
            get
            {
                return fadingOut;
            }
        }

        private static bool fadingOut;

        public static IEnumerator FadeOutOverSeconds(AudioSource audioSource, float seconds)
        {
            fadingOut = true;

            float deltaTimeAccumulator = 0.0f;
            while (deltaTimeAccumulator < seconds)
            {
                deltaTimeAccumulator += Time.deltaTime;
                audioSource.volume = 1.0f - (deltaTimeAccumulator / seconds);

                yield return null;
            }

            audioSource.Stop();
            fadingOut = false;
        }
    }
}