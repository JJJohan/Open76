﻿using System.Collections.Generic;
using Assets.Scripts.System.Fileparsers;
using UnityEngine;

namespace Assets.Scripts.System
{
    public class ReferenceImage
    {
        public Texture2D MainTexture { get; set; }
        public string Name { get; private set; }

        private readonly Dictionary<string, Vector2Int> _referencePositions;

        public ReferenceImage(string name, Texture2D mainTexture, Dictionary<string, ETbl.ETblItem> etblItems)
        {
            Name = name;
            MainTexture = mainTexture;

            _referencePositions = new Dictionary<string, Vector2Int>(etblItems.Count);
            foreach (KeyValuePair<string, ETbl.ETblItem> item in etblItems)
            {
                ETbl.ETblItem itemValue = item.Value;
                Vector2Int referencePosition = new Vector2Int(itemValue.XOffset, itemValue.YOffset);
                _referencePositions.Add(item.Key, referencePosition);
            }
        }
        
        public void ApplySprite(string referenceId, I76Sprite sprite, bool uploadToGpu, bool alphaBlend = false)
        {
            if (sprite == null)
            {
                return;
            }

            Vector2Int referencePos = Vector2Int.zero;
            if (referenceId != null)
            { 
                if (!_referencePositions.TryGetValue(referenceId, out referencePos))
                {
                    Debug.LogErrorFormat("Reference ID '{0}' does not exist for reference image '{1}'.", referenceId, Name);
                    return;
                }
            }

            int xOffset = referencePos.x;
            int yOffset = Mathf.Max(0, MainTexture.height - referencePos.y - sprite.Height - 1);

            Color32[] pixels = sprite.Pixels;
            if (alphaBlend)
            {
                Color[] existingPixels = MainTexture.GetPixels(xOffset, yOffset, sprite.Width, sprite.Height);
                int pixelCount = existingPixels.Length;
                for (int i = 0; i < pixelCount; ++i)
                {
                    pixels[i] = Color32.Lerp(existingPixels[i], pixels[i], pixels[i].a);
                }
            }

            MainTexture.SetPixels32(xOffset, yOffset, sprite.Width, sprite.Height, pixels, 0);

            if (uploadToGpu)
            {
                MainTexture.Apply();
            }
        }

        public void UploadToGpu()
        {
            MainTexture.Apply();
        }
    }

    public class I76Sprite
    {
        public string Name { get; set; }
        public Color32[] Pixels { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
