// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;

namespace GalaxyExplorer
{
    public class SolarSystemResizer : SingleInstance<SolarSystemResizer>
    {
        void Awake()
        {
            transform.localScale = transform.localScale * MyAppPlatformManager.SolarSystemScaleFactor;
        }
    }
}