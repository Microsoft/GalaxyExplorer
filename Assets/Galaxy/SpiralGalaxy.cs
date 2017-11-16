// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GalaxyExplorer
{
    public class SpiralGalaxy : MonoBehaviour
    {
        [System.Serializable]
        public struct StarDistributionGroup
        {
            public string GroupName;
            public float Percentage;
            public bool UseColorRange;
            public Gradient ColorRange;
            public Vector2 UVOffset;
            public Vector2 SizeMultiplierRange;
            public bool SizeIsAbsolute;
            public float RandomEllipseScaleOffset;
        }

        private const float LookAheadAngle = 0.02f;

        [Tooltip("X for 0 elevation, Y for 90 deg elevation")]
        public Vector2 verticalTintMultiplier = new Vector2(1, 1);

        public int EllipseCount = 1;

        public float XRadii = 1f;
        public float YRange = 1f;
        public float ZRadii = 1f;

        public int StarsPerEllipse = 100;

        public float MinEllipseScale = 1f;
        public float MaxEllipseScale = 1f;

        public float SpiralRotation = 180f;

        public float TransitionAlpha = 1;
        public Vector2 FuzzySideScale = new Vector2(1, 1);

        public AnimationCurve centerToRimStarSize;
        public AnimationCurve centerToRimVerticalOffset;

        public Gradient centerToRimGradient;

        public List<StarDistributionGroup> StarGroups;

        public Material baseMaterial;
        public Material screenComposeMaterial;
        public Material screenClearMaterial;
        public float worldSpaceScale;
        public int lastCount;
        public float velocityMultiplier = 5;

        private GameObject generated;
        private DrawStars generatedDrawer;

        public bool renderIntoDownscaledTarget;
        public Color tint = Color.white;
        public float tintMult = 1;

        private float age = 0;
        private float starGroupTotals = 0;

        public int index = 0;
        public MeshRenderer referenceQuad;

        public bool enableSecondArm = false;
        public float secondArmStartOffsetDeg = 95;

        public StarsData bakedStars;
        public int lastBakedStarsCount;

        public bool isShadow;

        [ContextMenu("Bake Stars")]
        public void BakeStarsDesign()
        {
#if UNITY_EDITOR
            string assetPath;

            if (bakedStars != null)
            {
                assetPath = UnityEditor.AssetDatabase.GetAssetPath(bakedStars);
            }
            else
            {
                assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Galaxy.asset");
            }

            bakedStars = ScriptableObject.CreateInstance<StarsData>();
            bakedStars.stars = CreateStarsContent();

            UnityEditor.AssetDatabase.CreateAsset(bakedStars, assetPath);
            UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.Default);
            UnityEditor.AssetDatabase.SaveAssets();

            bakedStars = UnityEditor.AssetDatabase.LoadAssetAtPath<StarsData>(assetPath);

            lastBakedStarsCount = bakedStars.stars.Length;
#endif
        }

        private IEnumerator Start()
        {
            while (!Camera.main || !Camera.main.isActiveAndEnabled)
            {
                yield return null;
            }

            while (index-- > 0)
            {
                yield return null;
            }

            InitializeParticles();
        }

        [ContextMenu("Initialize")]
        private void InitializeParticles()
        {
            var stars = (bakedStars == null || bakedStars.stars == null || bakedStars.stars.Length < 1) ? CreateStarsContent() : bakedStars.stars;

            if (generated)
            {
                Destroy(generated);
            }

            generated = new GameObject();
            generated.name = "Generated";
            generated.transform.parent = transform;
            generated.transform.localPosition = Vector3.zero;
            generated.transform.localRotation = Quaternion.identity;
            generated.transform.localScale = Vector3.one;

            generatedDrawer = generated.AddComponent<DrawStars>();
            generatedDrawer.galaxy = this;
            generatedDrawer.screenComposeMaterial = screenComposeMaterial;
            generatedDrawer.screenClearMaterial = screenClearMaterial;

            generatedDrawer.referenceQuad = referenceQuad;

            if (referenceQuad)
            {
                var refQuadRenderer = referenceQuad.GetComponent<MeshRenderer>();
                if (refQuadRenderer)
                {
                    refQuadRenderer.enabled = false;
                }
            }

            generatedDrawer.renderIntoDownscaledTarget = renderIntoDownscaledTarget;

            baseMaterial = Instantiate(baseMaterial);
            generatedDrawer.starsMaterial = baseMaterial;

            generatedDrawer.CreateBuffers(stars);

            lastCount = stars.Length;
        }

        private StarVertDescriptor[] CreateStarsContent()
        {
            IEnumerable<PosVel> starsContent = GenerateEllipses(EllipseCount, 0);

            if (enableSecondArm)
            {
                starsContent = starsContent.Concat(GenerateEllipses(EllipseCount, secondArmStartOffsetDeg));
            }

            var stars = starsContent;

            var points = stars.Select((v, i) => new StarVertDescriptor()
            {
                yOffset = v.position.y,
                curveOffset = v.curveOffset,
                ellipseDistance = Mathf.Lerp(MinEllipseScale, MaxEllipseScale, v.ellipseDistance) * Mathf.Lerp(FuzzySideScale.x, FuzzySideScale.y, UnityEngine.Random.value),
                ellipseOffset = v.ellipseOffset,
                uv = v.uv,
                size = v.size,
                color = new Vector3(v.color.r, v.color.g, v.color.b),
                random = UnityEngine.Random.value
            }).ToArray();

            return points;
        }

        private void Update()
        {
            age += Time.deltaTime;

            if (generatedDrawer)
            {
                baseMaterial.SetFloat("_Age", age * velocityMultiplier);

                generatedDrawer.Age = age * velocityMultiplier;
            }
        }

        public PosVel[] GenerateEllipses(int ellipseCount, float initialOffset)
        {
            List<PosVel> result = new List<PosVel>();

            float offset = SpiralRotation / (float)ellipseCount;

            for (int i = 0; i < ellipseCount; i++)
            {
                float progression = (float)i / (float)EllipseCount;

                result.AddRange(GenerateEllipse(initialOffset + (offset * i), progression));
            }

            return result.ToArray();
        }

        public PosVel[] GenerateEllipse(float offset, float ellipseProgression)
        {
            List<PosVel> result = new List<PosVel>(StarsPerEllipse);
            float ellipseScale = Mathf.Lerp(MinEllipseScale, MaxEllipseScale, ellipseProgression);

            for (int i = 0; i < StarsPerEllipse; i++)
            {
                float x = 0;
                float y = 0;
                float z = 0;

                float t = Random.Range(0, Mathf.PI * 2f);

                var maxRadii = Mathf.Max(XRadii, ZRadii);
                float maxDist = MaxEllipseScale * maxRadii;

                x = Mathf.Cos(t) * XRadii;
                z = Mathf.Sin(t) * ZRadii;

                var zp = (z * Mathf.Cos(offset * Mathf.Deg2Rad)) - (x * Mathf.Sin(offset * Mathf.Deg2Rad));
                var xp = (z * Mathf.Sin(offset * Mathf.Deg2Rad)) + (x * Mathf.Cos(offset * Mathf.Deg2Rad));

                x = xp;
                z = zp;

                Vector3 flatPos = new Vector3(x, y, z);
                float distance = flatPos.magnitude;
                float distPercent = Mathf.InverseLerp(0, maxDist, distance) * ellipseProgression;

                float verticalOffset = centerToRimVerticalOffset.Evaluate(distPercent) * YRange;
                y = Random.Range(-verticalOffset, verticalOffset);

                var pv = new PosVel();

                pv.curveOffset = t;
                pv.ellipseOffset = offset * Mathf.Deg2Rad;
                pv.ellipseDistance = ellipseProgression;
                pv.position = new Vector3(x * ellipseScale, y, z * ellipseScale);

                SetStarType(ref pv, distPercent, ellipseProgression);

                if (pv.size > 0)
                {
                    result.Add(pv);
                }
            }

            return result.ToArray();
        }

        public void SetStarType(ref PosVel pv, float distPercent, float ellipseProgression)
        {
            if (starGroupTotals == 0)
            {
                foreach (var item in StarGroups)
                {
                    starGroupTotals += item.Percentage * 0.01f; // This multiplied allows artists to use whole numbers in the UI. 
                }
            }

            float starType = Random.Range(0, starGroupTotals);

            float total = 0;
            for (int i = 0; i < StarGroups.Count; i++)
            {
                var sg = StarGroups[i];

                total += sg.Percentage * 0.01f;  // This multiplied allows artists to use whole numbers in the UI. 

                if (starType < total)
                {
                    pv.uv = sg.UVOffset;
                    pv.color = sg.UseColorRange ? sg.ColorRange.Evaluate(Random.value) : centerToRimGradient.Evaluate(distPercent);

                    float sgSize = Random.Range(sg.SizeMultiplierRange.x, sg.SizeMultiplierRange.y);
                    pv.size = sg.SizeIsAbsolute ? sgSize : centerToRimStarSize.Evaluate(ellipseProgression) * sgSize;
                    pv.ellipseDistance += sg.RandomEllipseScaleOffset * ((Random.value * 2) - 1);
                    break;
                }
            }
        }
    }
}