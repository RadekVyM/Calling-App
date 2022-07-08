namespace CallingApp.Maui
{
    public static class StatusBar
    {
        public static double Height
        {
            get
            {
                return 0;
            }
        }

        public static Thickness Padding => new Thickness(0, Height, 0, 0);
    }
}
