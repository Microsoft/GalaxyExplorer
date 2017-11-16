// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class MovableAudioSource : MonoBehaviour
    {
        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public float velocity;

        private AudioSource audioSource;
        private Vector3 directionVector;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (audioSource.isPlaying)
            {
                transform.position = transform.position + (directionVector * velocity * Time.deltaTime);
            }
        }

        public void Setup(Vector3 start, Vector3 end)
        {
            audioSource.Stop();
            StartPosition = start;
            EndPosition = end;

            transform.position = StartPosition;
        }

        [ContextMenu("Activate")]
        public void Activate()
        {
            directionVector = EndPosition - StartPosition;
            directionVector.Normalize();
            audioSource.Play();
        }
    }
}