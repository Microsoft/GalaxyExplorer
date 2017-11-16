// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using UnityEngine;

namespace GalaxyExplorer
{
    public class ToolSounds : SingleInstance<ToolSounds>
    {
        public string HighlightEvent;
        public AudioClip RemoveHighlightClip;
        public AudioClip SelectClip;
        public AudioClip DeselectClip;
        public AudioClip DisabledSelectClip;
        public AudioClip ClickClip;
        public AudioClip DisabledClickClip;
        public AudioClip MoveToolsUpClip;
        public AudioClip MoveToolsDownClip;
        public string EngagedEvent;
        public string DisengagedEvent;

        private AudioSource audioSource;

        public static bool isInitialized = false;

        private void Start()
        {
            isInitialized = true;
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                Debug.LogWarning("ToolSounds has no way to play sounds!");
            }
        }

        protected override void OnDestroy()
        {
            isInitialized = false;
            base.OnDestroy();
        }

        public void PlayHighlightSound()
        {
            UAudioManager.Instance.PlayEvent(HighlightEvent);
        }

        public void PlayRemoveHighlightSound()
        {
            if (audioSource && RemoveHighlightClip)
            {
                audioSource.PlayOneShot(RemoveHighlightClip);
            }
        }

        public void PlaySelectSound()
        {
            if (audioSource && SelectClip)
            {
                audioSource.PlayOneShot(SelectClip);
            }
        }

        public void PlayDeselectSound()
        {
            if (audioSource && DeselectClip)
            {
                audioSource.PlayOneShot(DeselectClip);
            }
        }

        public void PlayDisabledSelectSound()
        {
            if (audioSource && DisabledSelectClip)
            {
                audioSource.PlayOneShot(DisabledSelectClip);
            }
        }

        public void PlayClickSound()
        {
            if (audioSource && ClickClip)
            {
                audioSource.PlayOneShot(ClickClip);
            }
        }

        public void PlayDisabledClickSound()
        {
            if (audioSource && DisabledClickClip)
            {
                audioSource.PlayOneShot(DisabledClickClip);
            }
        }

        public void PlayMoveToolsUpSound()
        {
            if (audioSource && MoveToolsUpClip)
            {
                audioSource.PlayOneShot(MoveToolsUpClip);
            }
        }

        public void PlayMoveToolsDownSound()
        {
            if (audioSource && MoveToolsDownClip)
            {
                audioSource.PlayOneShot(MoveToolsDownClip);
            }
        }

        public void PlayEngagedSound()
        {
            UAudioManager.Instance.PlayEvent(EngagedEvent);
        }

        public void PlayDisengagedSound()
        {
            UAudioManager.Instance.PlayEvent(DisengagedEvent);
        }
    }
}