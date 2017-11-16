// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class Fader : MonoBehaviour
    {
        protected struct MaterialSettings
        {
            public Material material;
            public Renderer renderer;
            public int originalSourceBlend;
            public int originalDestinationBlend;
        }

        private MaterialSettings[] currentMaterials;
        protected float alpha = 1.0f;

        private bool isEnabled = false;

        public bool Enabled
        {
            get
            {
                return isEnabled;
            }
        }

        private void Start()
        {
            int materialCount = 0;

            Fader[] childrenFaders = GetComponentsInChildren<Fader>(true);
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (CanAddMaterialsFromRenderer(renderer, childrenFaders))
                {
                    materialCount += renderer.materials.Length;
                }
            }

            currentMaterials = new MaterialSettings[materialCount];

            int materialIndex = 0;
            foreach (Renderer renderer in renderers)
            {
                if (CanAddMaterialsFromRenderer(renderer, childrenFaders))
                {
                    foreach (Material rendererMaterial in renderer.materials)
                    {
                        currentMaterials[materialIndex].material = rendererMaterial;
                        currentMaterials[materialIndex].renderer = renderer;
                        currentMaterials[materialIndex].originalSourceBlend = rendererMaterial.HasProperty("_SRCBLEND") ? rendererMaterial.GetInt("_SRCBLEND") : -1;
                        currentMaterials[materialIndex].originalDestinationBlend = rendererMaterial.HasProperty("_DSTBLEND") ? rendererMaterial.GetInt("_DSTBLEND") : -1;
                        ++materialIndex;
                    }
                }
            }
        }

        protected virtual bool CanAddMaterialsFromRenderer(Renderer renderer, Fader[] faders)
        {
            if (renderer.GetComponentInParent<NoAutomaticFade>() != null)
            {
                return false;
            }

            if (faders == null)
            {
                return true;
            }

            foreach (Fader fader in faders)
            {
                if (fader == this)
                {
                    continue;
                }

                Transform testTransform = renderer.gameObject.transform;

                while (testTransform != null)
                {
                    if (fader.gameObject == testTransform.gameObject)
                    {
                        return false;
                    }

                    testTransform = testTransform.parent;
                }
            }

            return true;
        }

        protected virtual MaterialSettings[] Materials
        {
            get { return currentMaterials; }
        }

        public void EnableFade()
        {
            if (isEnabled)
            {
                return;
            }

            isEnabled = true;

            gameObject.SetActive(true);

            // switch blend mode to support transparency for clean fades
            MaterialSettings[] settings = Materials;
            if (settings != null)
            {
                foreach (MaterialSettings material in settings)
                {
                    if (material.material != null)
                    {
                        if (material.originalSourceBlend != -1)
                        {
                            material.material.SetInt("_SRCBLEND", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        }

                        if (material.originalDestinationBlend != -1)
                        {
                            material.material.SetInt("_DSTBLEND", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        }
                    }
                }
            }
        }

        public void DisableFade()
        {
            if (!isEnabled)
            {
                return;
            }

            MaterialSettings[] settings = Materials;
            if (settings != null)
            {
                foreach (MaterialSettings material in settings)
                {
                    if (material.material != null)
                    {
                        if (material.originalSourceBlend != -1)
                        {
                            material.material.SetInt("_SRCBLEND", material.originalSourceBlend);
                        }

                        if (material.originalDestinationBlend != -1)
                        {
                            material.material.SetInt("_DSTBLEND", material.originalDestinationBlend);
                        }
                    }
                }
            }

            isEnabled = false;
        }

        public void Hide()
        {
            bool enabledValue = isEnabled;

            // when hiding content, the materials need to be set up for transition alpha; however, their state remains the same
            EnableFade();
            SetAlpha(0.0f);
            isEnabled = enabledValue;
        }

        public virtual bool SetAlpha(float alphaValue)
        {
            MaterialSettings[] settings = Materials;

            if (!isEnabled || settings == null)
            {
                return false;
            }

            // if the content is hidden, we should reenable our renderers
            if ((alpha == 0.0f && alphaValue > 0.0f) ||
                (alpha > 0.0f && alphaValue == 0.0f))
            {
                bool enableRenderer = alphaValue > 0.0f;

                foreach (MaterialSettings material in settings)
                {
                    if (material.renderer != null)
                    {
                        material.renderer.enabled = enableRenderer;
                    }
                }
            }

            alpha = alphaValue;

            foreach (MaterialSettings material in settings)
            {
                if (material.material != null)
                {
                    material.material.SetFloat("_TransitionAlpha", alphaValue);
                }
            }

            return true;
        }

        public float GetAlpha()
        {
            return alpha;
        }
    }
}