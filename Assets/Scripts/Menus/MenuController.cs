using Assets.Scripts.Camera;
using Assets.Scripts.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Menus
{
    public class MenuController : MonoBehaviour
    {
        private IMenu _currentMenu;

        public static MenuController Instance { get; private set; }

        public Transform Transform { get; private set; }

        // Use this for initialization
        private void Awake()
        {
            Instance = this;
            Transform = transform;
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_currentMenu != null)
                {
                    _currentMenu.Back();
                }
                else
                {
                    ShowMenu<OptionsMenu>();
                }
            }
            
            if (!CameraManager.Instance.IsMainCameraActive || Car.Player == null || !Car.Player.Alive)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                if (_currentMenu == null)
                {
                    ShowMenu<MapScreen>();
                }
                else if (_currentMenu is MapScreen)
                {
                    CloseMenu();
                }
            }

        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void CloseMenu()
        {
            if (_currentMenu != null)
            {
                _currentMenu.Close();
                _currentMenu = null;
            }

            Game.Instance.Paused = false;
        }

        public void ShowMenu<T>() where T : IMenu, new()
        {
            if (_currentMenu != null)
            {
                _currentMenu.Close();
            }

            _currentMenu = new T();
            EventSystem.current.SetSelectedGameObject(null);

            _currentMenu.Draw();

            Game.Instance.Paused = true;
        }
    }
}