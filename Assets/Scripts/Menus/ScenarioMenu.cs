using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.System;
using Assets.Scripts.System.Fileparsers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Menus
{
    public class ScenarioMenu : MonoBehaviour
    {
        private List<Sprite> _sprites;
        private List<Texture2D> _textures;
        private int _selectedLevelIndex;
        private Vector2 _mousePos;
        private Text _selectedLevelText;
        private Text[] _levelListText;
        private Dictionary<int, string> _levelIndexLookup;
        private int _levelListIndex;

        private static readonly Rect CancelRect = Rect.MinMaxRect(331f, 412f, 606f, 450f);
        private static readonly Rect EnterAreaRect = Rect.MinMaxRect(-176f, 412f, 99f, 450f);
        private static readonly Rect ScrollUpRect = Rect.MinMaxRect(-594f, -210, -574f, -190f);
        private static readonly Rect ScrollDownRect = Rect.MinMaxRect(-594f, 32f, -574f, 52f);
        private static readonly Rect RenameRect = Rect.MinMaxRect(-285f, -375f, -206f, -351f);
        private static readonly Rect MakeModelRect = Rect.MinMaxRect(565f, 313f, 587f, 341f);
        private static readonly Rect VariantRect = Rect.MinMaxRect(228f, 352f, 253f, 380f);
        private static readonly Rect ConfigureRect = Rect.MinMaxRect(267f, 354f, 580f, 384f);
        private static readonly Rect LevelListRect = Rect.MinMaxRect(-535f, -176f, -201f, 22f);

        public GameObject CanvasObject;
        
        private enum Images
        {
            Options_Highlight,
            Options_Normal,
            AutoMelee_Highlight,
            AutoMelee_Normal,
            Melee_Highlight,
            Melee_Normal,
            Host_Highlight,
            Host_Normal,
            InstantMelee_Highlight,
            InstantMelee_Normal,
            Join_Highlight,
            Join_Normal,
            Modem_Highlight,
            Modem_Normal,
            Ipx_Highlight,
            Ipx_Normal,
            NullModem_Highlight,
            NullModem_Normal,
            Scenario_Highlight,
            Scenario_Normal,
            LoadBookmark_Highlight,
            LoadBookmark_Normal,
            NewTrip_Highlight,
            NewTrip_Normal,
            Trip_Highlight,
            Trip_Normal,
            Internet_Highlight,
            Internet_Normal,
            MultiMelee_Highlight,
            MultiMelee_Normal,
            Exit_Highlight,
            Exit_Normal,
            Training_Highlight,
            Training_Normal,
            Trip_SubText,
            NewTrip_SubText,
            LoadBookmark_SubText,
            Melee_SubText,
            AutoMelee_SubText,
            Unused1,
            Unused2,
            MultiMelee_SubText,
            Options_SubText,
            Exit_SubText,
            Training_SubText,
            Other_Highlight,
            Other_Normal
        }

        private Text AddText(int x, int y, string displayText)
        {
            GameObject textObject = new GameObject("Text");
            Text text = textObject.AddComponent<Text>();
            text.text = displayText;
            text.font = Game.Instance.Font;
            text.color = Color.black;

            RectTransform textTransform = textObject.GetComponent<RectTransform>();
            textTransform.SetParent(CanvasObject.transform);
            textTransform.anchorMin = new Vector2(0.5f, 0.5f);
            textTransform.anchorMax = new Vector2(0.5f, 0.5f);
            textTransform.pivot = new Vector2(0.0f, 1.0f);
            textTransform.sizeDelta = new Vector2(150f, 20f);
            textTransform.anchoredPosition = new Vector2(x, y);

            return text;
        }

        private void AddBackground(Texture2D texture, int x, int y)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            GameObject imageObj = new GameObject("Background");
            Image image = imageObj.AddComponent<Image>();
            image.sprite = sprite;

            RectTransform rectTransform = image.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(texture.width, texture.height);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(x, y);
            rectTransform.SetParent(CanvasObject.transform);

            _sprites.Add(sprite);
        }

        private void SelectLevel(int index)
        {
            _selectedLevelIndex = index;
            _selectedLevelText.text = _levelIndexLookup[index];
        }

        private void Awake()
        {
            _sprites = new List<Sprite>();
            _textures = new List<Texture2D>();
            _levelIndexLookup = new Dictionary<int, string>();

            int levelIndex = 0;
            foreach (string levelName in MsnMissionParser.ScenarioMissionLookup.Keys)
            {
                _levelIndexLookup.Add(levelIndex++, levelName);
            }

            ShowMenu();

            AddText(-307, 195, Game.Instance.PlayerName); // Player Name (Left)
            AddText(-30, 183, Game.Instance.PlayerName); // Player Name
            _selectedLevelText = AddText(-276, 130, ""); // Selected Level

            _levelListText = new Text[6];
            const int listSpacing = 17;
            int levelListYOffset = 91;
            for (int i = 0; i < 6; ++i)
            {
                _levelListText[i] = AddText(-276, levelListYOffset, _levelIndexLookup[i]);
                levelListYOffset -= listSpacing;
            }

            SelectLevel(2); // The original game defaults to the third scenario.
        }

        private bool DetectButton(Rect rect, Action onPress)
        {
            if (rect.Contains(_mousePos))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    onPress();
                }

                return true;
            }

            return false;
        }

        private void LoadSelectedLevel()
        {
            string levelName = _levelIndexLookup[_selectedLevelIndex];
            string missionFileName = MsnMissionParser.ScenarioMissionLookup[levelName];
            StartCoroutine(LevelLoader.Instance.LoadLevel(missionFileName));
        }

        private void Cancel()
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        private void ScrollLevelListUp()
        {
            if (_levelListIndex <= 0)
            {
                return;
            }

            --_levelListIndex;
            RefreshLevelList();
        }

        private void ScrollLevelListDown()
        {
            if (_levelListIndex >= 6)
            {
                return;
            }

            ++_levelListIndex;
            RefreshLevelList();
        }

        private void RefreshLevelList()
        {
            for (int i = 0; i < 6; ++i)
            {
                int levelIndex = _levelListIndex + i;
                string text = "";
                if (levelIndex < 7)
                {
                    text = _levelIndexLookup[levelIndex];
                }

                _levelListText[i].text = text;
            }
        }

        private void SelectLevel()
        {
            float top = LevelListRect.yMin;
            float bottom = LevelListRect.yMax;
            float range = bottom - top;
            float relativeMouseY = range - (bottom - _mousePos.y);
            int index = _levelListIndex + Mathf.Abs(Mathf.FloorToInt((relativeMouseY / range) * 6f));

            if (index < 7 && _selectedLevelIndex != index)
            {
                _selectedLevelIndex = index;
                _selectedLevelText.text = _levelIndexLookup[index];
            }
        }

        private void Update()
        {
            _mousePos = Input.mousePosition;
            _mousePos.x -= Screen.width / 2;
            _mousePos.y = (Screen.height - _mousePos.y) - Screen.height / 2;

            // Debug
            if (Input.GetKeyDown(KeyCode.Z))
            {
                Debug.Log($"relative mouse pos - X: {_mousePos.x} Y: {_mousePos.y}");
            }

            // Enter Area button
            if (DetectButton(EnterAreaRect, LoadSelectedLevel))
            {
                return;
            }
            
            // Level List
            if (DetectButton(LevelListRect, SelectLevel))
            {
                return;
            }

            // Cancel button
            if (DetectButton(CancelRect, Cancel))
            {
                return;
            }

            // Scroll up
            if (DetectButton(ScrollUpRect, ScrollLevelListUp))
            {
                return;
            }

            // Scroll down
            if (DetectButton(ScrollDownRect, ScrollLevelListDown))
            {
                return;
            }
            
            // Rename button
            if (DetectButton(RenameRect, null))
            {
                return;
            }

            // Make / Model button
            if (DetectButton(MakeModelRect, null))
            {
                return;
            }

            // Variant button
            if (DetectButton(VariantRect, null))
            {
                return;
            }

            // Configure button
            if (DetectButton(ConfigureRect, null))
            {
                return;
            }
        }

        private void OnDestroy()
        {
            int spriteCount = _sprites.Count;
            for (int i = 0; i < spriteCount; ++i)
            {
                Destroy(_sprites[i]);
            }

            int textureCount = _textures.Count;
            for (int i = 0; i < textureCount; ++i)
            {
                Destroy(_textures[i]);
            }

            _sprites.Clear();
            _textures.Clear();
        }

        private void OnCancel()
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }

        private void ShowMenu()
        {
            Texture2D menuBackground = Mw2Parser.GetBackground(Mw2Parser.Background.Scenario);

            if (menuBackground == null)
            {
                return;
            }

            AddBackground(menuBackground, Screen.width / 2, Screen.height / 2);
            _textures.Add(menuBackground);
        }
    }
}