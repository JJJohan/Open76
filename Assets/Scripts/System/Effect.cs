using System.Collections.Generic;
using Assets.Scripts.System.Fileparsers;
using UnityEngine;

namespace Assets.Scripts.System
{
    public class Effect : MonoBehaviour
    {
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
        private float _totalTime;
        private float _lifeTime;

        public bool Loop { get; set; }

        private void Awake()
        {
            _effectPairs = new List<EffectPair>();
        }

        public void Initialise(Xdf xdf)
        {
            _frameCount = xdf.Frames;
            _frameRate = 1f / xdf.LifeTime;
            _lifeTime = xdf.LifeTime;
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

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            _currentTime += dt;
            _totalTime += dt;
            if (_currentTime >= _frameRate)
            {
                _frameIndex = (_frameIndex + 1) % _frameCount;
                for (int i = 0; i < _effectCount; ++i)
                {
                    _effectPairs[i].Renderer.material = _effectPairs[i].Materials[_frameIndex];
                }

                _currentTime -= _frameRate;
            }

            if (!Loop && _totalTime >= _lifeTime)
            {
                Destroy(gameObject);
            }
        }
    }
}
