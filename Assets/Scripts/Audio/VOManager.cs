// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class VOManager : SingleInstance<VOManager>
    {
        [Serializable]
        public class QueuedAudioClip
        {
            public AudioClip clip;
            public float delay;
            public bool allowReplay;

            public QueuedAudioClip(AudioClip clip, float delay, bool allowReplay)
            {
                this.clip = clip;
                this.delay = delay;
                this.allowReplay = allowReplay;
            }
        }

        [Header("Intro Logo")]
        public QueuedAudioClip Welcome;

        [Header("Disclaimer")]
        public QueuedAudioClip Disclaimer1;
        public QueuedAudioClip Disclaimer2;
        public QueuedAudioClip Disclaimer3;

        [Header("Explore Room")]
        public QueuedAudioClip ExploreRoom;
        public QueuedAudioClip StandFacingWall;

        [Header("Paint Wall")]
        public QueuedAudioClip PaintWall;
        public QueuedAudioClip EachWall;

        [Header("Place Logo")]
        public QueuedAudioClip PlaceLogo;

        [Header("Intro to Toolbar")]
        public QueuedAudioClip Toolbar;
        public QueuedAudioClip GazeAndGestures;
        public QueuedAudioClip VoiceCommand;
        public QueuedAudioClip POIMarkers;

        [Header("Destinations")]
        public QueuedAudioClip TrueScale;

        [Header("UI Navigation")]
        public QueuedAudioClip TapToContinue;

        public string[] startTutorialAliases;
        public string[] stopTutorialAliases;

        public float FadeOutTime = 2.0f;

        private bool VOEnabled = true;

        private AudioSource audioSource;
        private Queue<QueuedAudioClip> clipQueue;

        private AudioClip nextClip;
        private float nextClipDelay;
        private float defaultVolume;

        private List<string> playedClips;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            clipQueue = new Queue<QueuedAudioClip>();

            playedClips = new List<string>();

            defaultVolume = audioSource.volume;

            foreach (string startAlias in startTutorialAliases)
            {
                PlayerInputManager.Instance.AddSpeechCallback(startAlias, StartTutorial);
            }

            foreach (string stopAlias in stopTutorialAliases)
            {
                PlayerInputManager.Instance.AddSpeechCallback(stopAlias, StopTutorial);
            }

            KeyboardInput.Instance.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(KeyCode.V, KeyboardInput.KeyEvent.KeyReleased), ToggleVOState);
        }

        private void ToggleVOState(KeyboardInput.KeyCodeEventPair keyCodeEvent)
        {
            SetVOState(!VOEnabled);
        }

        private void StartTutorial(string keyword)
        {
            SetVOState(true);
        }

        private void StopTutorial(string keyword)
        {
            SetVOState(false);
        }

        private void Update()
        {
            if (AudioHelper.FadingOut)
            {
                // Don't process any of queue while fading out
                return;
            }

            if (nextClip)
            {
                nextClipDelay -= Time.deltaTime;

                if (nextClipDelay <= 0.0f)
                {
                    // Fading out sets volume to 0, ensure we're playing at the right
                    // volume every time
                    audioSource.volume = defaultVolume;
                    audioSource.PlayOneShot(nextClip);
                    nextClip = null;
                }
            }
            else if (clipQueue.Count > 0 && !audioSource.isPlaying)
            {
                QueuedAudioClip queuedClip = clipQueue.Dequeue();

                if (queuedClip.clip && (queuedClip.allowReplay || !playedClips.Contains(queuedClip.clip.name)))
                {
                    nextClip = queuedClip.clip;
                    nextClipDelay = queuedClip.delay;

                    playedClips.Add(nextClip.name);
                }
            }
        }

        public bool SetVOState(bool enabled)
        {
            VOEnabled = enabled;

            if (VOEnabled)
            {
                audioSource.volume = defaultVolume;
            }
            else
            {
                clipQueue.Clear();
                StartCoroutine(AudioHelper.FadeOutOverSeconds(audioSource, FadeOutTime));
            }

            return VOEnabled;
        }

        public bool PlayClip(QueuedAudioClip clip, bool replaceQueue = false)
        {
            return PlayClip(clip.clip, clip.delay, clip.allowReplay, replaceQueue);
        }

        public bool PlayClip(AudioClip clip, float delay = 0.0f, bool allowReplay = false, bool replaceQueue = false)
        {
            bool clipWillPlay = false;

            if (VOEnabled)
            {
                if (replaceQueue)
                {
                    clipQueue.Clear();
                }

                clipQueue.Enqueue(new QueuedAudioClip(clip, delay, allowReplay));

                clipWillPlay = true;
            }

            return clipWillPlay;
        }

        public void Stop(bool clearQueue = false)
        {
            if (clearQueue)
            {
                clipQueue.Clear();
            }

            nextClip = null;

            // Fade out the audio that's currently playing to stop it. Check here to
            // prevent coroutines from stacking up and calling Stop() on audioSource
            // at undesired times. Audio that would be faded out instead would just
            // be skipped over if the queue was cleared, which is what we want.
            if (!AudioHelper.FadingOut)
            {
                StartCoroutine(AudioHelper.FadeOutOverSeconds(audioSource, FadeOutTime));
            }
        }
    }
}