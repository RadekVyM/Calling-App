using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;
using Xamarin.Forms.Xaml;

namespace CallingApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HangUpView : ContentView
    {
        #region Private members

        double arcRadius => Width / 6.5;
        double arcThickness => arcRadius / 2;
        Point defaultArcPosition;
        Point defaultTopTrianglePosition;
        Point defaultBottomTrianglePosition;
        double lastTotalY = 0;

        #endregion

        #region Public methods

        public event Action<object, EventArgs> Swiped;

        #endregion


        #region Constructor

        public HangUpView()
        {
            defaultArcPosition = new Point(0, 0);
            defaultTopTrianglePosition = new Point(0, 0);
            defaultBottomTrianglePosition = new Point(0, 0);

            InitializeComponent();
        }

        #endregion


        #region Private methods

        private void AbsoluteLayoutSizeChanged(object sender, EventArgs e)
        {
            arcPath.Data = new PathGeometry
            {
                Figures = new PathFigureCollection
                {
                    new PathFigure
                    {
                        IsClosed = true, IsFilled = true, StartPoint = new Point(0, arcRadius),
                        Segments = new PathSegmentCollection
                        {
                            new QuadraticBezierSegment(new Point(0, 0), new Point(arcRadius, 0)),
                            new QuadraticBezierSegment(new Point(2 * arcRadius, 0), new Point(2 * arcRadius, arcRadius)),
                            new LineSegment(new Point((2 * arcRadius) - arcThickness, arcRadius)),
                            new QuadraticBezierSegment(new Point((2 * arcRadius) - arcThickness, arcThickness), new Point(arcRadius, arcThickness)),
                            new QuadraticBezierSegment(new Point(arcThickness, arcThickness), new Point(arcThickness, arcRadius))
                        }
                    }
                }
            };
            defaultArcPosition = new Point((Width / 2) - arcRadius, (Height / 2) - arcRadius);
            arcPath.LayoutTo(new Xamarin.Forms.Rectangle(defaultArcPosition, new Size(2 * arcRadius, arcRadius)), 1);

            double triangleWidth = (2 * arcRadius) - (2 * arcThickness);
            double triangleHeight = triangleWidth / 2;

            topTrianglePath.Data = bottomTrianglePath.Data = new PathGeometry
            {
                Figures = new PathFigureCollection
                {
                    new PathFigure
                    {
                        IsClosed = true, IsFilled = true, StartPoint = new Point(0, 0),
                        Segments = new PathSegmentCollection
                        {
                            new LineSegment(new Point(triangleWidth, 0)),
                            new LineSegment(new Point(arcRadius - arcThickness, triangleHeight))
                        }
                    }
                }
            };
            defaultTopTrianglePosition = new Point(defaultArcPosition.X + arcThickness, defaultArcPosition.Y + arcRadius + 1);
            topTrianglePath.LayoutTo(new Xamarin.Forms.Rectangle(defaultTopTrianglePosition, new Size(triangleWidth, triangleHeight)), 1);

            defaultBottomTrianglePosition = new Point(defaultTopTrianglePosition.X, defaultTopTrianglePosition.Y + triangleHeight);
            bottomTrianglePath.LayoutTo(new Xamarin.Forms.Rectangle(defaultBottomTrianglePosition, new Size(triangleWidth, triangleHeight)), 1);
        }

        private async void PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch(e.StatusType)
            {
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    if (lastTotalY > ellipse.Height / 2)
                        Swiped?.Invoke(sender, e);

                    await Task.Delay(250);

                    Animation animation = new Animation();
                    animation.Add(0, 1, new Animation(v => arcPath.TranslationY = v, arcPath.TranslationY, 0));
                    animation.Add(0, 1, new Animation(v => topTrianglePath.TranslationY = v, topTrianglePath.TranslationY, 0));
                    animation.Add(0, 1, new Animation(v => bottomTrianglePath.TranslationY = v, bottomTrianglePath.TranslationY, 0));
                    animation.Add(0, 1, new Animation(v => topTrianglePath.Opacity = v, topTrianglePath.Opacity, 1));
                    animation.Add(0, 1, new Animation(v => bottomTrianglePath.Opacity = v, bottomTrianglePath.Opacity, 1));
                    animation.Commit(absoluteLayout, "ToDefaultStateAnimation");
                    break;
                case GestureStatus.Running:
                    lastTotalY = e.TotalY;

                    double y = e.TotalY < 0 ? 0 : e.TotalY > ellipse.Height / 2 ? ellipse.Height / 2 : e.TotalY;
                    double scale = y / (ellipse.Height / 2);

                    double offset = ((2 * arcRadius) - (2 * arcThickness)) / 3;

                    arcPath.TranslationY = -offset * scale;
                    topTrianglePath.TranslationY = offset * scale;
                    bottomTrianglePath.TranslationY = (offset * 2) * scale;
                    topTrianglePath.Opacity = 1 - (scale * 0.3);
                    bottomTrianglePath.Opacity = 1 - (scale * 0.5);
                    break;
            }
        }

        #endregion

        #region Public methods

        public async Task Show()
        {
            ellipse.StrokeThickness = 0;
            this.Scale = 1.5;
            IsVisible = true;
            arcPath.Opacity = 0;
            topTrianglePath.Scale = 0;
            bottomTrianglePath.Scale = 0;

            Animation animation = new Animation();

            animation.Add(0, 0.7, new Animation(v => 
            {
                this.Scale = v;
                ellipse.Clip = new EllipseGeometry { Center = new Point(Width / 2, Height / 2), RadiusX = Width / 2, RadiusY = Height / 2 };
            }, 1.4, 1));
            animation.Add(0, 0.7, new Animation(v =>
            {
                ellipse.StrokeThickness = v;
            }, 0, HeightRequest / 2));
            animation.Add(0.3, 0.5, new Animation(v =>
            {
                arcPath.Opacity = v;
            }, 0, 1));
            animation.Add(0, 1, new Animation(v =>
            {
                arcPath.Rotation = v;
            }, 0, 360));
            animation.Add(0.8, 1, new Animation(v =>
            {
                topTrianglePath.Scale = v;
            }, 0, 1));
            animation.Add(0.6, 1, new Animation(v =>
            {
                bottomTrianglePath.Scale = v;
            }, 0, 1));

            animation.Commit(ellipse, "ShowAnimation", length: 700);

            await Task.Delay(700);

            arcPath.Rotation = 0;
        }

        public async Task Hide()
        {
            Animation animation = new Animation();

            animation.Add(0, 1, new Animation(v =>
            {
                this.Scale = v;
            }, 1, 0, easing: Easing.SpringIn));

            animation.Commit(ellipse, "HideAnimation", length: 600);

            await Task.Delay(600);

            IsVisible = false;
        }

        #endregion
    }
}