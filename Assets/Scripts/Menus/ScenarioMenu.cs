using System.Collections.Generic;
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

        private void Awake()
        {
            _sprites = new List<Sprite>();
            _textures = new List<Texture2D>();
            
            ShowMenu();
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