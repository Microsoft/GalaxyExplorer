// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace GalaxyExplorer
{
    [Serializable]
    public struct CursorStageImage
    {
        public CursorState mode;
        public Texture2D baseState;
        public Texture2D activatedState;
    }
}