using CallingApp.Core.ViewModels;
using CallingApp.Maui.Views.Controls;
using SimpleToolkit.Core;

namespace CallingApp.Maui.Views.Pages;

public partial class MainPage : ContentPage
{
    const double SizePadding = 25;


    public MainPage()
    {
        BindingContext = new MainPageViewModel();

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }


    private void OnLoaded(object sender, EventArgs e)
    {
        Window.SubscribeToSafeAreaChanges(OnSafeAreaChanged);
    }

    private void OnUnloaded(object sender, EventArgs e)
    {
        Window.UnsubscribeFromSafeAreaChanges(OnSafeAreaChanged);
    }

    private void OnSafeAreaChanged(Thickness thickness)
    {
        var sideMargin = new Thickness(SizePadding + thickness.Left, 0, SizePadding + thickness.Right, 0);

        contentGrid.Padding = new Thickness(0, thickness.Top, 0, 0);
        listScrollView.Padding = new Thickness(thickness.Left, 0, thickness.Right, thickness.Bottom);
        callView.SafeArea = thickness;
        balanceValueCotnainer.Margin = sideMargin;
        balanceCotnainer.Margin = sideMargin;
        buttonsContainer.Margin = sideMargin;
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

        var animation = new Animation
        {
            { 0, 1, new Animation(v => whiteBoxView.TranslationY = v, 0, Height) },
            { 0, 1, new Animation(v => whiteGradientBoxView.TranslationY = v, 0, Height) }
        };

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

        var animation = new Animation
        {
            { 0, 1, new Animation(v => whiteBoxView.TranslationY = v, Height, 0) },
            { 0, 1, new Animation(v => whiteGradientBoxView.TranslationY = v, Height, 0) }
        };

        foreach (var hidableContentView in hidableContentViews)
        {
            hidableContentView.ShowContent();
        }

        animation.Commit(this, "ShowContentAnimation", length: 500, easing: Easing.SinIn);
    }
}