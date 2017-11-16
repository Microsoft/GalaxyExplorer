// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using UnityEngine;

namespace GalaxyExplorer
{
    public class ButtonSlateConnection : MonoBehaviour
    {
        public AboutSlate Slate;

        private Tagalong tagalong;
        private float distance = 2.0f;

        private void Start()
        {
            if (Slate)
            {
                tagalong = Slate.gameObject.GetComponent<Tagalong>();

                if (tagalong)
                {
                    distance = tagalong.TagalongDistance;
                }
            }
        }

        public void Show()
        {
            Slate.gameObject.SetActive(true);

            Vector3 headForward = Camera.main.transform.forward;
            headForward.y = 0;
            headForward.Normalize();

            Slate.gameObject.transform.position = Camera.main.transform.position + (headForward * distance);

            Slate.Show();
        }

        public void Hide()
        {
            Slate.Hide();
        }
    }
}