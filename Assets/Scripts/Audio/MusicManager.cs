// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using UnityEngine;
using UnityEngine.Audio;

namespace GalaxyExplorer
{
    public class MusicManager : SingleInstance<MusicManager>
    {
        public AudioMixer mixer;

        public string Welcome = "01_Welcome";
        public string Galaxy = "02_Galaxy";
        public string SolarSystem = "03_SolarSystem";
        public string Planet = "04_PlanetaryView";

        private const float TransitionTime = 3.8f;

        public bool FindSnapshotAndTransition(string name, float time = TransitionTime)
        {
            bool transitioned = false;

            if (mixer)
            {
                AudioMixerSnapshot snapshot = mixer.FindSnapshot(name);

                if (snapshot)
                {
                    snapshot.TransitionTo(time);
                    transitioned = true;
                }
                else
                {
                    Debug.LogWarning("Couldn't find AudioMixer Snapshot with name " + name);
                }
            }

            return transitioned;
        }
    }
}