﻿using System.Collections.Generic;
using Assets.Scripts.System.Fileparsers;
using UnityEngine;

namespace Assets.Scripts.System
{
    public class Effect : MonoBehaviour
    {
        private const float FrameRate = 1f / 25f; // Haven't found a consistent value in XDF that resembles framerate.

        private struct EffectPair
        {
            public MeshRenderer Renderer;
            public Material[] Materials;
        }

        private List<EffectPair> _effectPairs;
        private int _frameCount;
        private float _frameRate;
        private float _currentTime;
        private int _frameIndex;
        private int _effectCount;

        public bool Loop { get; set; } // Only used for debugging / XDF viewer scene.

        private void Awake()
        {
            _effectPairs = new List<EffectPair>();
        }

        public void Initialise(Xdf xdf)
        {
            _frameRate = FrameRate;
            _frameCount = xdf.Frames;
            gameObject.SetActive(false);
        }

        public void AddPart(MeshRenderer partRenderer, Material[] materials)
        {
            _effectPairs.Add(new EffectPair
            {
                Renderer = partRenderer,
                Materials = materials
            });

            ++_effectCount;
        }

        public void Fire()
        {
            _currentTime = 0f;
            _frameIndex = 0;
            UpdateMaterial();
            gameObject.SetActive(true);
        }

        private void UpdateMaterial()
        {
            for (int i = 0; i < _effectCount; ++i)
            {
                _effectPairs[i].Renderer.material = _effectPairs[i].Materials[_frameIndex];
            }
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            _currentTime += dt;
            if (_currentTime >= _frameRate)
            {
                UpdateMaterial();
                ++_frameIndex;
                _currentTime -= _frameRate;
            }

            if (Loop)
            {
                _frameIndex %= _frameCount;
            }
            else if (_frameIndex == _frameCount)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
