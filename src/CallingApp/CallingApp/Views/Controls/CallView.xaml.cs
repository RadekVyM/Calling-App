using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;
using Xamarin.Forms.Xaml;

namespace CallingApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CallView : ContentView
    {
        #region Private members

        int numberOfPoints => 22;
        int numberOfHiddenPoints => 4;
        SKPoint upVector;
        SKPoint downVector;
        SKPoint firstPointOffsetVector;
        SKPoint firstPoint;
        SKPoint pointToPointVector;
        float visibleLineLength => (float)Math.Sqrt(Math.Pow(heightPx, 2) + Math.Pow(widthPx, 2));
        float entireLineLength => visibleLineLength + (pointsDistance * numberOfHiddenPoints);
        float pointsDistance => visibleLineLength / (numberOfPoints - numberOfHiddenPoints - 1);
        float heightPx => (float)(canvasView.Height * DeviceDisplay.MainDisplayInfo.Density);
        float widthPx => (float)(canvasView.Width * DeviceDisplay.MainDisplayInfo.Density);
        bool isWaveAnimating;
        bool isOnThePhone = false;
        bool isHangedUp = true;
        SKPaint paint;
        float waveVectorScale = 0;
        float maxWaveVectorScale => 0.4f;
        Point overlayTopLeftPoint = new Point(0, 0);
        Point minOverlayTopLeftPoint => new Point(0, 0.7d * Height);
        Point maxOverlayTopLeftPoint => new Point(0, minOverlayTopLeftPoint.Y + 20);
        Point overlayTopRightPoint = new Point(0, 0);
        Point minOverlayTopRightPoint => new Point(Width, (7d / 24d) * Height);
        Point maxOverlayTopRightPoint => new Point(Width, (2d / 5d) * Height);
        DateTime startTime = new DateTime();

        #endregion


        #region Constructor

        public CallView()
        {
            paint = new SKPaint
            {
                Color = App.Current.Resources.GetValue<Color>("OverlayColor").ToSKColor(),
                StrokeWidth = 4,
                Style = SKPaintStyle.Fill
            };

            InitializeComponent();
        }

        #endregion


        #region Private methods

        #region SKCanvas methods

        private void CanvasViewSizeChanged(object sender, EventArgs e)
        {
            float scale = pointsDistance / (float)Math.Sqrt(Math.Pow(heightPx, 2) + Math.Pow(widthPx, 2));

            pointToPointVector = new SKPoint(widthPx * scale, -heightPx * scale);
            firstPoint = new SKPoint(-pointToPointVector.X * numberOfHiddenPoints, (-pointToPointVector.Y * numberOfHiddenPoints) + heightPx);
            firstPointOffsetVector = new SKPoint(0, 0);
            upVector = new SKPoint(0, 0);
            downVector = new SKPoint(0, 0);
        }

        private void CanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            canvas.Clear();

            upVector = new SKPoint(pointToPointVector.Y * waveVectorScale, -pointToPointVector.X * waveVectorScale);
            downVector = new SKPoint(-pointToPointVector.Y * waveVectorScale, pointToPointVector.X * waveVectorScale);

            canvas.DrawPath(GetPath(), paint);
        }

        private List<SKPoint> GetPoints()
        {
            List<SKPoint> points = new List<SKPoint>();

            bool up = true;
            bool onLine = true;

            for (int i = 0; i < numberOfPoints; i++)
            {
                SKPoint point = new SKPoint(firstPoint.X + (pointToPointVector.X * i) + firstPointOffsetVector.X, firstPoint.Y + (pointToPointVector.Y * i) + firstPointOffsetVector.Y);

                if (onLine)
                {
                    points.Add(point);
                    onLine = false;
                }
                else
                {
                    if (up)
                    {
                        points.Add(point + upVector);
                        up = false;
                        onLine = true;
                    }
                    else
                    {
                        points.Add(point + downVector);
                        up = true;
                        onLine = true;
                    }

                    if (i == numberOfPoints - 1)
                        points.Add(new SKPoint(firstPoint.X + (pointToPointVector.X * (i + 1)) + firstPointOffsetVector.X, firstPoint.Y + (pointToPointVector.Y * (i + 1)) + firstPointOffsetVector.Y));
                }
            }

            return points;
        }

        private SKPath GetPath()
        {
            List<SKPoint> points = GetPoints();
            SKPath path = new SKPath();

            path.MoveTo(firstPoint);
            path.LineTo(points[0]);

            for (int i = 0; i < points.Count; i++)
            {
                if (i + 1 == points.Count)
                    break;

                if (i % 2 == 0)
                    continue;

                SKPoint fPoint = points[i];
                SKPoint sPoint = points[i + 1];

                path.QuadTo(fPoint, sPoint);
            }

            SKPoint lastPoint = new SKPoint(widthPx + (pointToPointVector.X * numberOfHiddenPoints), pointToPointVector.X * numberOfHiddenPoints);

            path.LineTo(lastPoint);
            path.LineTo(lastPoint.X, firstPoint.Y);
            path.Close();

            return path;
        }

        #endregion

        #region Wave animation methods

        private void StartMovingWaveAnimation()
        {
            canvasView.AbortAnimation("MovingWaveAnimation");
            isWaveAnimating = true;

            Animation animation = new Animation();

            animation.Add(0, 1, new Animation(v =>
            {
                firstPointOffsetVector = new SKPoint(pointToPointVector.X * (float)v, pointToPointVector.Y * (float)v);
                canvasView.InvalidateSurface();
            }, 0, numberOfHiddenPoints));

            animation.Commit(canvasView, "MovingWaveAnimation", length: 400, repeat: () => isWaveAnimating);
        }

        private void StartWaveAnimation()
        {
            canvasView.AbortAnimation("WaveAnimation");

            Animation animation = new Animation();

            animation.Add(0.1, 0.3, new Animation(v =>
            {
                waveVectorScale = (float)v;
            }, 0, maxWaveVectorScale));

            animation.Add(0.7, 0.9, new Animation(v =>
            {
                waveVectorScale = (float)v;
            }, maxWaveVectorScale, 0));

            animation.Commit(canvasView, "WaveAnimation", length: 1200, repeat: () => isWaveAnimating);
        }

        private async Task StopWaveAnimation()
        {
            canvasView.AbortAnimation("WaveAnimation");

            Animation animation = new Animation(v =>
            {
                waveVectorScale = (float)v;
            }, waveVectorScale, 0);

            animation.Commit(canvasView, "StopWaveAnimation", length: 240);
            await Task.Delay(250);

            canvasView.AbortAnimation("MovingWaveAnimation");

            isWaveAnimating = false;

            overlayPath.IsVisible = true;
            canvasView.IsVisible = false;
        }

        #endregion

        private async void HangUpViewSwiped(object sender, EventArgs e)
        {
            isHangedUp = true;

            // Abort all running animations
            overlayPath.AbortAnimation("RectAnimation");
            overlayPath.AbortAnimation("TriangleToRectAnimation");
            await StopWaveAnimation();

            await Task.Delay(200);

            this.InputTransparent = true;
            _ = hangUpView.Hide();
            _ = avatarView.OnPhoneHangedUp(isOnThePhone);
            GetMainPage(this)?.ShowAllViews();

            if (isOnThePhone)
            {
                Animation animation = new Animation();

                // Hide the gray overlay
                animation.Add(0, 1, new Animation(v =>
                {
                    overlayTopLeftPoint = new Point(maxOverlayTopLeftPoint.X, v);
                }, overlayTopLeftPoint.Y, this.Height + 80));
                animation.Add(0, 1, new Animation(v =>
                {
                    overlayTopRightPoint = new Point(maxOverlayTopRightPoint.X, v);
                }, overlayTopRightPoint.Y, this.Height + 20));
                animation.Add(0, 1, new Animation(v =>
                {
                    overlayPath.Data = new PathGeometry
                    {
                        Figures = new PathFigureCollection
                        {
                            new PathFigure
                            {
                                IsClosed = true, IsFilled = true, StartPoint = new Point(this.Width, this.Height),
                                Segments = new PathSegmentCollection
                                {
                                    new LineSegment(new Point(overlayTopRightPoint.X, overlayTopRightPoint.Y)),
                                    new LineSegment(new Point(overlayTopLeftPoint.X, overlayTopLeftPoint.Y)),
                                    new LineSegment(new Point(overlayTopLeftPoint.X, this.Height + 80))
                                }
                            }
                        }
                    };
                }));

                animation.Commit(overlayPath, "TriangleCollapseAnimation", length: 800, easing: Easing.CubicOut);

                // Hide all icons, buttons, time...
                _ = AnimateDetailElements(false);

                await Task.Delay(900);
            }
            else
            {
                // Hide the gray overlay
                await AnimateTriangle(false);
            }

            overlayPath.IsVisible = false;
            buttonsGrid.IsVisible = false;
            timeGrid.IsVisible = false;
            isOnThePhone = false;
        }

        private async Task PickUpPhone()
        {
            await StopWaveAnimation();

            _ = avatarView.OnPhonePickedUp();

            Animation startAnimation = new Animation();

            // Animate the overlay from triangle to quadrangle
            startAnimation.Add(0, 1, new Animation(v =>
            {
                overlayTopLeftPoint = new Point(maxOverlayTopLeftPoint.X, v);
            }, this.Height, maxOverlayTopLeftPoint.Y));
            startAnimation.Add(0, 1, new Animation(v =>
            {
                overlayTopRightPoint = new Point(maxOverlayTopRightPoint.X, v);
            }, 0, maxOverlayTopRightPoint.Y));
            startAnimation.Add(0, 1, new Animation(v =>
            {
                overlayPath.Data = new PathGeometry
                {
                    Figures = new PathFigureCollection
                    {
                        new PathFigure
                        {
                            IsClosed = true, IsFilled = true, StartPoint = new Point(this.Width, this.Height),
                            Segments = new PathSegmentCollection
                            {
                                new LineSegment(new Point(overlayTopRightPoint.X, overlayTopRightPoint.Y)),
                                new LineSegment(new Point(overlayTopLeftPoint.X, overlayTopLeftPoint.Y)),
                                new LineSegment(new Point(overlayTopLeftPoint.X, this.Height))
                            }
                        }
                    }
                };
            }));

            startAnimation.Commit(overlayPath, "TriangleToRectAnimation", length: 800, easing: Easing.CubicOut);

            // Show all icons, buttons, time...
            _ = AnimateDetailElements(true);

            await Task.Delay(900);

            Animation animation = new Animation();

            // Animation of the moving quadrangle - setting points
            animation.Add(0.1, 0.5, new Animation(v =>
            {
                overlayTopLeftPoint = new Point(minOverlayTopLeftPoint.X, v);
            }, maxOverlayTopLeftPoint.Y, minOverlayTopLeftPoint.Y));
            animation.Add(0, 0.4, new Animation(v =>
            {
                overlayTopRightPoint = new Point(minOverlayTopRightPoint.X, v);
            }, maxOverlayTopRightPoint.Y, minOverlayTopRightPoint.Y, easing: Easing.SinInOut));
            animation.Add(0.6, 1, new Animation(v =>
            {
                overlayTopLeftPoint = new Point(maxOverlayTopLeftPoint.X, v);
            }, minOverlayTopLeftPoint.Y, maxOverlayTopLeftPoint.Y));
            animation.Add(0.5, 0.9, new Animation(v =>
            {
                overlayTopRightPoint = new Point(maxOverlayTopRightPoint.X, v);
            }, minOverlayTopRightPoint.Y, maxOverlayTopRightPoint.Y, easing: Easing.SinInOut));

            // Animation of the moving quadrangle - updating shape
            animation.Add(0, 1, new Animation(v =>
            {
                overlayPath.Data = new PathGeometry
                {
                    Figures = new PathFigureCollection
                    {
                        new PathFigure
                        {
                            IsClosed = true, IsFilled = true, StartPoint = new Point(this.Width, this.Height),
                            Segments = new PathSegmentCollection
                            {
                                new LineSegment(new Point(overlayTopRightPoint.X, overlayTopRightPoint.Y)),
                                new LineSegment(new Point(overlayTopLeftPoint.X, overlayTopLeftPoint.Y)),
                                new LineSegment(new Point(overlayTopLeftPoint.X, this.Height))
                            }
                        }
                    }
                };
            }));

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

            animation.Commit(overlayPath, "RectAnimation", length: 4000, repeat: () => isOnThePhone);
        }

        private async Task AnimateDetailElements(bool show)
        {
            if (show)
            {
                // Set default values
                buttonsGrid.IsVisible = true;
                buttonsGrid.Opacity = 0;
                buttonsGrid.TranslationY = -buttonsGrid.Height;
                timeGrid.IsVisible = true;
                timeGrid.Opacity = 0;
                timeGrid.TranslationY = timeGrid.Height;

                secondsLabel.Text = "00";
                minutesLabel.Text = "00";
            }

            Animation animation = new Animation();

            // Show/hide buttons and the time element
            animation.Add(0, 1, new Animation(v => buttonsGrid.Opacity = v, show ? 0 : 1, show ? 1 : 0));
            if (show)
                animation.Add(0, 1, new Animation(v => buttonsGrid.TranslationY = v, show ? -buttonsGrid.Height : 0, show ? 0 : -buttonsGrid.Height));
            animation.Add(show ? 0.6 : 0, show ? 1 : 0.6, new Animation(v => timeGrid.Opacity = v, show ? 0 : 1, show ? 1 : 0));
            animation.Add(show ? 0.6 : 0, show ? 1 : 0.6, new Animation(v => timeGrid.TranslationY = v, show ? timeGrid.Height / 2 : 0, show ? 0 : timeGrid.Height / 2));

            animation.Commit(overlayPath, "IconsGridAnimation", length: 700);

            await Task.Delay(700);

            if (!show)
            {
                buttonsGrid.IsVisible = false;
                timeGrid.IsVisible = false;
            }
        }

        private async Task AnimateTriangle(bool show)
        {
            overlayPath.IsVisible = true;

            // Show/hide the gray triangular overlay
            Animation animation = new Animation(v =>
            {
                overlayPath.Data = new PathGeometry
                {
                    Figures = new PathFigureCollection
                    {
                        new PathFigure
                        {
                            IsClosed = true, IsFilled = true, StartPoint = new Point(this.Width, this.Height),
                            Segments = new PathSegmentCollection
                            {
                                new LineSegment(new Point(this.Width, this.Height * (1 - (Math.Pow((1 - v), 2))))),
                                new LineSegment(new Point(this.Width * v, this.Height))
                            }
                        }
                    }
                };
            }, show ? 1 : 0, show ? 0 : 1);

            animation.Commit(overlayPath, "ToTriangleAnimation", length: 800, easing: show ? Easing.CubicOut : Easing.CubicIn);

            await Task.Delay(900);
        }

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
            await AnimateTriangle(true);

            canvasView.IsVisible = true;
            overlayPath.IsVisible = false;

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
    }
}