using Microsoft.Maui.Controls.Shapes;

namespace CallingApp.Maui;

public class PathGeometryExtension : IMarkupExtension<Geometry>
{
    readonly PathGeometryConverter pathGeometryConverter = new();

    public string Path { get; set; }

    public Geometry ProvideValue(IServiceProvider serviceProvider) =>
        pathGeometryConverter.ConvertFromInvariantString(Path) as Geometry;

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) =>
        (this as IMarkupExtension<Geometry>).ProvideValue(serviceProvider);
}