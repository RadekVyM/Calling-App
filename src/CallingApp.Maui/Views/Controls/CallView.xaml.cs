using CallingApp.Maui.Views.Pages;

namespace CallingApp.Maui.Views.Controls;

public partial class CallView : ContentView
{
    #region Private members
    
    const string MovingQuadrangleAndTimeAnimationName = "MovingQuadrangleAndTimeAnimation";
    const string TriangleToQuadrangleAnimationName = "TriangleToQuadrangleAnimation";
    const string TriangleAnimationName = "TriangleAnimation";
    const string QuadrangleCollapseAnimationName = "QuadrangleCollapseAnimation";
    const string IconsGridAnimationName = "IconsGridAnimation";
    const string MovingWaveAnimationName = "MovingWaveAnimation";
    const string WaveAnimationName = "WaveAnimation";
    const string StopWaveAnimationName = "StopWaveAnimation";

    bool initButtonsStackSizeChanged = true;
    bool initTimeGridSizeChanged = true;

    int numberOfPoints => 22;
    int numberOfHiddenPoints => 4;
    Point upVector;
    Point downVector;
    Point firstPointOffsetVector;
    Point firstPoint;
    Point pointToPointVector;
    float visibleLineLength => (float)Math.Sqrt(Math.Pow(overlayGraphicsView.Height, 2) + Math.Pow(overlayGraphicsView.Width, 2));
    float pointsDistance => visibleLineLength / (numberOfPoints - numberOfHiddenPoints - 1);
    bool isWaveAnimating;
    bool isOnThePhone = false;
    bool isHangedUp = true;
    double waveVectorScale = 0;
    double maxWaveVectorScale => 0.4f;
    Point overlayTopLeftPoint = new Point(0, 0);
    Point minOverlayTopLeftPoint => new Point(0, Math.Min(0.7d * overlayGraphicsView.Height, overlayGraphicsView.Height - 200));
    Point maxOverlayTopLeftPoint => new Point(0, minOverlayTopLeftPoint.Y + 20);
    Point overlayTopRightPoint = new Point(0, 0);
    Point minOverlayTopRightPoint => new Point(overlayGraphicsView.Width, (7d / 24d) * overlayGraphicsView.Height);
    Point maxOverlayTopRightPoint => new Point(overlayGraphicsView.Width, (2d / 5d) * overlayGraphicsView.Height);
    DateTime startTime = new DateTime();

    OverlayDrawable overlayDrawable;

    #endregion


    #region Constructor

    public CallView()
    {
        InitializeComponent();
    }

    #endregion

    #region Private methods

    private void OverlayGraphicsViewSizeChanged(object sender, EventArgs e)
    {
        float scale = pointsDistance / visibleLineLength;

        pointToPointVector = new Point(overlayGraphicsView.Width * scale, -overlayGraphicsView.Height * scale);
        firstPoint = new Point(-pointToPointVector.X * numberOfHiddenPoints, (-pointToPointVector.Y * numberOfHiddenPoints) + overlayGraphicsView.Height);
        firstPointOffsetVector = new Point(0, 0);
        upVector = new Point(0, 0);
        downVector = new Point(0, 0);

        App.Current.Resources.TryGetValue("OverlayColor", out object overlayColor);

        overlayDrawable = new OverlayDrawable(overlayColor as Color) { OverlayPath = new PathF() };

        overlayGraphicsView.Drawable = overlayDrawable;
        overlayGraphicsView.Invalidate();
    }

    private void ButtonsStackSizeChanged(object sender, EventArgs e)
    {
        if (initButtonsStackSizeChanged)
        {
            buttonsStack.IsVisible = false;
            initButtonsStackSizeChanged = false;
        }
    }

    private void TimeGridSizeChanged(object sender, EventArgs e)
    {
        if (initTimeGridSizeChanged)
        {
            timeGrid.IsVisible = false;
            initTimeGridSizeChanged = false;
        }
    }

    private async void HangUpViewInteracted(object sender, EventArgs e)
    {
        isHangedUp = true;

        // Abort all running animations
        overlayGraphicsView.AbortAnimation(MovingQuadrangleAndTimeAnimationName);
        overlayGraphicsView.AbortAnimation(TriangleToQuadrangleAnimationName);
        await StopWaveAnimation();

        await Task.Delay(200);

        this.InputTransparent = true;
        _ = hangUpView.Hide();
        _ = avatarView.OnPhoneHangedUp(isOnThePhone);
        GetMainPage(this)?.ShowAllViews();

        if (isOnThePhone)
        {
            await Task.WhenAll(
                StartCollapsingQuadrangleAnimation(800),
                StartDetailElementsAnimation(false));
            await Task.Delay(100);
        }
        else
        {
            await StartTriangleAnimation(false);
        }

        overlayGraphicsView.IsVisible = false;
        buttonsStack.IsVisible = false;
        timeGrid.IsVisible = false;
        isOnThePhone = false;
    }

    private async Task PickUpPhone()
    {
        await StopWaveAnimation();

        _ = avatarView.OnPhonePickedUp();

        _ = StartTriangleToQuadrangleAnimation();

        // Show all icons, buttons, time...
        _ = StartDetailElementsAnimation(true);

        await Task.Delay(900);

        StartMovingQuadrangleAndTimeAnimation(4000);
    }

    #region Start animation methods

    private void StartMovingWaveAnimation()
    {
        overlayGraphicsView.AbortAnimation(MovingWaveAnimationName);
        isWaveAnimating = true;

        Animation animation = new Animation();

        animation.Add(0, 1, new Animation(v =>
        {
            firstPointOffsetVector = new Point(pointToPointVector.X * v, pointToPointVector.Y * v);
            upVector = new Point(pointToPointVector.Y * waveVectorScale, -pointToPointVector.X * waveVectorScale);
            downVector = new Point(-pointToPointVector.Y * waveVectorScale, pointToPointVector.X * waveVectorScale);

            overlayDrawable.OverlayPath = GetOverlayWavePathF();
            overlayGraphicsView.Invalidate();
        }, 0, numberOfHiddenPoints));

        animation.Commit(overlayGraphicsView, MovingWaveAnimationName, length: 400, repeat: () => isWaveAnimating);
    }

    private void StartWaveAnimation()
    {
        overlayGraphicsView.AbortAnimation(WaveAnimationName);

        Animation animation = new Animation();

        animation.Add(0.1, 0.3, new Animation(v =>
        {
            waveVectorScale = v;
        }, 0, maxWaveVectorScale));

        animation.Add(0.7, 0.9, new Animation(v =>
        {
            waveVectorScale = v;
        }, maxWaveVectorScale, 0));

        animation.Commit(overlayGraphicsView, WaveAnimationName, length: 1200, repeat: () => isWaveAnimating);
    }

    private async Task StartCollapsingQuadrangleAnimation(uint lengthOfAnimation)
    {
        var bottomLeftPointOffsetY = 80;

        Animation animation = new Animation();

        animation.Add(0, 1, new Animation(v =>
        {
            overlayTopLeftPoint = new Point(maxOverlayTopLeftPoint.X, v);
        }, overlayTopLeftPoint.Y, overlayGraphicsView.Height + bottomLeftPointOffsetY));
        animation.Add(0, 1, new Animation(v =>
        {
            overlayTopRightPoint = new Point(maxOverlayTopRightPoint.X, v);
        }, overlayTopRightPoint.Y, overlayGraphicsView.Height + 20));
        animation.Add(0, 1, new Animation(v =>
        {
            overlayDrawable.OverlayPath = GetQuadrAnglePathF(bottomLeftPointOffsetY);
            overlayGraphicsView.Invalidate();
        }));

        animation.Commit(overlayGraphicsView, QuadrangleCollapseAnimationName, length: lengthOfAnimation, easing: Easing.CubicOut);

        await Task.Delay((int)lengthOfAnimation);
    }

    private void StartMovingQuadrangleAndTimeAnimation(uint lengthOfAnimationCycle = 4000)
    {
        var animation = CreateMovingQuadrangleAnimation();

        // Animation of the time element
        startTime = DateTime.Now;
        animation.Add(0, 1, new Animation(v =>
        {
            var timeSpan = DateTime.Now - startTime;
            string seconds = timeSpan.ToString("ss");
            string minutes = timeSpan.ToString("mm");

            if (seconds != secondsLabel.Text)
                secondsLabel.Text = seconds;
            if (minutes != minutesLabel.Text)
                minutesLabel.Text = minutes;
        }, 0, 1));

        animation.Commit(overlayGraphicsView, MovingQuadrangleAndTimeAnimationName, length: lengthOfAnimationCycle, repeat: () => isOnThePhone);
    }

    private async Task StartTriangleToQuadrangleAnimation(uint lengthOfAnimation = 800)
    {
        Animation animation = CreateTriangelToQuadrangleAnimation();

        animation.Commit(overlayGraphicsView, TriangleToQuadrangleAnimationName, length: lengthOfAnimation, easing: Easing.CubicOut);

        await Task.Delay((int)lengthOfAnimation);
    }

    private async Task StartTriangleAnimation(bool show, uint lengthOfAnimation = 800)
    {
        overlayGraphicsView.IsVisible = true;

        // Show/hide the gray triangular overlay
        Animation animation = new Animation(v =>
        {
            var path = new PathF();

            path.MoveTo(new Point(overlayGraphicsView.Width, overlayGraphicsView.Height));

            path.LineTo(new Point(overlayGraphicsView.Width, overlayGraphicsView.Height * (1 - (Math.Pow((1 - v), 2)))));
            path.LineTo(new Point(overlayGraphicsView.Width * v, overlayGraphicsView.Height));

            path.Close();

            overlayDrawable.OverlayPath = path;
            overlayGraphicsView.Invalidate();
        }, show ? 1 : 0, show ? 0 : 1);

        animation.Commit(overlayGraphicsView, TriangleAnimationName, length: lengthOfAnimation, easing: show ? Easing.CubicOut : Easing.CubicIn);

        await Task.Delay((int)lengthOfAnimation + 100);
    }

    private async Task StartDetailElementsAnimation(bool show, uint lengthOfAnimation = 700)
    {
        if (show)
        {
            // Set default values
            buttonsStack.IsVisible = true;
            buttonsStack.Opacity = 0;
            buttonsStack.TranslationY = -buttonsStack.Height;
            timeGrid.IsVisible = true;
            timeGrid.Opacity = 0;
            timeGrid.TranslationY = timeGrid.Height;

            secondsLabel.Text = "00";
            minutesLabel.Text = "00";
        }

        Animation animation = new Animation();

        // Show/hide buttons and the time element
        animation.Add(0, 1, new Animation(v => buttonsStack.Opacity = v, show ? 0 : 1, show ? 1 : 0));
        if (show)
            animation.Add(0, 1, new Animation(v => buttonsStack.TranslationY = v, show ? -buttonsStack.Height : 0, show ? 0 : -buttonsStack.Height));
        animation.Add(show ? 0.6 : 0, show ? 1 : 0.6, new Animation(v => timeGrid.Opacity = v, show ? 0 : 1, show ? 1 : 0));
        animation.Add(show ? 0.6 : 0, show ? 1 : 0.6, new Animation(v => timeGrid.TranslationY = v, show ? timeGrid.Height / 2 : 0, show ? 0 : timeGrid.Height / 2));

        animation.Commit(overlayGraphicsView, IconsGridAnimationName, length: lengthOfAnimation);

        await Task.Delay((int)lengthOfAnimation);

        if (!show)
        {
            buttonsStack.IsVisible = false;
            timeGrid.IsVisible = false;
        }
    }

    private async Task StopWaveAnimation()
    {
        overlayGraphicsView.AbortAnimation(WaveAnimationName);

        Animation animation = new Animation(v =>
        {
            waveVectorScale = v;
        }, waveVectorScale, 0);

        animation.Commit(overlayGraphicsView, StopWaveAnimationName, length: 240);
        await Task.Delay(250);

        overlayGraphicsView.AbortAnimation(MovingWaveAnimationName);

        isWaveAnimating = false;

        overlayGraphicsView.IsVisible = true;
    }

    #endregion

    #region Animation creation methods

    private Animation CreateTriangelToQuadrangleAnimation()
    {
        var animation = new Animation();

        // Animate the overlay from triangle to quadrangle
        animation.Add(0, 1, new Animation(v =>
        {
            overlayTopLeftPoint = new Point(maxOverlayTopLeftPoint.X, v);
        }, overlayGraphicsView.Height, maxOverlayTopLeftPoint.Y));
        animation.Add(0, 1, new Animation(v =>
        {
            overlayTopRightPoint = new Point(maxOverlayTopRightPoint.X, v);
        }, 0, maxOverlayTopRightPoint.Y));
        animation.Add(0, 1, new Animation(v =>
        {
            overlayDrawable.OverlayPath = GetQuadrAnglePathF();
            overlayGraphicsView.Invalidate();
        }));

        return animation;
    }

    private Animation CreateMovingQuadrangleAnimation()
    {
        var animation = new Animation();

        // Top left point up
        animation.Add(0.1, 0.5, new Animation(v =>
        {
            overlayTopLeftPoint = new Point(minOverlayTopLeftPoint.X, maxOverlayTopLeftPoint.Y - (v * (maxOverlayTopLeftPoint.Y - minOverlayTopLeftPoint.Y)));
        }, 0, 1));
        // Top right point up
        animation.Add(0, 0.4, new Animation(v =>
        {
            overlayTopRightPoint = new Point(minOverlayTopRightPoint.X, maxOverlayTopRightPoint.Y - (v * (maxOverlayTopRightPoint.Y - minOverlayTopRightPoint.Y)));
        }, 0, 1, easing: Easing.SinInOut));
        // Top left point down
        animation.Add(0.6, 1, new Animation(v =>
        {
            overlayTopLeftPoint = new Point(maxOverlayTopLeftPoint.X, maxOverlayTopLeftPoint.Y - (v * (maxOverlayTopLeftPoint.Y - minOverlayTopLeftPoint.Y)));
        }, 1, 0));
        // Top right point down
        animation.Add(0.5, 0.9, new Animation(v =>
        {
            overlayTopRightPoint = new Point(maxOverlayTopRightPoint.X, maxOverlayTopRightPoint.Y - (v * (maxOverlayTopRightPoint.Y - minOverlayTopRightPoint.Y)));
        }, 1, 0, easing: Easing.SinInOut));

        // Animation of the moving quadrangle - updating path
        animation.Add(0, 1, new Animation(v =>
        {
            overlayDrawable.OverlayPath = GetQuadrAnglePathF();
            overlayGraphicsView.Invalidate();
        }));

        return animation;
    }

    #endregion

    #region PathF methods

    private PathF GetOverlayWavePathF()
    {
        List<Point> points = GetAllWavePoints();

        var path = new PathF();

        path.MoveTo(firstPoint);
        path.LineTo(points[0]);

        for (int i = 0; i < points.Count; i++)
        {
            if (i + 1 == points.Count)
                break;

            if (i % 2 == 0)
                continue;

            Point fPoint = points[i];
            Point sPoint = points[i + 1];

            path.QuadTo(fPoint, sPoint);
        }

        Point lastPoint = new Point(overlayGraphicsView.Width + (pointToPointVector.X * numberOfHiddenPoints), pointToPointVector.X * numberOfHiddenPoints);

        path.LineTo(lastPoint);
        path.LineTo(new Point(lastPoint.X, firstPoint.Y));

        path.Close();

        return path;
    }

    private PathF GetQuadrAnglePathF(double bottomLeftPointOffsetY = 0)
    {
        var path = new PathF();

        path.MoveTo(new Point(overlayGraphicsView.Width, overlayGraphicsView.Height));

        path.LineTo(new Point(overlayTopRightPoint.X, overlayTopRightPoint.Y));
        path.LineTo(new Point(overlayTopLeftPoint.X, overlayTopLeftPoint.Y));
        path.LineTo(new Point(overlayTopLeftPoint.X, overlayGraphicsView.Height + bottomLeftPointOffsetY));

        path.Close();
        return path;
    }

    private List<Point> GetAllWavePoints()
    {
        List<Point> points = new List<Point>();

        bool up = true;
        bool onLine = true;

        for (int i = 0; i < numberOfPoints; i++)
        {
            Point point = new Point(firstPoint.X + (pointToPointVector.X * i) + firstPointOffsetVector.X, firstPoint.Y + (pointToPointVector.Y * i) + firstPointOffsetVector.Y);

            if (onLine)
            {
                points.Add(point);
                onLine = false;
            }
            else
            {
                if (up)
                {
                    points.Add(new Point(point.X + upVector.X, point.Y + upVector.Y));
                    up = false;
                    onLine = true;
                }
                else
                {
                    points.Add(new Point(point.X + downVector.X, point.Y + downVector.Y));
                    up = true;
                    onLine = true;
                }

                if (i == numberOfPoints - 1)
                    points.Add(new Point(firstPoint.X + (pointToPointVector.X * (i + 1)) + firstPointOffsetVector.X, firstPoint.Y + (pointToPointVector.Y * (i + 1)) + firstPointOffsetVector.Y));
            }
        }

        return points;
    }

    #endregion

    private MainPage GetMainPage(View view)
    {
        if (view.Parent.GetType() != typeof(App))
        {
            VisualElement parent = view;

            while (parent != null)
            {
                if (parent is MainPage mainPage)
                    return mainPage;
                else if (parent.Parent is ShellGroupItem || parent.Parent is App)
                    parent = null;
                else
                    parent = (VisualElement)parent.Parent;
            }
        }

        return null;
    }

    #endregion

    #region Public methods

    public async Task Call()
    {
        this.InputTransparent = false;
        isHangedUp = false;

        _ = hangUpView.Show();
        _ = avatarView.OnCalled();
        await StartTriangleAnimation(true);

        overlayGraphicsView.IsVisible = true;

        StartMovingWaveAnimation();
        StartWaveAnimation();

        for (int i = 0; i < 100; i++)
        {
            await Task.Delay(40);
            if (isHangedUp)
                return;
        }

        if (!isHangedUp)
        {
            isOnThePhone = true;
            await PickUpPhone();
        }
    }

    #endregion

    class OverlayDrawable : IDrawable
    {
        readonly Color overlayColor;

        public PathF OverlayPath { get; set; }


        public OverlayDrawable(Color overlayColor)
        {
            this.overlayColor = overlayColor;
        }


        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.SetFillPaint(new SolidPaint(overlayColor), dirtyRect);

            canvas.FillPath(OverlayPath);
        }
    }
}