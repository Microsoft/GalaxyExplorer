// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class BillboardLine : MonoBehaviour
    {
        public class LineFader : Fader
        {
            public BillboardLine owner;

            protected override bool CanAddMaterialsFromRenderer(Renderer renderer, Fader[] faders)
            {
                return false;
            }

            public override bool SetAlpha(float alphaValue)
            {
                if (owner)
                {
                    owner.transitionAlpha = alphaValue;
                }

                alpha = alphaValue;
                return true;
            }
        }

        public Transform headTransform;
        public Material material;
        public float width = 0.01f;
        public Transform[] points;

        public Vector3 bottomOffset;

        public float transitionAlpha = 1;

        private ScaleWithDistance scaleWithDistance;
        private Renderer lineRenderer;
        private MeshFilter filter;
        private Mesh mesh;

        private Color[] colors;
        private Vector3[] vertices;
        private Vector2[] uvs;

        private void Awake()
        {
            var rendererObject = new GameObject("Line");
            rendererObject.layer = LayerMask.NameToLayer("Tools");
            rendererObject.transform.SetParent(transform, worldPositionStays: false);
            rendererObject.AddComponent<NoAutomaticFade>();
            rendererObject.AddComponent<LineFader>().owner = this;

            lineRenderer = rendererObject.GetComponent<MeshRenderer>();
            if (!lineRenderer)
            {
                lineRenderer = rendererObject.AddComponent<MeshRenderer>();
            }

            lineRenderer.sharedMaterial = material;

            filter = rendererObject.GetComponent<MeshFilter>();
            if (!filter)
            {
                filter = rendererObject.AddComponent<MeshFilter>();
            }

            scaleWithDistance = GetComponent<ScaleWithDistance>();

            mesh = new Mesh();
            mesh.MarkDynamic();
            filter.sharedMesh = mesh;
        }

        private void LateUpdate()
        {
            if (headTransform != null)
            {
                UpdateLines();
            }
            else if (Camera.main != null)
            {
                // This happens once the camera is added to the scene (not on start)
                headTransform = Camera.main.transform;
            }
        }

        private void UpdateLines()
        {
            if (points == null || points.Length != 2)
            {
                return;
            }

            int[] indices = mesh.triangles;
            if (vertices == null || vertices.Length != points.Length * 2)
            {
                vertices = new Vector3[points.Length * 2];
                uvs = new Vector2[points.Length * 2];
                colors = new Color[points.Length * 2];
            }

            var lastIndex = points.Length - 1;
            float vCoordDelta = 1.0f / (points.Length - 1.0f);
            float vCoord = 0.0f;
            for (int i = 0; i < points.Length; ++i)
            {
                var startIndex = Mathf.Min(i, lastIndex - 1);
                var endIndex = Mathf.Min(i + 1, lastIndex);
                var dir = points[endIndex].position - points[startIndex].position;

                int vertexIndex = i * 2;
                var startPos = points[i].position;

                if (i == 0)
                {
                    startPos += scaleWithDistance.GetScaleFromPosition(startPos) * (transform.rotation * bottomOffset);
                }

                CalcVertexPair(vertices, vertexIndex, startPos, dir);

                uvs[vertexIndex] = new Vector2(1.0f, vCoord);
                uvs[vertexIndex + 1] = new Vector2(0.0f, vCoord);

                colors[vertexIndex] = Color.white * transitionAlpha;
                colors[vertexIndex + 1] = Color.white * transitionAlpha;

                vCoord += vCoordDelta;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.colors = colors;

            int indexCount = (points.Length - 1) * 6;
            if (indices.Length != indexCount)
            {
                indices = new int[indexCount];
                for (int i = 0, offset = 0; i < indexCount; i += 6, offset += 2)
                {
                    indices[i + 0] = 0 + offset;
                    indices[i + 1] = 1 + offset;
                    indices[i + 2] = 2 + offset;
                    indices[i + 3] = 3 + offset;
                    indices[i + 4] = 2 + offset;
                    indices[i + 5] = 1 + offset;
                }

                mesh.triangles = indices;
            }

            mesh.RecalculateBounds();
        }

        public void OnDrawGizmos()
        {
            if (points == null || points.Length < 2)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            var start = points[0].position;
            for (int i = 1; i < points.Length; ++i)
            {
                if (points[i])
                {
                    var end = points[i].position;
                    Gizmos.DrawLine(start, end);
                    start = end;
                }
            }
        }

        private void SetQuadsEnabled(bool enabled)
        {
            lineRenderer.enabled = enabled;
        }

        public void OnEnable()
        {
            SetQuadsEnabled(true);
        }

        public void OnDisable()
        {
            SetQuadsEnabled(false);
        }

        private void CalcVertexPair(Vector3[] vertices, int vertexIndex, Vector3 start, Vector3 dir)
        {
            Vector3 viewDir = start - headTransform.position;
            Vector3 left = Vector3.Cross(viewDir, dir).normalized; // left-handed coordinate system
            left *= width * 0.5f;

            if (scaleWithDistance != null)
            {
                left *= scaleWithDistance.GetScaleFromPosition(start);
            }

            vertices[vertexIndex] = transform.InverseTransformPoint(start - left);
            vertices[vertexIndex + 1] = transform.InverseTransformPoint(start + left);
        }
    }
}