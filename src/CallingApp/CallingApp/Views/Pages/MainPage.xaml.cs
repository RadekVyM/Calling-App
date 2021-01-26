using CallingApp.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;

namespace CallingApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new MainPageViewModel();
        }


        #region Private methods

        private async void CallButtonClicked(object sender, EventArgs e)
        {
            var ellipse = sender as Ellipse;

            Animation animation = new Animation();

            animation.Add(0, 0.7, new Animation(v => ellipse.Scale = v, 1, 1.2));
            animation.Add(0.8, 1, new Animation(v => ellipse.Scale = v, 1.2, 1));

            animation.Commit(ellipse, "EllipseAnimation", length: 200);
            await Task.Delay(100);

            HideAllViews();

            await Task.Delay(200);

            await callView.Call();
        }

        private void HideAllViews()
        {
            var frames = GetAllFrames(contentGrid);

            Animation animation = new Animation();

            animation.Add(0, 1, new Animation(v => whiteBoxView.TranslationY = v, 0, Height));
            animation.Add(0, 1, new Animation(v => whiteGradientBoxView.TranslationY = v, 0, Height));

            foreach (var frame in frames)
            {
                animation.Add(0, 1, new Animation(v => frame.Content.TranslationY = v, 0, frame.Content.Height));
            }

            animation.Commit(this, "HideContentAnimation", length: 500, easing: Easing.SinIn);
        }

        private IEnumerable<Frame> GetAllFrames(Layout layout)
        {
            List<Frame> frames = new List<Frame>();

            foreach(var child in layout.Children)
            {
                if (child is Frame frame)
                    frames.Add(frame);
                else if (child is Layout innerLayout)
                    frames.AddRange(GetAllFrames(innerLayout));
            }

            return frames;
        }

        #endregion

        #region Public methods

        public void ShowAllViews()
        {
            var frames = GetAllFrames(contentGrid);

            Animation animation = new Animation();

            animation.Add(0, 1, new Animation(v => whiteBoxView.TranslationY = v, Height, 0));
            animation.Add(0, 1, new Animation(v => whiteGradientBoxView.TranslationY = v, Height, 0));

            foreach (var frame in frames)
            {
                animation.Add(0, 1, new Animation(v => frame.Content.TranslationY = v, frame.Content.Height, 0));
            }

            animation.Commit(this, "ShowContentAnimation", length: 500, easing: Easing.SinIn);
        }

        #endregion
    }
}
