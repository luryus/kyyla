using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Kyyla
{
    [MarkupExtensionReturnType(typeof(DrawingImage))]
    public class IconImageExtension : StaticResourceExtension
    {
        private static readonly FontFamily fontFamily
            = new FontFamily("Segoe MDL2 Assets");

        public int SymbolCode { get; set; }

        public double SymbolSize { get; set; } = 16;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var textBlock = new TextBlock
            {
                FontFamily = fontFamily,
                Text = char.ConvertFromUtf32(SymbolCode)
            };

            var brush = new VisualBrush
            {
                Visual = textBlock,
                Stretch = Stretch.Uniform
            };

            var drawing = new GeometryDrawing
            {
                Brush = brush,
                Geometry = new RectangleGeometry(
                    new Rect(0, 0, SymbolSize, SymbolSize))
            };

            return new DrawingImage(drawing);
        }
    }
}
