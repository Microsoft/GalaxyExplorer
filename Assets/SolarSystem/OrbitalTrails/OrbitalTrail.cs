// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GalaxyExplorer
{
    public class OrbitalTrail : MonoBehaviour
    {
        public class OrbitsRenderer : SingleInstance<OrbitsRenderer>
        {
            public class OrbitsRendererCameraProxy : MonoBehaviour
            {
                public OrbitsRenderer owner;

                public void Update()
                {
                    if (!owner)
                    {
                        Destroy(this);
                    }
                }

                private void OnPostRender()
                {
                    if (owner)
                    {
                        owner.RenderOrbits();
                    }
                }

                private void OnDrawGizmos()
                {
                    if (owner)
                    {
                        owner.RenderOrbits();
                    }
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct OrbitDataPoint
            {
                public const int size =
                    sizeof(float) * 3 +
                    sizeof(float) * 3 +
                    sizeof(uint) +

                    sizeof(uint) +
                    sizeof(uint) +
                    sizeof(uint);

                public Vector3 realPos;
                public Vector3 schematicPos;
                public uint globalIndex;

                public uint orbitStartIndex;
                public uint orbitEntryCount;
                public uint orbitIndex;
            }
            
            static bool isInitialized = false;
            public static OrbitsRenderer GetOrCreate(Transform world)
            {
                if (!isInitialized)
                {
                    var go = new GameObject("Orbits Renderer Hook");
                    go.transform.SetParent(world, worldPositionStays: false);
                    go.AddComponent<NoAutomaticFade>();

                    var renderer = go.AddComponent<OrbitsRenderer>();
                    renderer.orbitsWorld = world;

                    isInitialized = true;
                    return renderer;
                }
                else
                {
                    return OrbitsRenderer.Instance;
                }
            }

            public Material orbitsMaterial;
            private Transform orbitsWorld;

            /// <summary>
            /// There is only one solar system at anytime, so we kill ourselves
            /// when any of our trails dies
            /// </summary>
            private List<OrbitalTrail> registeredTrails = new List<OrbitalTrail>();
            private uint currentStartIndex = 0;

            private List<OrbitDataPoint> orbitsData = new List<OrbitDataPoint>();

            private ComputeBuffer orbitsBuffer;
            private bool dataInvalidated = true;

            private OrbitsRendererCameraProxy cameraProxy;

            private float previousTruthfulness = -1;

            public void AddOrbit(OrbitalTrail origin, List<Vector3> realPositions, List<Vector3> schematicPositions)
            {
                if (realPositions.Count != schematicPositions.Count)
                {
                    throw new InvalidOperationException("Real and schematic positions must have the same length");
                }

                registeredTrails.Add(origin);

                for (int i = 0; i < realPositions.Count; i++)
                {
                    var stepData = new OrbitDataPoint()
                    {
                        globalIndex = (uint)i,
                        realPos = realPositions[i],
                        schematicPos = schematicPositions[i],
                        orbitStartIndex = currentStartIndex,
                        orbitEntryCount = (uint)schematicPositions.Count,
                        orbitIndex = (uint)(registeredTrails.Count - 1)
                    };

                    orbitsData.Add(stepData);
                }

                currentStartIndex += (uint)realPositions.Count;

                dataInvalidated = true;
            }

            private void ReCreateData()
            {
                DestroyBuffers();

                if (registeredTrails.Count > 0)
                {
                    orbitsBuffer = new ComputeBuffer(orbitsData.Count, OrbitDataPoint.size);
                    orbitsBuffer.SetData(orbitsData.ToArray());
                }
            }

            protected override void OnDestroy()
            {
                DestroyBuffers();
                isInitialized = false;
                base.OnDestroy();
            }

            private void DestroyBuffers()
            {
                if (orbitsBuffer != null)
                {
                    orbitsBuffer.Dispose();
                    orbitsBuffer = null;
                }
            }

            private void Update()
            {
                if (TrueScaleSetting.Instance && orbitsMaterial)
                {
                    if (previousTruthfulness != TrueScaleSetting.Instance.CurrentRealismScale)
                    {
                        previousTruthfulness = TrueScaleSetting.Instance.CurrentRealismScale;

                        if (previousTruthfulness == 1)
                        {
                            orbitsMaterial.DisableKeyword("IN_TRANSITION");
                            orbitsMaterial.DisableKeyword("SCHEMATIC");
                            orbitsMaterial.EnableKeyword("REALSCALE");
                        }
                        else if (previousTruthfulness == 0)
                        {
                            orbitsMaterial.DisableKeyword("IN_TRANSITION");
                            orbitsMaterial.DisableKeyword("REALSCALE");
                            orbitsMaterial.EnableKeyword("SCHEMATIC");
                        }
                        else
                        {
                            orbitsMaterial.DisableKeyword("SCHEMATIC");
                            orbitsMaterial.DisableKeyword("REALSCALE");
                            orbitsMaterial.EnableKeyword("IN_TRANSITION");
                        }
                    }
                }

                for (int i = 0; i < registeredTrails.Count; i++)
                {
                    var item = registeredTrails[i];

                    if (!item)
                    {
                        Destroy(gameObject);
                    }
                    else if (orbitsMaterial)
                    {
                        var planetPos = item.planet.transform.position;
                        orbitsMaterial.SetVector("planetPositionsAndRadius" + i.ToString(), new Vector4(planetPos.x, planetPos.y, planetPos.z, 0));
                    }
                }

                if (dataInvalidated)
                {
                    dataInvalidated = false;
                    ReCreateData();
                }

                if (!cameraProxy && Camera.main)
                {
                    cameraProxy = Camera.main.gameObject.AddComponent<OrbitsRendererCameraProxy>();
                    cameraProxy.owner = this;
                }
            }

            public void RenderOrbits()
            {
                if (orbitsMaterial && orbitsBuffer != null && orbitsWorld && orbitsWorld.gameObject.activeInHierarchy)
                {
                    orbitsMaterial.SetPass(0);
                    orbitsMaterial.SetBuffer("_OrbitsData", orbitsBuffer);
                    orbitsMaterial.SetMatrix("_Orbits2World", orbitsWorld.transform.localToWorldMatrix);
                    orbitsMaterial.SetFloat("_GlobalScale", orbitsWorld.lossyScale.x);

                    Graphics.DrawProcedural(MeshTopology.Points, orbitsData.Count);
                }
            }
        }

        public Material orbitMaterial;
        private float originalGlobalScale;
        private float originalWidth;

        public int orbitStepCount = 50;

        public PointOfInterest pointOfInterestMarker;
        public float pointOfInterestAngle;

        public bool snapPointOfInterestToPosition;

        private GameObject pointOfInterestTarget;

        private OrbitUpdater planet;
        private GameObject container;

        private Transform sun;

        private bool FindSunIfNeeded()
        {
            if (!sun)
            {
                var sunGo = container.transform.parent.gameObject.GetComponentInChildren<SunBrightnessAdjust>().gameObject;
                if (sunGo)
                {
                    sun = sunGo.transform;
                    return true;
                }
            }

            return sun;
        }

        private void Awake()
        {
            // We put the trail object under the planet, but we want it to be the parented to the parent of the planet ...
            // which will be the solar system
            planet = GetComponentInParent<OrbitUpdater>();

            container = new GameObject("Orbit Container " + planet.name);

            var planetTransform = planet.transform;
            container.transform.SetParent(planetTransform.parent, worldPositionStays: false);

            FindSunIfNeeded();

            container.AddComponent<NoAutomaticFade>();

            List<Vector3> realPositions, schematicPositions;

            GeneratePositionsForOrbit(out realPositions, out schematicPositions);

            var orbitsRenderer = OrbitsRenderer.GetOrCreate(planet.transform.parent);
            orbitsRenderer.AddOrbit(this, realPositions, schematicPositions);
            originalGlobalScale = orbitMaterial.GetFloat("_GlobalScale");
            originalWidth = orbitMaterial.GetFloat("_Width");
            if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD)
            {
                orbitMaterial.SetFloat("_Width", MyAppPlatformManager.OrbitalTrailFixedWidth);
            }

            orbitsRenderer.orbitsMaterial = orbitMaterial;
        }

        private void GeneratePositionsForOrbit(out List<Vector3> realPositions, out List<Vector3> schematicPositions)
        {
            var orbitStep = planet.CurrentPeriod / orbitStepCount;

            var orbitStepTime = TimeSpan.FromDays(orbitStep);
            var currentDate = DateTime.MinValue;

            realPositions = new List<Vector3>();
            schematicPositions = new List<Vector3>();

            for (float currentStep = 0; currentStep < planet.CurrentPeriod; currentStep += orbitStep)
            {
                planet.Reality = 1;
                var realStepPosition = planet.CalculatePosition(currentDate);

                planet.Reality = 0;
                var schematicPosition = planet.CalculatePosition(currentDate);

                realPositions.Add(realStepPosition);
                schematicPositions.Add(schematicPosition);

                currentDate += orbitStepTime;
            }

            realPositions.RemoveAt(realPositions.Count - 1);
            schematicPositions.RemoveAt(schematicPositions.Count - 1);
        }

        private void Start()
        {
            // create a point of interest target position
            if (pointOfInterestMarker != null && planet != null && container != null)
            {
                pointOfInterestTarget = new GameObject("POI_OrbitTarget");
                pointOfInterestTarget.transform.localPosition = planet.CalculatePosition(pointOfInterestAngle * Mathf.Deg2Rad);
                pointOfInterestTarget.transform.localRotation = Quaternion.identity;
                pointOfInterestTarget.transform.localScale = Vector3.one;
                pointOfInterestTarget.transform.SetParent(container.transform, false);

                if (pointOfInterestMarker.IndicatorLine != null)
                {
                    pointOfInterestMarker.IndicatorLine.points[0] = pointOfInterestTarget.transform;
                }
            }
        }

        private void Update()
        {
            FindSunIfNeeded();

            if (pointOfInterestTarget != null && planet != null)
            {
                if (snapPointOfInterestToPosition)
                {
                    pointOfInterestTarget.transform.position = planet.transform.position;
                }
                else
                {
                    pointOfInterestTarget.transform.localPosition = planet.CalculatePosition(pointOfInterestAngle * Mathf.Deg2Rad);
                }
            }
        }

        private void OnDestroy()
        {
            orbitMaterial.SetFloat("_GlobalScale", originalGlobalScale);
            orbitMaterial.SetFloat("_Width", originalWidth);
        }
    }
}