// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace GalaxyExplorer
{
    public class ScrollMaterialTexture : MonoBehaviour
    {
        public Vector2 direction;
        public float speed;

        public Material material;
        public string textureName;

        private float currentOffset;
        private Vector2 originalTextureOffset;

        private void Awake()
        {
            if (material)
            {
                originalTextureOffset = material.GetTextureOffset(textureName);
            }
        }

        private void Update()
        {
            currentOffset += Time.deltaTime * speed;

            if (material)
            {
                material.SetTextureOffset(textureName, new Vector2((currentOffset * direction.x) % 1.0f, (currentOffset * direction.y) % 1.0f));
            }
        }

        private void OnDestroy()
        {
            if (material)
            {
                material.SetTextureOffset(textureName, originalTextureOffset);
            }
        }
    }
}