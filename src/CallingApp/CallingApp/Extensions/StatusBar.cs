using Xamarin.Forms;

namespace CallingApp
{
    public static class StatusBar
    {
        public static double Height
        {
            get
            {
                IStatusBar statusBar = DependencyService.Get<IStatusBar>();
                double height = 0;

                if (statusBar != null)
                    height = statusBar.GetHeight() / Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;

                return height;
            }
        }

        public static Thickness Padding => new Thickness(0, Height, 0, 0);
    }
}
