// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using UnityEngine;

namespace GalaxyExplorer
{
    public class AboutSlate : MonoBehaviour
    {
        public Material AboutMaterial;
        public GameObject Slate;
        public float TransitionDuration = 1.0f;

        private bool visible;
        private bool wasJustShown;

        private void Awake()
        {
            DisableLinks();
            AboutMaterial.SetFloat("_TransitionAlpha", 0);

            InputRouter.Instance.Tapped += delegate
            {
                if (visible && !wasJustShown)
                {
                    Hide();
                }
            };
        }

        private IEnumerator AnimateToOpacity(float target)
        {
            var timeLeft = TransitionDuration;

            DisableLinks();
            Slate.SetActive(true);

            if (TransitionDuration > 0)
            {
                while (timeLeft > 0)
                {
                    Slate.SetActive(true);
                    AboutMaterial.SetFloat("_TransitionAlpha", Mathf.Lerp(target, 1 - target, timeLeft / TransitionDuration));
                    yield return null;

                    timeLeft -= Time.deltaTime;
                }
            }

            AboutMaterial.SetFloat("_TransitionAlpha", target);

            if (target > 0)
            {
                EnableLinks();
                Slate.SetActive(true);
            }
            else
            {
                DisableLinks();
                Slate.SetActive(false);
            }
        }

        private void EnableLinks()
        {
            var links = GetComponentsInChildren<Hyperlink>(includeInactive: true);
            foreach (var link in links)
            {
                link.gameObject.SetActive(true);
            }
        }

        private void DisableLinks()
        {
            var links = GetComponentsInChildren<Hyperlink>(includeInactive: true);
            foreach (var link in links)
            {
                link.gameObject.SetActive(false);
            }
        }

        public void Show()
        {
            if (visible)
            {
                Hide();
            }
            else
            {
                if (CardPOIManager.Instance)
                {
                    CardPOIManager.Instance.HideAllCards();
                }

                EnableLinks();

                StartCoroutine(AnimateToOpacity(1));

                visible = true;
                wasJustShown = true;
            }
        }

        public void Update()
        {
            wasJustShown = false;
        }

        public void Hide()
        {
            StartCoroutine(AnimateToOpacity(0));

            if (ToolManager.Instance)
            {
                if (ToolManager.Instance.SelectedTool && ToolManager.Instance.SelectedTool.type == ToolType.About)
                {
                    ToolManager.Instance.UnselectAllTools();
                }
            }

            visible = false;
        }
    }
}