// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using UnityEngine;

namespace GalaxyExplorer
{
    public class AlignableLogo : MonoBehaviour
    {
        private TightTagalong tightTagalong;
        private Interpolator interpolator;

        private void Awake()
        {
            tightTagalong = GetComponent<TightTagalong>();
            interpolator = GetComponent<Interpolator>();
        }

        public void LockPosition()
        {
            tightTagalong.enabled = false;
            interpolator.enabled = false;

            ViewLoader.Instance.transform.position = transform.position;
            ViewLoader.Instance.transform.rotation = transform.rotation;
        }
    }
}