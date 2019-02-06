using Assets.Scripts.Camera;
using Assets.Scripts.System;
using Assets.Scripts.System.Fileparsers;
using UnityEngine;

namespace Assets.Scripts
{
    public class Sky : IUpdateable
    {
        private const float Height = 10f;
        private const float Speed = 0.02f;

        private Material _material;
        private Transform _transform;

        private string _textureFileName;
        public string TextureFilename
        {
            get { return _textureFileName; }
            set
            {
                if (_textureFileName == value)
                {
                    return;
                }

                _textureFileName = value;
                _material.mainTexture = TextureParser.ReadMapTexture(TextureFilename, CacheManager.Instance.Palette);
            }
        }

        public Sky()
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            obj.name = "Sky";
            _transform = obj.transform;
            _transform.localScale = new Vector3(1000f, 1f, 1000f);
            _transform.rotation = Quaternion.Euler(180f, 0f, 0f);

            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            _material = new Material(Resources.Load<Shader>("Shaders/UnlitSky"));
            _material.mainTextureScale = new Vector2(100f, 100f);
            renderer.material = _material;

            if (!string.IsNullOrEmpty(TextureFilename))
            {
                _material.mainTexture = TextureParser.ReadMapTexture(TextureFilename, CacheManager.Instance.Palette);
            }

            UpdateManager.Instance.AddUpdateable(this);
        }

        public void Update()
        {
            _material.mainTextureOffset += new Vector2(Speed, 0f) * Time.deltaTime;
            _transform.position = CameraManager.Instance.ActiveCamera.transform.position + Vector3.up * Height;
        }
        
        public void Destroy()
        {
            UpdateManager.Instance.RemoveUpdateable(this);
            if (_transform != null)
            {
                Object.Destroy(_transform.gameObject);
            }
        }
    }
}