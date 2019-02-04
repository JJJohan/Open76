using Assets.Scripts.System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menus
{
    public abstract class FullScreenMenu : IMenu
    {
        private int _cullingMask;
        private Color _clearColour;

        protected abstract string MenuName { get; }
        protected abstract string TextureName { get; }
        protected GameObject RootObject { get; private set; }

        public void Back()
        {
            MenuController.Instance.CloseMenu();
        }

        public void Close()
        {
            if (RootObject == null)
            {
                return;
            }

            UnityEngine.Camera camera = UnityEngine.Camera.main;
            camera.cullingMask = _cullingMask;
            camera.backgroundColor = _clearColour;
            Object.Destroy(RootObject);
            RootObject = null;
        }

        public virtual void Open()
        {
            if (TextureName == null)
            {
                return;
            }

            if (RootObject != null)
            {
                Close();
            }

            UnityEngine.Camera camera = UnityEngine.Camera.main;
            _clearColour = camera.backgroundColor;
            _cullingMask = camera.cullingMask;
            camera.cullingMask = 0;
            camera.backgroundColor = Color.black;
            Texture2D texture = CacheManager.Instance.GetTexture(TextureName);

            RootObject = new GameObject(MenuName);
            RawImage image = RootObject.AddComponent<RawImage>();
            image.texture = texture;

            RectTransform transform = RootObject.GetComponent<RectTransform>();
            transform.SetParent(MenuController.Instance.Transform);
            transform.anchorMin = new Vector2(0.5f, 0f);
            transform.anchorMax = new Vector2(0.5f, 1f);
            transform.anchoredPosition = new Vector2(0f, 0f);
            transform.sizeDelta = new Vector2(640f, 0f);
            transform.localScale = new Vector3(1f, -1f, 1f);
        }
    }
}
