using System.Windows;
using System.Windows.Media;

namespace NexaTweaks.App.Controls;

/// <summary>Minimal self-drawn sparkline - avoids pulling in a full charting library for one rolling line.</summary>
public sealed class Sparkline : FrameworkElement
{
    public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
        nameof(Values), typeof(IReadOnlyList<double>), typeof(Sparkline),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
        nameof(Stroke), typeof(Brush), typeof(Sparkline),
        new FrameworkPropertyMetadata(Brushes.CornflowerBlue, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
        nameof(MaxValue), typeof(double), typeof(Sparkline),
        new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public IReadOnlyList<double>? Values
    {
        get => (IReadOnlyList<double>?)GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double MaxValue
    {
        get => (double)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        var values = Values;
        if (values is null || values.Count < 2 || ActualWidth <= 0 || ActualHeight <= 0) return;

        var pen = new Pen(Stroke, 2) { LineJoin = PenLineJoin.Round, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var max = Math.Max(MaxValue, 1);
        var stepX = ActualWidth / (values.Count - 1);

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            var first = new Point(0, ActualHeight - (values[0] / max) * ActualHeight);
            ctx.BeginFigure(first, false, false);
            for (var i = 1; i < values.Count; i++)
            {
                var y = ActualHeight - Math.Clamp(values[i] / max, 0, 1) * ActualHeight;
                ctx.LineTo(new Point(i * stepX, y), true, false);
            }
        }
        geometry.Freeze();
        dc.DrawGeometry(null, pen, geometry);
    }
}
