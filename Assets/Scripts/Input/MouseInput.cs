// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public sealed class MouseInput : MonoBehaviour
    {
        private void Start()
        {
            if (PlayerInputManager.Instance == null)
            {
                Debug.LogError("No PlayerInputManager available. Disabling");
                enabled = false;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                PlayerInputManager.Instance.TriggerTapPress();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                PlayerInputManager.Instance.TriggerTapRelease();
            }
        }
    }
}