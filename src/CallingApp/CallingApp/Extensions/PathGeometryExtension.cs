using System;
using Xamarin.Forms.Shapes;
using Xamarin.Forms.Xaml;

namespace CallingApp
{
    public class PathGeometryExtension : IMarkupExtension<Geometry>
    {
        PathGeometryConverter PathGeometryConverter = new PathGeometryConverter();

        public string Path { get; set; }

        public Geometry ProvideValue(IServiceProvider serviceProvider)
        {
            return PathGeometryConverter.ConvertFromInvariantString(Path) as Geometry;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return (this as IMarkupExtension<Geometry>).ProvideValue(serviceProvider);
        }
    }
}
