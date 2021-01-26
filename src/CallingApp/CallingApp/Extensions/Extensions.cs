using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace CallingApp
{
    public static class Extensions
    {
        public static string GetHexString(this Color color)
        {
            var red = (int)(color.R * 255);
            var green = (int)(color.G * 255);
            var blue = (int)(color.B * 255);
            var alpha = (int)(color.A * 255);
            var hex = $"#{alpha:X2}{red:X2}{green:X2}{blue:X2}";

            return hex;
        }

        public static T GetValue<T>(this ResourceDictionary dictionary, string key)
        {
            object value;

            dictionary.TryGetValue(key, out value);

            return (T)value;
        }

        public static Color GetColour(this object value)
        {
            if (value == null)
                return Color.Transparent;
            if (value.ToString() == "")
                return Color.Transparent;
            if (value is Color color)
                return color;
            else if (value is DynamicResource resource)
                return App.Current.Resources.GetValue<Color>(resource.Key);
            else if (value is string code)
                return Color.FromHex(code);
            else
                return Color.Transparent;
        }
    }
}
