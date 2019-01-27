using System.Linq;
using Assets.Scripts.System;
using Assets.Scripts.System.Fileparsers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class XdfViewer : MonoBehaviour
    {
        public string GamePath;

        private GameObject _currentObject;
        private CacheManager _cacheManager;

        public Transform ButtonPrefab;
        public Transform ListTarget;

        private void Awake()
        {
            Game.Instance.GamePath = GamePath;

            _cacheManager = CacheManager.Instance;
            _cacheManager.Palette = ActPaletteParser.ReadActPalette("t01.act");

            string[] xdfFiles = VirtualFilesystem.Instance.FindAllWithExtension(".xdf").ToArray();

            foreach (string xdfFile in xdfFiles)
            {
                string filename = xdfFile;
                Button button = Instantiate(ButtonPrefab, Vector3.zero, Quaternion.identity, ListTarget).GetComponent<Button>();
                button.GetComponentInChildren<Text>().text = filename;
                button.onClick.AddListener(() =>
                {
                    OnClickButton(filename);
                });
            }
        }

        private void OnClickButton(string filename)
        {
            LoadXDF(filename);
        }

        private void LoadXDF(string filename)
        {
            if (_currentObject != null)
                Destroy(_currentObject);

            _currentObject = _cacheManager.ImportXdf(filename, transform, true);
            _cacheManager.ClearCache();
        }
    }
}