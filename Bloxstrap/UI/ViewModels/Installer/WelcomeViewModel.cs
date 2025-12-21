namespace Bloxstrap.UI.ViewModels.Installer
{
    public class WelcomeViewModel : NotifyPropertyChangedViewModel
    {
        // formatting is done here instead of in xaml, it's just a bit easier
        public string MainText => String.Format(
            Strings.Installer_Welcome_MainText,
            "[github.com/ItzBloxxy/BubbleStrap](https://github.com/ItzBloxxy/BubbleStrap)"
        );

        public bool CanContinue { get; set; } = false;
    }
}
