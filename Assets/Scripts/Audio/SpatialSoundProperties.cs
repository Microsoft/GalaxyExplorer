// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class SpatialSoundProperties : MonoBehaviour
    {
#pragma warning disable 0649    // Private to scripts, public to editor
        [SerializeField]
        private RoomSize roomSize;
#pragma warning restore 0649
        [SerializeField]
        private float minGain = -96f;
        [SerializeField]
        private float maxGain = 12f;
        [SerializeField]
        private float unityGainDistance = 1f;

        private AudioSource audioSource;

        private const int RoomModelIndex = 1;
        private const int MinGainIndex = 2;
        private const int MaxGainIndex = 3;
        private const int UnityGainIndex = 4;

        public enum RoomSize
        {
            Small,
            Medium,
            Large,
            None
        }

        private void Start()
        {
            this.audioSource = GetComponent<AudioSource>();
            if (this.audioSource == null)
            {
                return;
            }

            this.audioSource.SetSpatializerFloat(RoomModelIndex, (float)this.roomSize);
            this.audioSource.SetSpatializerFloat(MinGainIndex, this.minGain);
            this.audioSource.SetSpatializerFloat(MaxGainIndex, this.maxGain);
            this.audioSource.SetSpatializerFloat(UnityGainIndex, this.unityGainDistance);
        }
    }
}