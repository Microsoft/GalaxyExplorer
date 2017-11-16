// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class ContentView : MonoBehaviour
    {
        public float MaxZoomSize = 3.0f;

        public void Awake()
        {
            ToolManager.Instance.LargestZoom = MaxZoomSize;
        }

        public void WillUnload()
        {
            AudioSource[] sources = GetComponentsInChildren<AudioSource>();

            foreach (AudioSource source in sources)
            {
                if (source.isPlaying)
                {
                    StartCoroutine(AudioHelper.FadeOutOverSeconds(source, ViewLoader.AudioFadeoutTime));
                }
            }
        }
    }
}