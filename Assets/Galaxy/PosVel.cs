// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    [System.Serializable]
    public struct PosVel
    {
        public Vector3 position;
        public Vector2 uv;
        public float size;
        public Color color;
        public float curveOffset;
        public float ellipseOffset;
        public float ellipseDistance;
    }
}