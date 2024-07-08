namespace CallingApp.Maui.Views.Controls;

// TODO: When issue https://github.com/dotnet/maui/issues/8050 is fixed, use ContentView instead
public class HidableContentView : Grid
{
    private bool shown = true;

    public HidableContentView()
    {
        IsClippedToBounds = true;
        SizeChanged += HidableContentViewSizeChanged;
    }

    private void HidableContentViewSizeChanged(object sender, EventArgs e)
    {
        foreach (var child in Children)
        {
            if (child is View view)
                view.TranslationY = shown ? 0 : view.Height;
        }
    }

    public void HideContent()
    {
        var animation = new Animation();
        foreach (var child in Children)
        {
            if (child is View view)
                animation.Add(0, 1, new Animation(v => view.TranslationY = v, 0, view.Height));
        }
        animation.Commit(this, "HideContentAnimation", length: 500, easing: Easing.SinIn);

        shown = false;
    }

    public void ShowContent()
    {
        var animation = new Animation();
        foreach (var child in Children)
        {
            if (child is View view)
                animation.Add(0, 1, new Animation(v => view.TranslationY = v, view.Height, 0));
        }
        animation.Commit(this, "ShowContentAnimation", length: 500, easing: Easing.SinIn);

        shown = true;
    }
}