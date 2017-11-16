// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using UnityEngine;

namespace GalaxyExplorer
{
    /// <summary>
    /// Should match StarVertDescriptor.cginc
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [System.Serializable]
    public struct StarVertDescriptor
    {
        public static int StructSize =
            (sizeof(float) * 4) + // yOffset, curveOffset, ellipseDistance, ellipseOffset
            (sizeof(float) * 3) + // color
            (sizeof(float) * 2) + // uv
            sizeof(float) + // size
            sizeof(float); // random

        public float yOffset;
        public float curveOffset;
        public float ellipseDistance;
        public float ellipseOffset;
        public Vector3 color;
        public Vector2 uv;
        public float size;
        public float random;
    }
}