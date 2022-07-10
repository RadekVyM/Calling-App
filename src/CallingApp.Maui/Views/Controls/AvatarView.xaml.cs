using CallingApp.Core;

namespace CallingApp.Maui.Views.Controls
{
    public partial class AvatarView : ContentView
    {
        bool initImageGridSizeChanged = true;
        bool isPhonePickedUp = false;

        Point imageCenterPosition => new Point((Width - imageGrid.WidthRequest) / 2, (Height - imageGrid.HeightRequest) / 2);
        Point imageTopPosition = new Point(0, 0);
        Point topLabelPosition = new Point(0, 0);
        double topImageSize => 20;
        double topImageScale => topImageSize / imageGrid.Width;

        TopLabelDrawable topLabelDrawable;

        public AvatarView()
        {
            InitializeComponent();

            App.Current.Resources.TryGetValue("OverlayColor", out object overlayColor);
            topLabelDrawable = new TopLabelDrawable(17, overlayColor as Color);

            topLabelDrawable.TextSizeUpdated += TopLabelDrawableTextSizeUpdated;

            topLabelGraphicsView.Drawable = topLabelDrawable;
            topLabelGraphicsView.Invalidate();
        }

        private void TopLabelDrawableTextSizeUpdated()
        {
            if (BindingContext is Person person)
            {
                topLabelDrawable.Text = person.Name;
                topLabelGraphicsView.Invalidate();

                UpdateSizes();
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext is Person person)
            {
                topLabelDrawable.Text = person.Name;
                topLabelGraphicsView.Invalidate();

                UpdateSizes();
            }
        }

        private void ImageGridSizeChanged(object sender, EventArgs e)
        {
            if (initImageGridSizeChanged && imageGrid.Height != 0)
            {
                IsVisible = false;
                initImageGridSizeChanged = false;
            }
            
            UpdateSizes();
        }

        private void ContentViewSizeChanged(object sender, EventArgs e)
        {
            UpdateSizes();

            if (isPhonePickedUp)
                imageGrid.TranslationY = imageTopPosition.Y - imageGrid.Y;
        }

        private void UpdateSizes()
        {
            double spacing = 8;
            double verticalCenter = StatusBar.Height + 15;
            double totalWidth = topImageSize + topLabelDrawable.TextSize.Width + spacing;

            imageTopPosition = new Point(((Width - totalWidth) / 2) - ((imageGrid.Width - topImageSize) / 2), verticalCenter - (topImageSize / 2) - ((imageGrid.Height - topImageSize) / 2));
            topLabelPosition = new Point(((Width - totalWidth) / 2) + topImageSize + spacing, verticalCenter - (topLabelDrawable.TextSize.Height / 2));

            AbsoluteLayout.SetLayoutBounds(imageGrid, new Rect(imageCenterPosition, imageGrid.Bounds.Size)); // This works on Windows but does not work on Android
            imageGrid.Layout(new Rect(imageCenterPosition, imageGrid.Bounds.Size)); // This works on Android but does not work on Windows

            topLabelDrawable.TextPosition = topLabelPosition;
            if (!isPhonePickedUp)
                topLabelDrawable.TextTranslationX = -topLabelDrawable.TextSize.Width;
            topLabelGraphicsView.Invalidate();
        }

        public async Task OnCalled()
        {
            IsVisible = true;

            await Task.Delay(100);

            var animation = new Animation();

            animation.Add(0, 0.6, new Animation(v => imageGrid.Opacity = v, 0, 1));
            animation.Add(0, 1, new Animation(v => imageGrid.Scale = v, 1.5, 1));
            animation.Add(0, 1, new Animation(v => centerLabel.Opacity = v, 0, 1));

            animation.Commit(this, "ShowAnimation", length: 700);

            await Task.Delay(700);
        }

        public async Task OnPhonePickedUp()
        {
            topLabelDrawable.TextOpacity = 1;
            topLabelDrawable.TextTranslationX = -topLabelDrawable.TextSize.Width;
            topLabelGraphicsView.Invalidate();

            var animation = new Animation();

            animation.Add(0.5, 1, new Animation(v => {
                topLabelDrawable.TextTranslationX = (float)v * topLabelDrawable.TextTranslationX;
                topLabelGraphicsView.Invalidate();
            }, 1, 0));
            animation.Add(0, 0.8, new Animation(v => centerLabel.Opacity = v, 1, 0));
            animation.Add(0, 1, new Animation(v => imageGrid.Scale = v, imageGrid.Scale, topImageScale));
            animation.Add(0, 1, new Animation(v => imageGrid.TranslationX = v * (imageTopPosition.X - imageGrid.X), 0, 1));
            animation.Add(0, 1, new Animation(v => imageGrid.TranslationY = v * (imageTopPosition.Y - imageGrid.Y), 0, 1));

            animation.Commit(this, "TranslationAnimation", length: 700);

            await Task.Delay(700);
            isPhonePickedUp = true;
        }

        public async Task OnPhoneHangedUp(bool isOnThePhone)
        {
            isPhonePickedUp = false;

            var animation = new Animation();

            animation.Add(0.4, 1, new Animation(v => imageGrid.Opacity = v, 1, 0));

            if (isOnThePhone)
            {
                animation.Add(0.5, 1, new Animation(v => {
                    topLabelDrawable.TextTranslationX = (float)v;
                    topLabelGraphicsView.Invalidate();
                }, 0, -topLabelDrawable.TextSize.Width));
            }
            else
            {
                animation.Add(0, 0.8, new Animation(v => centerLabel.Opacity = v, 1, 0));
                animation.Add(0, 1, new Animation(v => imageGrid.Scale = v * imageGrid.Scale, 1, 0, easing: Easing.SpringIn));
            }

            animation.Commit(this, "HangedUpAnimation", length: 700);

            await Task.Delay(700);

            imageGrid.TranslationX = 0;
            imageGrid.TranslationY = 0;

            topLabelDrawable.TextOpacity = 0;
            IsVisible = true;

            topLabelGraphicsView.Invalidate();
        }

        class TopLabelDrawable : IDrawable
        {
            readonly float fontSize;
            readonly Color color;

            public string Text { get; set; }
            public SizeF TextSize { get; private set; }
            public float TextTranslationX { get; set; } = 0;
            public float TextOpacity { get; set; } = 0;
            public PointF TextPosition { get; set; }

            public event Action TextSizeUpdated;

            public TopLabelDrawable(float fontSize, Color color)
            {
                this.fontSize = fontSize;
                this.color = color;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                canvas.FontSize = fontSize;
                canvas.FontColor = new Color(color.Red, color.Green, color.Blue, TextOpacity);

                var newTextSize = canvas.GetStringSize(Text, Microsoft.Maui.Graphics.Font.Default, fontSize);
                if (TextSize != newTextSize)
                {
                    TextSize = newTextSize;
                    TextSizeUpdated?.Invoke();
                }
                var textRect = new RectF(TextTranslationX + TextPosition.X, TextPosition.Y, TextSize.Width * 2, TextSize.Height);

                canvas.ClipRectangle(new RectF(TextPosition.X, 0, textRect.Width, textRect.Height * 2));
                canvas.DrawString(Text, textRect, HorizontalAlignment.Left, VerticalAlignment.Center);

                canvas.ResetState(); // See: https://github.com/dotnet/Microsoft.Maui.Graphics/issues/405
            }
        }
    }
}
