using Android.Views;
using Xamarin.Forms;

[assembly: Dependency(typeof(CallingApp.Droid.StatusBar))]
namespace CallingApp.Droid
{
    public class StatusBar : IStatusBar
    {
        public int GetHeight()
        {
            int statusBarHeight = -1;
            int resourceId = Xamarin.Essentials.Platform.CurrentActivity.Resources.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                statusBarHeight = Xamarin.Essentials.Platform.CurrentActivity.Resources.GetDimensionPixelSize(resourceId);
            }
            return statusBarHeight;
        }

        public void SetLightStatusBar(bool light)
        {
            int uiOptions = (int)Xamarin.Essentials.Platform.CurrentActivity.Window.DecorView.SystemUiVisibility;

            if (light)
                uiOptions |= (int)SystemUiFlags.LightStatusBar;
            else
                uiOptions &= ~(int)SystemUiFlags.LightStatusBar;

            Xamarin.Essentials.Platform.CurrentActivity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
        }
    }
}