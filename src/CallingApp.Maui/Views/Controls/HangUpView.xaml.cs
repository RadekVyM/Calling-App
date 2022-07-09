using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace CallingApp.Maui.Views.Controls
{
    public partial class HangUpView : ContentView
    {
        #region Private members

        bool initSizeChange = true;

        double arcRadius => Width / 6.5;
        double arcThickness => arcRadius / 2;
        double lastTotalY = 0;

        HangupDrawable drawable;

        #endregion

        #region Public members

        public event Action<object, EventArgs> Interacted;

        #endregion


        #region Constructor

        public HangUpView()
        {
            InitializeComponent();
        }

        #endregion


        #region Private methods

        private void GraphicsViewSizeChanged(object sender, EventArgs e)
        {
            float triangleWidth = (float)((2 * arcRadius) - (2 * arcThickness));
            float triangleHeight = triangleWidth / 2;

            var defaulArcPosition = new Point((Width / 2) - arcRadius, (Height / 2) - arcRadius);
            var defaultTopTrianglePosition = new Point(defaulArcPosition.X + arcThickness, defaulArcPosition.Y + arcRadius + 1);
            var defaultBottomTrianglePosition = new Point(defaultTopTrianglePosition.X, defaultTopTrianglePosition.Y + triangleHeight);

            App.Current.Resources.TryGetValue("OverlayColor", out object overlayColor);

            drawable = new HangupDrawable(
                (float)arcRadius,
                (float)arcThickness,
                triangleWidth,
                triangleHeight,
                defaulArcPosition,
                defaultTopTrianglePosition,
                defaultBottomTrianglePosition,
                overlayColor as Color);
            
            graphicsView.Drawable = drawable;

            graphicsView.Invalidate();

            if (initSizeChange)
            {
                IsVisible = false;
                initSizeChange = false;
            }    
        }

        private async void PanUpdated(object sender, PanUpdatedEventArgs e)
        {
#if IOS || ANDROID
            switch (e.StatusType)
            {
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    if (lastTotalY > ellipse.Height / 3)
                        Interacted?.Invoke(sender, e);

                    await Task.Delay(250);

                    Animation animation = new Animation();

                    animation.Add(0, 1, new Animation(v => drawable.ArcTranslationY = (float)v, drawable.ArcTranslationY, 0));
                    animation.Add(0, 1, new Animation(v => drawable.TopTriangleTranslationY = (float)v, drawable.TopTriangleTranslationY, 0));
                    animation.Add(0, 1, new Animation(v => drawable.BottomTriangleTranslationY = (float)v, drawable.BottomTriangleTranslationY, 0));
                    animation.Add(0, 1, new Animation(v => drawable.TopTriangleOpacity = (float)v, drawable.TopTriangleOpacity, 1));
                    animation.Add(0, 1, new Animation(v => drawable.BottomTriangleOpacity = (float)v, drawable.BottomTriangleOpacity, 1));
                    animation.Add(0, 1, new Animation(v => graphicsView.Invalidate(), 0, 1));

                    animation.Commit(this, "ToDefaultStateAnimation");

                    await Task.Delay(250);

                    drawable.ArcTranslationY = 0;
                    drawable.TopTriangleTranslationY = 0;
                    drawable.BottomTriangleTranslationY = 0;
                    drawable.TopTriangleOpacity = 1;
                    drawable.BottomTriangleOpacity = 1;
                    graphicsView.Invalidate();
                    break;
                case GestureStatus.Running:
                    lastTotalY = e.TotalY;

                    float y = e.TotalY < 0 ? 0f : (float)(e.TotalY > ellipse.Height / 2 ? ellipse.Height / 2 : e.TotalY);
                    float scale = (float)(y / (ellipse.Height / 2));

                    float offset = (float)((2 * arcRadius) - (2 * arcThickness)) / 3;

                    drawable.ArcTranslationY = -offset * scale;
                    drawable.TopTriangleTranslationY = offset * scale;
                    drawable.BottomTriangleTranslationY = (offset * 2) * scale;
                    drawable.TopTriangleOpacity = 1 - (scale * 0.3f);
                    drawable.BottomTriangleOpacity = 1 - (scale * 0.5f);

                    graphicsView.Invalidate();
                    break;
            }
#endif
        }

        private void Tapped(object sender, EventArgs e)
        {
#if WINDOWS || MACCATALYST
            Interacted?.Invoke(sender, e);
#endif
        }

        #endregion

        #region Public methods

        public async Task Show()
        {
            IsVisible = true;
            ellipse.StrokeThickness = 0;
            this.Scale = 1.5;

            drawable.ArcOpacity = 0;
            drawable.ArcRotation = 0;
            drawable.TopTriangleScale = 0;
            drawable.BottomTriangleScale = 0;
            graphicsView.Invalidate();

            Animation animation = new Animation();

            animation.Add(0, 0.7, new Animation(v =>
            {
                this.Scale = v;
                ellipse.Clip = new EllipseGeometry { Center = new Point(Width / 2, Height / 2), RadiusX = Width / 2, RadiusY = Height / 2 };
            }, 1.4, 1));
            animation.Add(0, 0.7, new Animation(v => ellipse.StrokeThickness = v, 0, HeightRequest / 2));
            animation.Add(0.3, 0.5, new Animation(v => drawable.ArcOpacity = (float)v, 0, 1));
            animation.Add(0, 1, new Animation(v => drawable.ArcRotation = (float)v, 0, 360));
            animation.Add(0.8, 1, new Animation(v => drawable.TopTriangleScale = (float)v, 0, 1));
            animation.Add(0.6, 1, new Animation(v => drawable.BottomTriangleScale = (float)v, 0, 1));
            animation.Add(0, 1, new Animation(v => graphicsView.Invalidate(), 0, 1));

            animation.Commit(this, "ShowAnimation", length: 700);

            await Task.Delay(700);

            drawable.ArcOpacity = 1;
            drawable.ArcRotation = 0;
            drawable.TopTriangleScale = 1;
            drawable.BottomTriangleScale = 1;
            graphicsView.Invalidate();
        }

        public async Task Hide()
        {
            Animation animation = new Animation();

            animation.Add(0, 1, new Animation(v => this.Scale = v, 1, 0, easing: Easing.SpringIn));

            animation.Commit(this, "HideAnimation", length: 600);

            await Task.Delay(600);

            IsVisible = false;
        }

        #endregion

        class HangupDrawable : IDrawable
        {
            readonly float arcRadius;
            readonly float arcThickness;
            readonly float triangleWidth;
            readonly float triangleHeight;
            readonly PointF defaultArcPosition;
            readonly PointF defaultTopTrianglePosition;
            readonly PointF defaultBottomTrianglePosition;
            readonly Color primaryColor;
            readonly PathF defaultArcPath;
            readonly PathF defaultTopTrianglePath;
            readonly PathF defaultBottomTrianglePath;

            public float ArcOpacity { get; set; } = 1;
            public float TopTriangleOpacity { get; set; } = 1;
            public float BottomTriangleOpacity { get; set; } = 1;
            public float ArcRotation { get; set; } = 0;
            public float ArcTranslationY { get; set; } = 0;
            public float TopTriangleTranslationY { get; set; } = 0;
            public float BottomTriangleTranslationY { get; set; } = 0;
            public float TopTriangleScale { get; set; } = 1;
            public float BottomTriangleScale { get; set; } = 1;


            public HangupDrawable(float arcRadius, float arcThickness, float triangleWidth, float triangleHeight, PointF defaultArcPosition, PointF defaultTopTrianglePosition, PointF defaultBottomTrianglePosition, Color primaryColor)
            {
                this.arcRadius = arcRadius;
                this.arcThickness = arcThickness;
                this.triangleWidth = triangleWidth;
                this.triangleHeight = triangleHeight;
                this.primaryColor = primaryColor;
                this.defaultArcPosition = defaultArcPosition;
                this.defaultTopTrianglePosition = defaultTopTrianglePosition;
                this.defaultBottomTrianglePosition = defaultBottomTrianglePosition;

                defaultArcPath = CreateDefaultArcPath();
                defaultTopTrianglePath = CreateDefaultTopTrianglePath();
                defaultBottomTrianglePath = CreateDefaultBottomTrianglePath();
            }


            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                var arcPath = new PathF(defaultArcPath);
                arcPath.Move(0, ArcTranslationY);
                arcPath = arcPath.Rotate(ArcRotation, new Point(defaultArcPosition.X + (defaultArcPath.Bounds.Width / 2), defaultTopTrianglePosition.Y + ArcTranslationY));

                var topTrianglePath = defaultTopTrianglePath.AsScaledPath(TopTriangleScale);
                topTrianglePath.Move(
                    defaultTopTrianglePosition.X - topTrianglePath.Bounds.X - (topTrianglePath.Bounds.Width / 2) + (defaultTopTrianglePath.Bounds.Width / 2),
                    defaultTopTrianglePosition.Y - topTrianglePath.Bounds.Y + TopTriangleTranslationY);

                var bottomTrianglePath = defaultBottomTrianglePath.AsScaledPath(BottomTriangleScale);
                bottomTrianglePath.Move(
                    defaultBottomTrianglePosition.X - bottomTrianglePath.Bounds.X - (bottomTrianglePath.Bounds.Width / 2) + (defaultBottomTrianglePath.Bounds.Width / 2),
                    defaultBottomTrianglePosition.Y - bottomTrianglePath.Bounds.Y + BottomTriangleTranslationY);

                canvas.SetFillPaint(new SolidColorBrush(Color.FromRgba(primaryColor.Red, primaryColor.Green, primaryColor.Blue, ArcOpacity)), dirtyRect);
                canvas.FillPath(arcPath);

                canvas.SetFillPaint(new SolidColorBrush(Color.FromRgba(primaryColor.Red, primaryColor.Green, primaryColor.Blue, TopTriangleOpacity)), dirtyRect);
                canvas.FillPath(topTrianglePath);

                canvas.SetFillPaint(new SolidColorBrush(Color.FromRgba(primaryColor.Red, primaryColor.Green, primaryColor.Blue, BottomTriangleOpacity)), dirtyRect);
                canvas.FillPath(bottomTrianglePath);
            }

            private PathF CreateDefaultArcPath()
            {
                var path = new PathF();
                path.MoveTo(new Point(0, arcRadius));
                path.QuadTo(new Point(0, 0), new Point(arcRadius, 0));
                path.QuadTo(new Point(2 * arcRadius, 0), new Point(2 * arcRadius, arcRadius));
                path.LineTo(new Point((2 * arcRadius) - arcThickness, arcRadius));
                path.QuadTo(new Point((2 * arcRadius) - arcThickness, arcThickness), new Point(arcRadius, arcThickness));
                path.QuadTo(new Point(arcThickness, arcThickness), new Point(arcThickness, arcRadius));
                path.Close();
                path.Move(defaultArcPosition.X, defaultArcPosition.Y);
                return path;
            }

            private PathF CreateDefaultTrianglePath()
            {
                var path = new PathF();

                path.MoveTo(new Point(0, 0));
                path.LineTo(new Point(triangleWidth, 0));
                path.LineTo(new Point(arcRadius - arcThickness, triangleHeight));
                path.Close();

                return path;
            }

            private PathF CreateDefaultTopTrianglePath()
            {
                var path = CreateDefaultTrianglePath();
                path.Move(defaultTopTrianglePosition.X, defaultTopTrianglePosition.Y);
                return path;
            }

            private PathF CreateDefaultBottomTrianglePath()
            {
                var path = CreateDefaultTrianglePath();
                path.Move(defaultBottomTrianglePosition.X, defaultBottomTrianglePosition.Y);
                return path;
            }
        }
    }
}
