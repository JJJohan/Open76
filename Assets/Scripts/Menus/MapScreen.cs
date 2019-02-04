using UnityEngine;

namespace Assets.Scripts.Menus
{
    public class MapScreen : FullScreenMenu
    {
        private const string OpenMapSound = "cmap2.gpw";
        private const string SpeechSound = "gdsgc26.gpw";

        protected override string MenuName
        {
            get { return "Map Screen"; }
        }

        protected override string TextureName
        {
            get
            {
                string mapFileName = Game.Instance.MapFileName;
                if (mapFileName == null)
                {
                    Debug.LogError("No map file name currently set.");
                    return null;
                }

                return mapFileName;
            }
        }

        public override void Open()
        {
            base.Open();

            SceneRoot.Instance.PlayUiSound(OpenMapSound);
            SceneRoot.Instance.PlayUiSound(SpeechSound);
        }
    }
}
