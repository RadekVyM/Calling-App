using CallingApp.Maui.Views.Pages;
using Microsoft.Maui.Controls.Shapes;

namespace CallingApp.Maui.Views.Controls
{
    public partial class CallView : ContentView
    {
        #region Private members

        int numberOfPoints => 22;
        int numberOfHiddenPoints => 4;
        Point upVector;
        Point downVector;
        Point firstPointOffsetVector;
        Point firstPoint;
        Point pointToPointVector;
        double visibleLineLength => (double)Math.Sqrt(Math.Pow(overlayPath.Height, 2) + Math.Pow(overlayPath.Width, 2));
        double entireLineLength => visibleLineLength + (pointsDistance * numberOfHiddenPoints);
        double pointsDistance => visibleLineLength / (numberOfPoints - numberOfHiddenPoints - 1);
        bool isWaveAnimating;
        bool isOnThePhone = false;
        bool isHangedUp = true;
        double waveVectorScale = 0;
        double maxWaveVectorScale => 0.4f;
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
            InitializeComponent();
        }

        #endregion


        #region Private methods

        #region Canvas methods

        private void CanvasViewSizeChanged(object sender, EventArgs e)
        {
            double scale = pointsDistance / (double)Math.Sqrt(Math.Pow(overlayPath.Height, 2) + Math.Pow(overlayPath.Width, 2));

            pointToPointVector = new Point(overlayPath.Width * scale, -overlayPath.Height * scale);
            firstPoint = new Point(-pointToPointVector.X * numberOfHiddenPoints, (-pointToPointVector.Y * numberOfHiddenPoints) + overlayPath.Height);
            firstPointOffsetVector = new Point(0, 0);
            upVector = new Point(0, 0);
            downVector = new Point(0, 0);
        }

        // TODO: Use GraphicsView for overlay
        private List<Point> GetPoints()
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

        private PathGeometry GetPath()
        {
            List<Point> points = GetPoints();

            PathFigure pathFigure = new PathFigure
            {
                IsClosed = true,
                IsFilled = true,
                StartPoint = firstPoint,
                Segments = new PathSegmentCollection
                {
                    new LineSegment(points[0])
                }
            };

            for (int i = 0; i < points.Count; i++)
            {
                if (i + 1 == points.Count)
                    break;

                if (i % 2 == 0)
                    continue;

                Point fPoint = points[i];
                Point sPoint = points[i + 1];

                pathFigure.Segments.Add(new QuadraticBezierSegment(fPoint, sPoint));
            }

            Point lastPoint = new Point(overlayPath.Width + (pointToPointVector.X * numberOfHiddenPoints), pointToPointVector.X * numberOfHiddenPoints);

            pathFigure.Segments.Add(new LineSegment(lastPoint));
            pathFigure.Segments.Add(new LineSegment(new Point(lastPoint.X, firstPoint.Y)));

            return new PathGeometry
            {
                Figures = new PathFigureCollection { pathFigure }
            };
        }

        #endregion

        #region Wave animation methods

        private void StartMovingWaveAnimation()
        {
            overlayPath.AbortAnimation("MovingWaveAnimation");
            isWaveAnimating = true;

            Animation animation = new Animation();

            animation.Add(0, 1, new Animation(v =>
            {
                firstPointOffsetVector = new Point(pointToPointVector.X * v, pointToPointVector.Y * v);
                upVector = new Point(pointToPointVector.Y * waveVectorScale, -pointToPointVector.X * waveVectorScale);
                downVector = new Point(-pointToPointVector.Y * waveVectorScale, pointToPointVector.X * waveVectorScale);

                overlayPath.Data = GetPath();
            }, 0, numberOfHiddenPoints));

            animation.Commit(overlayPath, "MovingWaveAnimation", length: 400, repeat: () => isWaveAnimating);
        }

        private void StartWaveAnimation()
        {
            overlayPath.AbortAnimation("WaveAnimation");

            Animation animation = new Animation();

            animation.Add(0.1, 0.3, new Animation(v =>
            {
                waveVectorScale = v;
            }, 0, maxWaveVectorScale));

            animation.Add(0.7, 0.9, new Animation(v =>
            {
                waveVectorScale = v;
            }, maxWaveVectorScale, 0));

            animation.Commit(overlayPath, "WaveAnimation", length: 1200, repeat: () => isWaveAnimating);
        }

        private async Task StopWaveAnimation()
        {
            overlayPath.AbortAnimation("WaveAnimation");

            Animation animation = new Animation(v =>
            {
                waveVectorScale = v;
            }, waveVectorScale, 0);

            animation.Commit(overlayPath, "StopWaveAnimation", length: 240);
            await Task.Delay(250);

            overlayPath.AbortAnimation("MovingWaveAnimation");

            isWaveAnimating = false;

            overlayPath.IsVisible = true;
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

            overlayPath.IsVisible = true;

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
