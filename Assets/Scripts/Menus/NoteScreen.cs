using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menus
{
    public class NoteScreen : FullScreenMenu
    {
        private const string NotesTexture = "zhpd6.map";
        private const float StartHeightOffset = -375f;
        private const float LeftOffset = 90f;
        private const float Spacing = 30f;


        protected override string MenuName
        {
            get { return "Note Screen"; }
        }

        protected override string TextureName
        {
            get
            {
                return NotesTexture;
            }
        }

        public override void Open()
        {
            base.Open();

            List<Game.Objective> objectives = Game.Instance.GetVisibleObjectives();
            int objectiveCount = objectives.Count;

            float heightOffset = StartHeightOffset;
            for (int i = 0; i < objectiveCount; ++i)
            {
                Game.Objective objective = objectives[i];

                GameObject textObject = new GameObject("Objective");
                Text text = textObject.AddComponent<Text>();
                text.text = objective.ObjectiveText;
                text.font = Game.Instance.Font;
                text.color = Color.black;
                text.fontSize = 18;

                RectTransform transform = textObject.GetComponent<RectTransform>();
                transform.SetParent(RootObject.transform);
                transform.anchorMin = new Vector2(0f, 1f);
                transform.anchorMax = new Vector2(0f, 1f);
                transform.pivot = new Vector2(0f, 0.5f);
                transform.anchoredPosition = new Vector2(LeftOffset, heightOffset);
                transform.sizeDelta = new Vector2(640f, 22f);

                if (objective.Completed)
                {
                    GameObject lineObject = new GameObject("Strikethrough");
                    Image image = lineObject.AddComponent<Image>();
                    image.color = Color.black;

                    transform = lineObject.GetComponent<RectTransform>();
                    transform.SetParent(RootObject.transform);
                    transform.anchorMin = new Vector2(0f, 1f);
                    transform.anchorMax = new Vector2(0f, 1f);
                    transform.pivot = new Vector2(0f, 0.5f);
                    transform.anchoredPosition = new Vector2(LeftOffset, heightOffset);
                    transform.sizeDelta = new Vector2(text.preferredWidth, 1f);
                }

                heightOffset += Spacing;
            }
        }
    }
}
