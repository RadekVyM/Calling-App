using CallingApp.Maui.Views.Pages;

namespace CallingApp.Maui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }
    }
}