// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using UnityEngine;

namespace GalaxyExplorer
{
    public class StarBackgroundManager : SingleInstance<StarBackgroundManager>
    {
        public float FadeInOutTime = 1.0f;
        public AnimationCurve StarBackgroundFadeCurve;
        public GameObject Stars;

        private void Start()
        {
            gameObject.SetActive(
                //PlayspaceManager.Instance.useFakeFloor ||
                (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD)
            );
            TransitionManager.Instance.ViewVolume.GetComponent<PlacementControl>().ContentPlaced += UpdateShaderProperties;
            TransitionManager.Instance.ContentLoaded += UpdateShaderProperties;
            ToolManager.Instance.ContentZoomChanged += UpdateShaderProperties;
        }

        private void UpdateShaderProperties()
        {
            GameObject currentContent = ViewLoader.Instance.GetCurrentContent();
            if (currentContent)
            {
                SceneSizer sceneSizer = currentContent.GetComponent<SceneSizer>();
                if (sceneSizer)
                {
                    float scalar = sceneSizer.GetScalar();
                    Vector3 contentWP = currentContent.transform.position;
                    Renderer renderer = GetComponentInChildren<Renderer>();
                    if (renderer)
                    {
                        Material mat = renderer.sharedMaterial;
                        if (mat)
                        {
                            mat.SetFloat("_ContentRadius", scalar);
                            mat.SetVector("_ContentWorldPos", contentWP);
                        }
                    }
                }
            }
        }

        public void FadeInOut(bool fadeIn)
        {
            if (fadeIn)
            {
                gameObject.SetActive(true);
            }
            StartCoroutine(TransitionManager.Instance.FadeContent(
                Stars,
                fadeIn ? TransitionManager.FadeType.FadeIn : TransitionManager.FadeType.FadeOut,
                Instance.FadeInOutTime,
                Instance.StarBackgroundFadeCurve));
        }
    }
}