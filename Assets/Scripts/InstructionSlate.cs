// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class InstructionSlate : MonoBehaviour
    {
        public enum InstructionText
        {
            AppDescription,
            Developers,
            Community
        }

        public Material AppDescriptionMaterial;
        public Material DevelopersMaterial;
        public Material CommunityMaterial;
        public MeshRenderer slateRenderer;

        private Animator animator;
        private bool isShown = false;
        private Material nextInstruction;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            transform.localScale = transform.localScale * MyAppPlatformManager.SlateScaleFactor;
        }

        public void DisplayMessage(InstructionText text)
        {
            switch (text)
            {
                case InstructionText.AppDescription:
                    nextInstruction = AppDescriptionMaterial;
                    break;
                case InstructionText.Developers:
                    nextInstruction = DevelopersMaterial;
                    break;
                case InstructionText.Community:
                    nextInstruction = CommunityMaterial;
                    break;
            }

            if (isShown)
            {
                ChangeText();
            }
            else
            {
                slateRenderer.material = nextInstruction;
                nextInstruction = null;
                Show();
            }
        }

        public void InstructionMaterialSwap()
        {
            if (nextInstruction)
            {
                slateRenderer.material = nextInstruction;
                nextInstruction = null;
            }
        }

        public void Show()
        {
            slateRenderer.material.SetFloat("TransitionAlpha", 0.0f);
            animator.SetTrigger("Show");
            isShown = true;
        }

        public void Hide()
        {
            animator.SetTrigger("Hide");
            isShown = false;
        }

        public void ChangeText()
        {
            animator.SetTrigger("ChangeText");
        }
    }
}