// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GalaxyExplorer
{
    public class DynamicMeshCombiner : MonoBehaviour
    {
        public class DynamicMeshCombinerHook : MonoBehaviour
        {
            public string type;
            public List<DynamicMeshCombiner> owners;
            public Material material;

            public Vector3[] ref_vertices;
            public Vector3[] ref_normals;
            public Vector2[] ref_uvs;
            public int[] ref_indices;

            private MeshFilter currentFilter;
            private MeshRenderer currentRenderer;
            private Mesh currentMesh;

            private List<DynamicMeshCombiner> instances;

            private Vector3[] vertices;
            private Vector2[] uvs;
            private int[] indices;
            private Vector3[] normals;

            private void Awake()
            {
                owners = new List<DynamicMeshCombiner>();
                instances = new List<DynamicMeshCombiner>();

                currentFilter = gameObject.AddComponent<MeshFilter>();
                currentRenderer = gameObject.AddComponent<MeshRenderer>();

                currentMesh = new Mesh();
                currentFilter.mesh = currentMesh;
                currentMesh.MarkDynamic();
            }

            private void Start()
            {
                currentRenderer.material = material;
            }

            public void Add(DynamicMeshCombiner origin)
            {
                if (!instances.Contains(origin))
                {
                    instances.Add(origin);
                }
            }

            private void LateUpdate()
            {
                Refresh();
            }

            public void Refresh()
            {
                if (ref_vertices == null || ref_vertices.Length < 1)
                {
                    return;
                }

                for (int i = instances.Count - 1; i >= 0; i--)
                {
                    if (!instances[i] || !instances[i].Visible)
                    {
                        instances.RemoveAt(i);
                    }
                }

                if (instances.Count < 1)
                {
                    Destroy(gameObject);
                }

                if (vertices == null || vertices.Length != instances.Count * ref_vertices.Length)
                {
                    vertices = new Vector3[instances.Count * ref_vertices.Length];
                    normals = new Vector3[instances.Count * ref_vertices.Length];
                    uvs = new Vector2[instances.Count * ref_uvs.Length];
                    indices = new int[instances.Count * ref_indices.Length];

                    for (int i = 0; i < instances.Count; i++)
                    {
                        var stepVertexOffset = i * ref_vertices.Length;
                        Array.Copy(ref_uvs, 0, uvs, stepVertexOffset, ref_uvs.Length);

                        for (int index = 0; index < ref_indices.Length; index++)
                        {
                            indices[i * ref_indices.Length + index] = ref_indices[index] + stepVertexOffset;
                        }
                    }

                    currentMesh.Clear();
                }

                for (int i = 0; i < instances.Count; i++)
                {
                    var obj = instances[i].transform;

                    var stepVertexOffset = i * ref_vertices.Length;

                    for (int index = 0; index < ref_vertices.Length; index++)
                    {
                        vertices[stepVertexOffset + index] = obj.TransformPoint(ref_vertices[index]);
                        normals[stepVertexOffset + index] = obj.TransformDirection(ref_normals[index]);
                    }
                }

                currentMesh.vertices = vertices;
                currentMesh.normals = normals;
                currentMesh.SetIndices(indices, MeshTopology.Triangles, 0);
                currentMesh.uv = uvs;
                currentMesh.RecalculateBounds();
            }
        }

        public string type;
        public Material material;

        private DynamicMeshCombinerHook hook;
        private Fader currentFader;

        public bool Visible
        {
            get { return gameObject.activeInHierarchy && (!currentFader || currentFader.GetAlpha() > 0); }
        }

        private void Awake()
        {
            currentFader = GetComponent<Fader>();
            Initialize();
        }

        private void Initialize()
        {
            var filter = GetComponent<MeshFilter>();
            if (!filter)
            {
                Destroy(this);
            }

            var mesh = filter.sharedMesh;

            hook = FindObjectsOfType<DynamicMeshCombinerHook>().FirstOrDefault(h => h.type == type);
            if (!hook)
            {
                var hookGo = new GameObject("DynamicMeshCombiner: " + type);

                hook = hookGo.AddComponent<DynamicMeshCombinerHook>();
                hook.type = type;

                hook.ref_vertices = mesh.vertices;
                hook.ref_uvs = mesh.uv;
                hook.ref_normals = mesh.normals;
                hook.ref_indices = mesh.GetIndices(0);

                hook.gameObject.layer = gameObject.layer;
            }

            var mRenderer = GetComponent<MeshRenderer>();
            mRenderer.enabled = false;

            if (!material)
            {
                if (mRenderer)
                {
                    material = mRenderer.sharedMaterial;
                }
            }

            hook.material = material;
        }

        private void OnDestroy()
        {
            if (hook)
            {
                hook.Refresh();
            }
        }

        private void Update()
        {
            if (!Visible)
            {
                return;
            }

            if (!hook)
            {
                Initialize();

                if (hook)
                {
                    hook.Add(this);
                }
            }
            else
            {
                hook.Add(this);
            }
        }
    }
}