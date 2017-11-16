// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class Placement : MonoBehaviour
    {
        public Transform Sun;
        public MeshFilter source;
        public MeshFilter generated;
        public int elevationSteps = 10;
        public int azimuthSteps = 16;
        public Vector2 elevationRange = new Vector2(-90, 90);
        public Vector2 azimuthRange = new Vector2(0, 360);

        private Material mat;

        public void Start()
        {
            if (generated)
            {
                var renderer = generated.GetComponent<MeshRenderer>();
                if (renderer)
                {
                    mat = renderer.material;
                }
            }
        }

        public bool FindSunIfNeeded()
        {
            if (!Sun)
            {
                var sunGo = GameObject.Find("Sun");
                if (sunGo)
                {
                    Sun = sunGo.transform;
                    return true;
                }
            }

            return Sun;
        }

        public void Update()
        {
            mat.SetVector("_LocalSpaceCameraPos", generated.transform.InverseTransformPoint(Camera.main.transform.position));

            if (FindSunIfNeeded())
            {
                mat.SetVector("_LocalSpaceSunDir", transform.InverseTransformPoint(Sun.position).normalized);
            }
        }

        [ContextMenu("Generate")]
        private void Generate()
        {
            if (!source)
            {
                throw new System.Exception("source is null");
            }

            var srcV = source.sharedMesh.vertices;
            var srcI = source.sharedMesh.GetIndices(0);

            Vector3[] dstN = new Vector3[srcV.Length * elevationSteps * azimuthSteps];
            Vector3[] dstV = new Vector3[srcV.Length * elevationSteps * azimuthSteps];
            int[] dstI = new int[srcI.Length * elevationSteps * azimuthSteps];

            int dv = 0;
            int di = 0;

            for (int el = 0; el < elevationSteps; ++el)
            {
                var pitch = ((float)el / elevationSteps) * (elevationRange.y - elevationRange.x) + elevationRange.x;
                for (int az = 0; az < azimuthSteps; ++az)
                {
                    var yaw = ((float)az / azimuthSteps) * (azimuthRange.y - azimuthRange.x) + azimuthRange.x;
                    Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

                    for (int si = 0; si < srcI.Length; ++si, ++di)
                    {
                        dstI[di] = srcI[si] + dv;
                    }

                    for (int sv = 0; sv < srcV.Length; ++sv, ++dv)
                    {
                        dstV[dv] = rotation * transform.InverseTransformPoint(source.transform.TransformPoint(srcV[sv]));
                        dstN[dv] = rotation * Vector3.up;
                    }
                }
            }

            generated.sharedMesh = new Mesh();
            generated.sharedMesh.vertices = dstV;
            generated.sharedMesh.normals = dstN;
            generated.sharedMesh.SetIndices(dstI, source.sharedMesh.GetTopology(0), 0);
        }
    }
}