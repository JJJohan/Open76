using System;
using UnityEngine;
using UnityEngine.XR;

namespace Assets.Scripts.Menus
{
    internal class GraphicsMenu : BaseMenu
    {
        protected override MenuDefinition BuildMenu()
        {
            return new MenuDefinition
            {
                BackgroundFilename = "6grxdet1",
                MenuItems = new MenuItem[] {
                    new MenuButton("Screen Resolution", GetCurrentResolution(), NextResolution),
                    new MenuButton("Quality", GetCurrentQuality(), NextQuality),
                    new MenuBlank(),
                    new MenuButton("Virtual Reality", GetVRStatus(), ToggleVR),
                    new MenuBlank(),
                    new MenuButton("Cancel", "", Back)
                }
            };
        }

        private string GetVRStatus()
        {
            return XRSettings.enabled ? "On" : "Off";
        }

        private string GetCurrentResolution()
        {
            Resolution current = Screen.currentResolution;
            return current.width + "x" + current.height + "@" + current.refreshRate;
        }

        private string GetCurrentQuality()
        {
            int level = QualitySettings.GetQualityLevel();
            return QualitySettings.names[level];
        }

        private void NextResolution()
        {
            int nextIndex = (Array.IndexOf(Screen.resolutions, Screen.currentResolution) + 1) % Screen.resolutions.Length;
            Resolution newResolution = Screen.resolutions[nextIndex];
            Screen.SetResolution(newResolution.width, newResolution.height, false, newResolution.refreshRate);

            Open();
        }

        private void NextQuality()
        {
            int nextLevel = (QualitySettings.GetQualityLevel() + 1) % QualitySettings.names.Length;
            QualitySettings.SetQualityLevel(nextLevel);

            Open();
        }

        private void ToggleVR()
        {
            XRSettings.enabled = !XRSettings.enabled;

            Open();
        }

        public override void Back()
        {
            MenuController.Instance.ShowMenu<OptionsMenu>();
        }
    }
}
