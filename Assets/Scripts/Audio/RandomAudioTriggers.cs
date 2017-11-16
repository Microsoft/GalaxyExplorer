// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class RandomAudioTriggers : MonoBehaviour
    {
        [SerializeField]
        private float startUpDelay = 0;
#pragma warning disable 0649    // Private to scripts, public to editor
        [SerializeField, Tooltip("The time in seconds between sounds being triggered")]
        private float triggerRate;
        [SerializeField]
        private AudioTrigger[] audioTriggers;
#pragma warning restore 0649

        private bool isRunning = false;
        private float currentTime = 0;

        [System.Serializable]
        public class AudioTrigger
        {
            public string eventName;
            public GameObject emitter;
        }

        private void Start()
        {
            if (this.audioTriggers.Length == 0 || this.triggerRate <= 0)
            {
                this.enabled = false;
            }

            if (this.startUpDelay > 0)
            {
                StartCoroutine(DelayedStart());
            }
        }

        private void Update()
        {
            if (!this.isRunning)
            {
                return;
            }

            if (this.currentTime < this.triggerRate)
            {
                this.currentTime += Time.deltaTime;
                return;
            }

            PlayRandomTrigger();
        }

        public void Activate()
        {
            if (this.audioTriggers.Length == 0 || this.triggerRate <= 0)
            {
                return;
            }

            this.currentTime = 0;
            this.isRunning = true;
        }

        private void PlayRandomTrigger()
        {
            this.currentTime = 0;
            int rndTrigger = Random.Range(0, this.audioTriggers.Length);
            AudioTrigger tempTrigger = this.audioTriggers[rndTrigger];

            if (tempTrigger.emitter == null || string.IsNullOrEmpty(tempTrigger.eventName))
            {
                return;
            }

            UAudioManager.Instance.PlayEvent(tempTrigger.eventName, tempTrigger.emitter);
        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(this.startUpDelay);
            Activate();
        }
    }
}