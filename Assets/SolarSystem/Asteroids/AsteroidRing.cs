// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GalaxyExplorer
{
    [RequireComponent(typeof(MeshFilter))]
    public class AsteroidRing : MonoBehaviour
    {
        private struct InstanceData
        {
            public float startAngle;
            public float ringRadius;
            public float verticalOffset;
        }

        public Material instancingMaterial;
        private Vector3 originalLightPosition;

        private MeshFilter meshFilter;

        public float meshScale = 1;

        public int ringCount;
        public float startRadius;
        public float endRadius;
        public int stepCount = 240;

        public Vector3 minScale = Vector3.one;
        public Vector3 maxScale = Vector3.one;

        public float maxVerticalRandomOffset = 0;
        public float minRadiusOffset = 1;
        public float maxRadiusOffset = 1;

        public Transform sunTransform;

        public int lastCount = 0;

        public float rotationSpeed;
        private float age;

        private GameObject generated;

        public Mesh generatedBakedMesh;

        // the size of the asteroid belt to fit appropriately in the solar system
        public Vector3 defaultLocalScale = new Vector3(0.833f, 0.833f, 0.833f);

        private void Awake()
        {
            if (instancingMaterial != null)
            {
                originalLightPosition = instancingMaterial.GetVector("_LightPosition");
            }
        }

        private void Start()
        {
            meshFilter = GetComponent<MeshFilter>();

            if (generatedBakedMesh != null)
            {
                BuildBuffers(null, generatedBakedMesh);
            }
            else
            {
                GeneratePattern();
            }
        }

        [ContextMenu("Generate Pattern")]
        public void GeneratePattern()
        {
            List<InstanceData> positions = GeneratePositions();

            lastCount = positions.Count;
            BuildBuffers(positions.ToArray(), generatedBakedMesh);
        }

        private List<InstanceData> GeneratePositions()
        {
            var positions = new List<InstanceData>();

            for (int ring = 0; ring < ringCount; ring++)
            {
                for (float t = 0; t < 360; t += 360f / stepCount)
                {
                    positions.Add(new InstanceData()
                    {
                        ringRadius = Mathf.Lerp(startRadius, endRadius, ring / (float)ringCount) + Mathf.Lerp(minRadiusOffset, maxRadiusOffset, UnityEngine.Random.value),
                        startAngle = UnityEngine.Random.value * Mathf.PI * 2,
                        verticalOffset = maxVerticalRandomOffset * (UnityEngine.Random.value * 2 - 1)
                    });
                }
            }

            return positions;
        }

        [ContextMenu("Bake Pattern")]
        public void BakePatternDesign()
        {
            List<InstanceData> positions = GeneratePositions();

            lastCount = positions.Count;
            var lastBuiltMesh = BakeMesh(positions.ToArray());

#if UNITY_EDITOR
            string assetPath;

            if (generatedBakedMesh != null)
            {
                assetPath = UnityEditor.AssetDatabase.GetAssetPath(generatedBakedMesh);
            }
            else
            {
                assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/AsteroidBelt.asset");
            }

            UnityEditor.AssetDatabase.CreateAsset(lastBuiltMesh, assetPath);
            UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.Default);
            UnityEditor.AssetDatabase.SaveAssets();

            generatedBakedMesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
#endif
        }

        private Mesh BuildBuffers(InstanceData[] instances, Mesh existingMesh)
        {
            if (meshFilter)
            {
                Mesh mesh;

                if (existingMesh == null)
                {
                    mesh = BakeMesh(instances);
                }
                else
                {
                    mesh = existingMesh;
                }

                generated = new GameObject("Generated");
                generated.transform.SetParent(transform, false);
                generated.transform.localScale = defaultLocalScale;
                var filter = generated.AddComponent<MeshFilter>();

                filter.sharedMesh = mesh;

                var renderer = generated.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = instancingMaterial;

                generated.AddComponent<NoAutomaticFade>();

                return mesh;
            }
            else
            {
                return null;
            }
        }

        private Mesh BakeMesh(InstanceData[] instances)
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            Mesh mesh = new Mesh();
            var meshVertices = meshFilter.sharedMesh.vertices;
            var meshUVs = meshFilter.sharedMesh.uv;
            var indices = meshFilter.sharedMesh.GetIndices(0);
            var normals = meshFilter.sharedMesh.normals;

            var vertexData = new List<Vector3>();
            var normalData = new List<Vector3>();
            var uv0Data = new List<Vector2>();

            for (int instanceIdx = 0; instanceIdx < instances.Length; instanceIdx++)
            {
                var scale = Vector3.Lerp(minScale, maxScale, UnityEngine.Random.value);
                var cur = instances[instanceIdx];

                var instancePos = new Vector3(Mathf.Cos(cur.startAngle), cur.verticalOffset, Mathf.Sin(cur.startAngle)) * cur.ringRadius;

                var instanceRotation = Quaternion.AngleAxis(UnityEngine.Random.value * 360, UnityEngine.Random.insideUnitSphere);

                vertexData.AddRange(meshVertices.Select(v => instancePos + instanceRotation * (new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z) * meshScale)));
                normalData.AddRange(normals.Select(v => Vector3.Normalize(instanceRotation * new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z) * meshScale)));
                uv0Data.AddRange(meshUVs);
            }

            mesh.SetVertices(vertexData);
            mesh.SetIndices(Enumerable.Range(0, instances.Length).SelectMany(i => indices.Select(vi => i * meshVertices.Length + vi)).ToArray(), MeshTopology.Triangles, 0);
            mesh.SetUVs(0, uv0Data);
            mesh.SetNormals(normalData);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private void Update()
        {
            age += Time.deltaTime * rotationSpeed;

            if (instancingMaterial)
            {
                if (sunTransform)
                {
                    instancingMaterial.SetVector("_LightPosition", sunTransform.transform.position);
                }

                generated.transform.localRotation = Quaternion.AngleAxis(age * Mathf.Rad2Deg, Vector3.up);
            }
        }

        private void OnDestroy()
        {
            if (instancingMaterial != null)
            {
                instancingMaterial.SetVector("_LightPosition", originalLightPosition);
            }
        }
    }
}