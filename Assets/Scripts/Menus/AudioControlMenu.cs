using System;

namespace Assets.Scripts.Menus
{
    internal class AudioControlMenu : BaseMenu
    {
        protected override MenuDefinition BuildMenu()
        {
            return new MenuDefinition
            {
                BackgroundFilename = "6audcon1",
                MenuItems = new MenuItem[] {
                    new MenuBlank(),
                    new MenuButton("Music Level", "3.00", Noop),
                    new MenuBlank(),
                    new MenuBlank(),
                    new MenuButton("SFX Level", "4.00", Noop),
                    new MenuBlank(),
                    new MenuBlank(),
                    new MenuButton("Voice Level", "10.00", Noop),
                    new MenuBlank(),
                    new MenuBlank(),
                    new MenuButton("Back", "", Back)
                }
            };
        }

        private void Noop()
        {
            throw new NotImplementedException();
        }
        
        public override void Back()
        {
            MenuController.Instance.ShowMenu<OptionsMenu>();
        }
    }
}
