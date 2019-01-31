namespace Assets.Scripts.Menus
{
    internal class OptionsMenu : BaseMenu
    {
        protected override MenuDefinition BuildMenu()
        {
            return new MenuDefinition
            {
                BackgroundFilename = "6mainmn1",
                MenuItems = new MenuItem[] {
                    new MenuButton("Abort Mission", "$6.78", Back),
                    new MenuBlank(),
                    new MenuButton("Graphic Detail", "$8.98", MenuController.Instance.ShowMenu<GraphicsMenu>),
                    new MenuButton("Audio Control", "$9.01", MenuController.Instance.ShowMenu<AudioControlMenu>),
                    new MenuBlank(),
                    new MenuButton("Continue Mission", "", Back),
                }
            };
        }

        public override void Back()
        {
            MenuController.Instance.CloseMenu();
        }
    }
}
