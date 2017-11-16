// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class KeyboardAnimationHelper : MonoBehaviour
    {
        private KeyCode introKeyCode = KeyCode.I;
        private KeyCode outroKeyCode = KeyCode.O;
        private Animator animator;

        private void Start()
        {
            animator = GetComponent<Animator>();

            if (ViewLoader.Instance)
            {
                ViewLoader.Instance.CoreSystemsLoaded += CoreSystemsLoaded;
            }
        }

        private void CoreSystemsLoaded()
        {
            if (KeyboardInput.Instance)
            {
                KeyboardInput.Instance.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(introKeyCode, KeyboardInput.KeyEvent.KeyReleased), PlayIntro);
                KeyboardInput.Instance.RegisterKeyEvent(new KeyboardInput.KeyCodeEventPair(outroKeyCode, KeyboardInput.KeyEvent.KeyReleased), PlayOutro);
            }
        }

        private void PlayIntro(KeyboardInput.KeyCodeEventPair keyCodeEvent)
        {
            if (animator)
            {
                animator.SetTrigger("Intro");
            }
        }

        private void PlayOutro(KeyboardInput.KeyCodeEventPair keyCodeEvent)
        {
            if (animator)
            {
                animator.SetTrigger("Outro");
            }
        }

        private void OnDestroy()
        {
            if (KeyboardInput.Instance)
            {
                KeyboardInput.Instance.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(introKeyCode, KeyboardInput.KeyEvent.KeyReleased), PlayIntro);
                KeyboardInput.Instance.UnregisterKeyEvent(new KeyboardInput.KeyCodeEventPair(outroKeyCode, KeyboardInput.KeyEvent.KeyReleased), PlayOutro);
            }
        }
    }
}