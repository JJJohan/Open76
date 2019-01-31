using Assets.Scripts.System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menus
{
    public class MapScreen : IMenu
    {
        private GameObject _rootObject;
        private GameObject _backgroundObject;

        public void Back()
        {
            MenuController.Instance.CloseMenu();
        }

        public void Close()
        {
            if (_rootObject == null)
            {
                return;
            }

            Object.Destroy(_rootObject);
            Object.Destroy(_backgroundObject);
            _rootObject = null;
            _backgroundObject = null;
        }

        public void Draw()
        {
            string mapFileName = Game.Instance.MapFileName;
            if (mapFileName == null)
            {
                Debug.LogError("No map file name currently set.");
                return;
            }

            if (_rootObject != null)
            {
                Close();
            }

            Texture2D mapTexture = CacheManager.Instance.GetTexture(mapFileName);

            _backgroundObject = new GameObject("Background");
            Image backgroundImage = _backgroundObject.AddComponent<Image>();
            backgroundImage.color = Color.black;
            RectTransform backgroundtransform = _backgroundObject.GetComponent<RectTransform>();
            backgroundtransform.SetParent(MenuController.Instance.Transform);
            backgroundtransform.anchorMin = new Vector2(0f, 0f);
            backgroundtransform.anchorMax = new Vector2(1f, 1f);
            backgroundtransform.anchoredPosition = new Vector2(0f, 0f);
            backgroundtransform.sizeDelta = new Vector2(0f, 0f);
            backgroundtransform.localScale = new Vector3(1f, 1f, 1f);

            _rootObject = new GameObject("Map Screen");
            RawImage mapImage = _rootObject.AddComponent<RawImage>();
            mapImage.texture = mapTexture;

            RectTransform transform = _rootObject.GetComponent<RectTransform>();
            transform.SetParent(MenuController.Instance.Transform);
            transform.anchorMin = new Vector2(0.5f, 0f);
            transform.anchorMax = new Vector2(0.5f, 1f);
            transform.anchoredPosition = new Vector2(0f, 0f);
            transform.sizeDelta = new Vector2(640f, 0f);
            transform.localScale = new Vector3(1f, -1f, 1f);
        }
    }
}
