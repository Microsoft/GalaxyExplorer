// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class LocationView : MonoBehaviour
    {
        public string IntroMusicEvent;
        public string MusicEvent;
        public float MusicDelayInSeconds = 1.0f;
        public VOManager.QueuedAudioClip VoiceOver;
        public AudioSource IntroSFX;

        private bool startTimer = false;
        private float delayTimer = 0.0f;
        private string musicEvent;

        private void Awake()
        {
            delayTimer = MusicDelayInSeconds;
        }

        private void Start()
        {
            musicEvent = MusicEvent;
            if (InIntro())
            {
                musicEvent = IntroMusicEvent;
            }

            ViewLoader.Instance.CoreSystemsLoaded += CoreSystemsLoaded;
            if (TransitionManager.Instance)
            {
                TransitionManager.Instance.ContentLoaded += ViewContentLoaded;
            }
        }

        private void OnDestroy()
        {
            if (TransitionManager.Instance)
            {
                TransitionManager.Instance.ContentLoaded -= ViewContentLoaded;
            }
        }

        private void Update()
        {
            if (startTimer)
            {
                delayTimer -= Time.deltaTime;
                if (delayTimer <= 0.0f)
                {
                    MusicManager.Instance.FindSnapshotAndTransition(musicEvent);
                    startTimer = false;
                }
            }
        }

        private void CoreSystemsLoaded()
        {
            ViewLoader.Instance.CoreSystemsLoaded -= CoreSystemsLoaded;

            // Only transition if we have music to transition with
            startTimer = !string.IsNullOrEmpty(musicEvent);

            // if the introduction flow exists, it means we shouldn't 
            // stop or play VO, the introduction flow will handle that for us
            if (!InIntro())
            {
                VOManager.Instance.Stop(clearQueue: true);
            }
            else if (IntroSFX)
            {
                IntroSFX.Play();
            }
        }

        private void ViewContentLoaded()
        {
            if (TransitionManager.Instance)
            {
                TransitionManager.Instance.ContentLoaded -= ViewContentLoaded;
            }

            // if the introduction flow exists, it means we shouldn't 
            // stop or play VO, the introduction flow will handle that for us
            if (!InIntro())
            {
                VOManager.Instance.PlayClip(VoiceOver);
            }
        }

        private bool InIntro()
        {
            return IntroductionFlow.Instance.enabled;
        }
    }
}