// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System;
using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class RenderProxy : MonoBehaviour
    {
        public DrawStars owner;
        private bool wasValid = false;

        private void OnPostRender()
        {
            if (owner)
            {
                wasValid = true;
                owner.Render(isEditor: false);
            }
        }

        private void Update()
        {
            if (wasValid && !owner)
            {
                Destroy(this);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// This will enable seeing the galaxy in the editor view.
        /// Without that, it will only draw in the game view.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (owner)
            {
                wasValid = true;
                owner.Render(isEditor: true);
            }
        }
#endif
    }

    public class RenderTexturesBucket : SingleInstance<RenderTexturesBucket>
    {
        public RenderTexture downRez;
        public RenderTexture downRezMed;
        public RenderTexture downRezHigh;

        private void CreateBuffers()
        {
            int downRezFactor = 3;
            downRez = new RenderTexture(Camera.main.pixelWidth >> downRezFactor, Camera.main.pixelHeight >> downRezFactor, 0, RenderTextureFormat.ARGB32);
            downRezMed = new RenderTexture(Camera.main.pixelWidth >> (downRezFactor - 1), Camera.main.pixelHeight >> (downRezFactor - 1), 0, RenderTextureFormat.ARGB32);
            downRezHigh = new RenderTexture(Camera.main.pixelWidth >> (downRezFactor - 2), Camera.main.pixelHeight >> (downRezFactor - 2), 0, RenderTextureFormat.ARGB32);
        }

        static bool isInitialized = false;
        public static bool CreateIfNeeded(GameObject owner)
        {
            if (!isInitialized)
            {
                var go = new GameObject("Galaxy Render Textures");
                go.transform.parent = owner.transform;

                var inst = go.AddComponent<RenderTexturesBucket>();

                inst.CreateBuffers();

                isInitialized = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override void OnDestroy()
        {
            isInitialized = false;
            base.OnDestroy();
        }
    }

    public class DrawStars : MonoBehaviour
    {
        public float Age;
        public Material starsMaterial;

        public int starCount;

        public SpiralGalaxy galaxy;

        private ComputeBuffer starsData;

        private bool isFirst;

        public Material screenComposeMaterial;
        public Material screenClearMaterial;

        public bool renderIntoDownscaledTarget;
        public MeshRenderer referenceQuad;
        private float originalTransitionAlpha;

        private Mesh cubeMeshProxy;

        private IEnumerator Start()
        {
            while (!Camera.main)
            {
                yield return null;
            }

            var renderProxy = Camera.main.gameObject.AddComponent<RenderProxy>();
            renderProxy.owner = this;

            if (referenceQuad && referenceQuad.sharedMaterial)
            {
                originalTransitionAlpha = referenceQuad.sharedMaterial.GetFloat("_TransitionAlpha");
            }
        }

        public void CreateBuffers(StarVertDescriptor[] stars)
        {
            if (renderIntoDownscaledTarget)
            {
                isFirst = RenderTexturesBucket.CreateIfNeeded(galaxy.gameObject);
            }

            starsData = new ComputeBuffer(stars.Length, StarVertDescriptor.StructSize);
            starsData.SetData(stars);

            var cubeProxyParent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubeProxyParent.name = "Cube Proxy";
            cubeProxyParent.transform.parent = transform;
            cubeProxyParent.SetActive(false);

            cubeMeshProxy = cubeProxyParent.GetComponent<MeshFilter>().mesh;

            starCount = stars.Length;
        }

        private void DisposeBuffer(ref ComputeBuffer buffer)
        {
            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }
        }

        private void OnDestroy()
        {
            if (referenceQuad && referenceQuad.sharedMaterial)
            {
                referenceQuad.sharedMaterial.SetFloat("_TransitionAlpha", originalTransitionAlpha);
            }

            DisposeBuffer(ref starsData);
        }

        public void Render(bool isEditor)
        {
            if (!enabled || !galaxy.gameObject.activeInHierarchy)
            {
                return;
            }

            var oldRT = RenderTexture.active;

            if (renderIntoDownscaledTarget)
            {
                RenderTexture.active = RenderTexturesBucket.Instance.downRez;
            }

            var mainCam = Camera.main;
            var mainCamTransform = mainCam.transform;

            if (renderIntoDownscaledTarget)
            {
                GL.PushMatrix();
                if (isFirst)
                {
                    GL.Clear(true, true, new Color(0, 0, 0, 0));
                }

                GL.PopMatrix();

                if (referenceQuad)
                {
                    var mesh = referenceQuad.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    referenceQuad.sharedMaterial.SetFloat("_TransitionAlpha", galaxy.TransitionAlpha);
                    referenceQuad.sharedMaterial.SetPass(0);
                    Graphics.DrawMeshNow(mesh, referenceQuad.localToWorldMatrix);
                }
            }

            float wsScale = galaxy.worldSpaceScale * galaxy.transform.lossyScale.x;

            var camDir = galaxy.transform.InverseTransformPoint(mainCamTransform.position).normalized;
            var skipRender = false;

            if (!renderIntoDownscaledTarget)
            {
                wsScale *= Mathf.Clamp01(4 * Math.Max(.1f, Mathf.Abs(camDir.y * .1f)));
            }
            else if (galaxy.isShadow)
            {
                var scaleMultiplier = 1 - Mathf.Clamp01(4 * Math.Max(.1f, Mathf.Abs(camDir.y * .1f)));
                wsScale *= scaleMultiplier;

                if (scaleMultiplier <= .1f)
                {
                    // Skip rendering the clouds, we can't see them anyway
                    skipRender = true;
                }
            }

            // Draw the galaxy
            starsMaterial.SetPass(0);
            starsMaterial.SetBuffer("_Stars", starsData);
            starsMaterial.SetVector("_LocalCamDir", camDir);
            starsMaterial.SetFloat("_WSScale", wsScale);

            starsMaterial.SetVector("_Color", galaxy.tint * galaxy.tintMult * Mathf.Lerp(galaxy.verticalTintMultiplier.x, galaxy.verticalTintMultiplier.y, Mathf.Abs(camDir.y)));

            starsMaterial.SetVector("_EllipseSize", new Vector4(galaxy.XRadii, galaxy.ZRadii, galaxy.MinEllipseScale, galaxy.MaxEllipseScale));
            starsMaterial.SetVector("_FuzzySideScale", galaxy.FuzzySideScale);
            starsMaterial.SetVector("_CamPos", mainCamTransform.position);
            starsMaterial.SetVector("_CamForward", mainCamTransform.forward);
            starsMaterial.SetFloat("_Age", Age);

            starsMaterial.SetMatrix("_GalaxyWorld", galaxy.transform.localToWorldMatrix);

            starsMaterial.SetFloat("_TransitionAlpha", galaxy.TransitionAlpha);

            GL.PushMatrix();

            if (!skipRender)
            {
                Graphics.DrawProcedural(MeshTopology.Points, starCount, 1);
            }

            if (renderIntoDownscaledTarget)
            {
                screenComposeMaterial.mainTexture = RenderTexturesBucket.Instance.downRez;

                RenderTexture.active = oldRT;

                if (!isFirst)
                {
                    RenderTexturesBucket.Instance.downRez.filterMode = FilterMode.Bilinear;
                    RenderTexturesBucket.Instance.downRezMed.filterMode = FilterMode.Bilinear;
                    RenderTexturesBucket.Instance.downRezHigh.filterMode = FilterMode.Bilinear;

                    RenderTexture.active = RenderTexturesBucket.Instance.downRezMed;
                    Graphics.Blit(RenderTexturesBucket.Instance.downRez, screenComposeMaterial, 0);

                    RenderTexture.active = RenderTexturesBucket.Instance.downRezHigh;
                    Graphics.Blit(RenderTexturesBucket.Instance.downRezMed, screenComposeMaterial, 0);

                    RenderTexture.active = oldRT;
                    var renderCubeScale = galaxy.transform.lossyScale;
                    renderCubeScale.x *= galaxy.MaxEllipseScale * 2 * Math.Max(galaxy.XRadii, galaxy.ZRadii);
                    renderCubeScale.z *= galaxy.MaxEllipseScale * 2 * Math.Max(galaxy.XRadii, galaxy.ZRadii);
                    renderCubeScale.y *= galaxy.YRange * 4 * Mathf.Lerp(2, .5f, camDir.y);

                    GL.PopMatrix();

                    screenComposeMaterial.mainTexture = RenderTexturesBucket.Instance.downRezHigh;
#if (UNITY_EDITOR)
                    if (isEditor)
                    {
                        // true if called from DrawGizmos...
                        screenComposeMaterial.SetPass(2);
                    }
                    else
                    {
                        screenComposeMaterial.SetPass(1);
                    }
#else
                    screenComposeMaterial.SetPass(1);
#endif
                    Graphics.DrawMeshNow(cubeMeshProxy, Matrix4x4.TRS(galaxy.transform.position, galaxy.transform.rotation, renderCubeScale));
                }
                else
                {
                    GL.PopMatrix();
                }
            }
            else
            {
                GL.PopMatrix();
            }
        }
    }
}