using CallingApp.Core;
using CallingApp.Maui.Views.Controls;

namespace CallingApp.Maui.Views.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new MainPageViewModel();
        }

        private async void CallButtonClicked(object sender, EventArgs e)
        {
            await Task.Delay(100);
            HideAllViews();

            await Task.Delay(200);

            await callView.Call();
        }

        private void HideAllViews()
        {
            var hidableContentViews = GetAllHidableContentViews(contentGrid);

            Animation animation = new Animation();

            animation.Add(0, 1, new Animation(v => whiteBoxView.TranslationY = v, 0, Height));
            animation.Add(0, 1, new Animation(v => whiteGradientBoxView.TranslationY = v, 0, Height));

            foreach (var hidableContentView in hidableContentViews)
            {
                hidableContentView.HideContent();
            }

            animation.Commit(this, "HideContentAnimation", length: 500, easing: Easing.SinIn);
        }

        private IEnumerable<HidableContentView> GetAllHidableContentViews(IView view)
        {
            var hidableContentViews = new List<HidableContentView>();

            if (view is null)
                return hidableContentViews;

            switch (view)
            {
                case HidableContentView hidableContentView:
                    hidableContentViews.Add(hidableContentView);
                    foreach (var child in hidableContentView.Children)
                        hidableContentViews.AddRange(GetAllHidableContentViews(child));
                    break;
                case IBindableLayout layout:
                    foreach (var child in layout.Children)
                        hidableContentViews.AddRange(GetAllHidableContentViews(child as IView));
                    break;
                case IContentView contenView:
                    hidableContentViews.AddRange(GetAllHidableContentViews(contenView.Content as IView));
                    break;
            }

            return hidableContentViews;
        }

        public void ShowAllViews()
        {
            var hidableContentViews = GetAllHidableContentViews(contentGrid);

            Animation animation = new Animation();

            animation.Add(0, 1, new Animation(v => whiteBoxView.TranslationY = v, Height, 0));
            animation.Add(0, 1, new Animation(v => whiteGradientBoxView.TranslationY = v, Height, 0));

            foreach (var hidableContentView in hidableContentViews)
            {
                hidableContentView.ShowContent();
            }

            animation.Commit(this, "ShowContentAnimation", length: 500, easing: Easing.SinIn);
        }
    }
}
