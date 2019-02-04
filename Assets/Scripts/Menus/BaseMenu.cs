using Assets.Scripts.System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Menus
{
    public abstract class BaseMenu : IMenu
    {
        private static Button _menuButtonPrefab;
        private static Transform _blankSeparatorPrefab;
        
        private bool _opened;
        private GameObject _rootObject;

        static BaseMenu()
        {
            _menuButtonPrefab = Resources.Load<GameObject>("Prefabs/MenuButton").GetComponent<Button>();
            _blankSeparatorPrefab = Resources.Load<GameObject>("Prefabs/BlankSeparator").transform;
        }

        protected abstract MenuDefinition BuildMenu();

        public abstract void Back();

        public void Open()
        {
            if (_opened)
            {
                Close();
            }

            MenuDefinition menuDefinition = BuildMenu();
            Transform menuTransform = MenuController.Instance.transform;

            _rootObject = new GameObject("Menu");
            RectTransform rootTransform = _rootObject.AddComponent<RectTransform>();
            rootTransform.SetParent(menuTransform);
            rootTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rootTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rootTransform.anchoredPosition = new Vector2(0f, 0f);
            rootTransform.sizeDelta = new Vector2(296f, 425f);

            GameObject itemObject = new GameObject("MenuItems");
            
            VerticalLayoutGroup layoutGroup = itemObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(5, 2, 2, 0);
            layoutGroup.spacing = 3f;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            
            GameObject backgroundObject = new GameObject("Background");
            RawImage background = backgroundObject.AddComponent<RawImage>();

            RectTransform backgroundTransform = backgroundObject.GetComponent<RectTransform>();
            backgroundTransform.SetParent(rootTransform);
            backgroundTransform.anchorMin = new Vector2(0.5f, 0.5f);
            backgroundTransform.anchorMax = new Vector2(0.5f, 0.5f);
            backgroundTransform.localScale = new Vector3(1f, -1f, 1f);

            Texture2D texture = CacheManager.Instance.GetTexture(menuDefinition.BackgroundFilename);
            background.texture = texture;
            backgroundTransform.anchoredPosition = new Vector2(0f, 0f);
            backgroundTransform.sizeDelta = new Vector2(texture.width, texture.height);
            
            RectTransform itemTransform = itemObject.GetComponent<RectTransform>();
            itemTransform.SetParent(rootTransform);
            itemTransform.anchorMin = new Vector2(0f, 0f);
            itemTransform.anchorMax = new Vector2(1f, 1f);
            itemTransform.anchoredPosition = new Vector2(10.5f, -32.5f);
            itemTransform.sizeDelta = new Vector2(-21f, -105f);

            int selectedIndex = EventSystem.current.currentSelectedGameObject == null ? 0 : EventSystem.current.currentSelectedGameObject.transform.GetSiblingIndex();

            for (int i = 0; i < menuDefinition.MenuItems.Length; i++)
            {
                MenuItem menuItem = menuDefinition.MenuItems[i];
                if (menuItem is MenuButton)
                {
                    MenuButton menuButton = menuItem as MenuButton;
                    Button button = Object.Instantiate(_menuButtonPrefab, itemTransform);
                    button.transform.Find("TextContainer").GetComponentInChildren<Text>().text = menuButton.Text;
                    button.transform.Find("Value").GetComponent<Text>().text = menuButton.Value;
                    button.onClick.AddListener(new UnityEngine.Events.UnityAction(menuButton.OnClick));

                    if (selectedIndex == i)
                        EventSystem.current.SetSelectedGameObject(button.gameObject);
                }
                else if (menuItem is MenuBlank)
                {
                    Object.Instantiate(_blankSeparatorPrefab, itemTransform);
                }
            }

            _opened = true;
        }

        public void Close()
        {
            if (!_opened)
            {
                return;
            }

            Object.Destroy(_rootObject);
            _rootObject = null;
            _opened = false;
        }
    }
}
