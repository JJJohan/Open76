using Assets.Scripts.Entities;
using Assets.Scripts.System.Fileparsers;
using UnityEngine;

namespace Assets.Scripts
{
    public class Effect : IFixedUpdateable
    {
        private const float FrameRate = 1f / 25f; // Haven't found a consistent value in XDF that resembles framerate.
        private const float Scale = 2f; // Scale seems far too small at 1.0x
        
        private int _frameCount;
        private float _frameRate;
        private float _currentTime;
        private int _frameIndex;
        private int _effectCount;

        public bool Loop { get; set; } // Only used for debugging / XDF viewer scene.
        public bool AutoDestroy { get; set; }
        public Transform Transform { get; private set; }
        public GameObject GameObject { get; private set; }

        public Effect(GameObject gameObject)
        {
            GameObject = gameObject;
            Transform = gameObject.transform;
            UpdateManager.Instance.AddFixedUpdateable(this);
        }

        public void Initialise(Xdf xdf)
        {
            _frameRate = FrameRate;
            _frameCount = xdf.Frames;
            GameObject.SetActive(false);
        }

        public void Fire()
        {
            _currentTime = 0f;
            _frameIndex = 0;
            GameObject.SetActive(true);
            Transform.localScale = new Vector3(Scale, Scale, Scale);
						
            Car playerCar = Car.Player;
            if (playerCar != null)
            {
                Transform.LookAt(playerCar.Transform);
            }
        }
        
        public void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            _currentTime += dt;
            if (_currentTime >= _frameRate)
            {
                ++_frameIndex;
                _currentTime -= _frameRate;
            }

            if (Loop)
            {
                _frameIndex %= _frameCount;
            }
            else if (_frameIndex == _frameCount)
            {
                if (AutoDestroy)
                {
                    Destroy();
                }
                else
                {
                    GameObject.SetActive(false);
                }
            }
        }

        public void Destroy()
        {
            Object.Destroy(GameObject);
            UpdateManager.Instance.RemoveFixedUpdateable(this);
        }
    }
}
