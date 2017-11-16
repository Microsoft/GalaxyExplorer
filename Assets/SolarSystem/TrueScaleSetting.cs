// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System.Linq;
using UnityEngine;

namespace GalaxyExplorer
{
    public class TrueScaleSetting : SingleInstance<TrueScaleSetting>
    {
        [Range(0, 1)]
        public float CurrentRealismScale = 0;
        private float originalRealismScale;
        public Material OrbitsMaterial;
        public float TargetRealismPlanetScale = 1;

        public Transform AsteroidBelt;
        public Material AsteroidMaterial;
        private float originalTransitionAlpha;
        public float AsteroidBeltRealismScale = 0.2560388f;

        public float TimeSpeedRealistic = 1;
        public float TimeSpeedSchematic = .1f;

        private OrbitUpdater[] planets;
        private float[] originalScales;
        private float previousRealismScale;

        private Transform sun;
        private float originalSunScale;

        private void Awake()
        {
            originalTransitionAlpha = AsteroidMaterial.GetFloat("_TransitionAlpha");
            originalRealismScale = OrbitsMaterial.GetFloat("_Truthfulness");
            planets = FindObjectsOfType<OrbitUpdater>().ToArray();
            originalScales = planets.Select(p => p.transform.localScale.x).ToArray();
            previousRealismScale = -1;

            ResolveSun();
        }

        private void ResolveSun()
        {
            var sunScript = FindObjectOfType<SunBrightnessAdjust>();

            if (sunScript)
            {
                sun = sunScript.transform.parent;
                originalSunScale = sun.localScale.x;
            }
        }

        private void Update()
        {
            if (OrbitsMaterial)
            {
                OrbitsMaterial.SetFloat("_Truthfulness", CurrentRealismScale);
                OrbitsMaterial.SetMatrix("_Orbits2World", transform.localToWorldMatrix);

                if (previousRealismScale != CurrentRealismScale)
                {
                    previousRealismScale = CurrentRealismScale;

                    for (int i = 0; i < originalScales.Length; i++)
                    {
                        if (planets[i])
                        {
                            var scale = Mathf.Lerp(originalScales[i], TargetRealismPlanetScale, CurrentRealismScale);
                            planets[i].transform.localScale = new Vector3(scale, scale, scale);
                            planets[i].Reality = CurrentRealismScale;
                            planets[i].SpeedMultiplier = Mathf.Lerp(TimeSpeedSchematic, TimeSpeedRealistic, CurrentRealismScale);
                        }
                    }

                    if (sun)
                    {
                        var sunScale = Mathf.Lerp(originalSunScale, TargetRealismPlanetScale, CurrentRealismScale);
                        sun.localScale = new Vector3(sunScale, sunScale, sunScale);
                    }
                    else
                    {
                        ResolveSun();
                    }

                    if (AsteroidBelt)
                    {
                        var desiredAsteroidScale = Mathf.Lerp(1, AsteroidBeltRealismScale, CurrentRealismScale);

                        AsteroidBelt.localScale = new Vector3(desiredAsteroidScale, desiredAsteroidScale, desiredAsteroidScale);

                        AsteroidMaterial.SetFloat("_TransitionAlpha", 1 - CurrentRealismScale);
                        AsteroidBelt.gameObject.SetActive(CurrentRealismScale < 1);
                    }
                }
            }
        }
        protected override void OnDestroy()
        {
            if (AsteroidMaterial)
            {
                AsteroidMaterial.SetFloat("_TransitionAlpha", originalTransitionAlpha);
            }
            if (OrbitsMaterial)
            {
                OrbitsMaterial.SetFloat("_Truthfulness", originalRealismScale);
            }
            base.OnDestroy();
        }
    }
}