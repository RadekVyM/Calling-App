using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CallingApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AvatarView : ContentView
    {
        #region Private members

        Point imageCenterPosition => new Point((Width - imageGrid.Width) / 2, (Height - imageGrid.Height) / 2);
        Point imageTopPosition = new Point(0,0);
        Point topLabelPosition = new Point(0,0);
        double topImageSize => 20;
        double topImageScale => topImageSize / imageGrid.Width;

        #endregion


        #region Constructor

        public AvatarView()
        {
            InitializeComponent();
        }

        #endregion


        #region Private methods

        private void UpdatePositions(object sender, EventArgs e)
        {
            double spacing = 8;
            double verticalCenter = StatusBar.Height + 15;
            double totalWidth = topImageSize + topLabel.Width + spacing;

            imageTopPosition = new Point(((Width - totalWidth) / 2) - ((imageGrid.Width - topImageSize) / 2), verticalCenter - (topImageSize / 2) - ((imageGrid.Height - topImageSize) / 2));
            topLabelPosition = new Point(((Width - totalWidth) / 2) + topImageSize + spacing, verticalCenter - (topLabel.Height / 2));

            AbsoluteLayout.SetLayoutBounds(topLabelFrame, new Rectangle(topLabelPosition, new Size(300, topLabel.Height)));
            AbsoluteLayout.SetLayoutBounds(imageGrid, new Rectangle(imageCenterPosition, imageGrid.Bounds.Size));
            topLabel.TranslationX = -topLabel.Width;
        }

        #endregion

        #region Public methods

        public async Task OnCalled()
        {
            IsVisible = true;

            await Task.Delay(100);

            Animation animation = new Animation();

            animation.Add(0, 0.6, new Animation(v => imageGrid.Opacity = v, 0, 1));
            animation.Add(0, 1, new Animation(v => imageGrid.Scale = v, 1.5, 1));
            animation.Add(0, 1, new Animation(v => centerLabel.Opacity = v, 0, 1));

            animation.Commit(this, "ShowAnimation", length: 700);

            await Task.Delay(700);
        }

        public async Task OnPhonePickedUp()
        {
            topLabel.Opacity = 1;
            topLabel.TranslationX = -topLabel.Width;

            Animation animation = new Animation();

            animation.Add(0.5, 1, new Animation(v => topLabel.TranslationX = v, topLabel.TranslationX, 0));
            animation.Add(0, 0.8, new Animation(v => centerLabel.Opacity = v, 1, 0));
            animation.Add(0, 1, new Animation(v => imageGrid.Scale = v, imageGrid.Scale, topImageScale));
            animation.Add(0, 1, new Animation(v => imageGrid.TranslationX = v, 0, imageTopPosition.X - imageGrid.X));
            animation.Add(0, 1, new Animation(v => imageGrid.TranslationY = v, 0, imageTopPosition.Y - imageGrid.Y));

            animation.Commit(this, "TranslationAnimation", length: 700);

            await Task.Delay(700);
        }

        public async Task OnPhoneHangedUp(bool isOnThePhone)
        {
            Animation animation = new Animation();

            animation.Add(0.4, 1, new Animation(v => imageGrid.Opacity = v, 1, 0));

            if (isOnThePhone)
            {
                animation.Add(0.5, 1, new Animation(v => topLabel.TranslationX = v, topLabel.TranslationX, -topLabel.Width));
            }
            else
            {
                animation.Add(0, 0.8, new Animation(v => centerLabel.Opacity = v, 1, 0));
                animation.Add(0, 1, new Animation(v => imageGrid.Scale = v, imageGrid.Scale, 0, easing: Easing.SpringIn));
            }

            animation.Commit(this, "HangedUpAnimation", length: 700);

            await Task.Delay(700);

            imageGrid.TranslationX = 0;
            imageGrid.TranslationY = 0;

            topLabel.Opacity = 0;
            IsVisible = true;
        }

        #endregion
    }
}