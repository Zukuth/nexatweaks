using System.Windows;
using System.Windows.Media;

namespace NexaTweaks.App.Controls;

/// <summary>Simple self-drawn circular progress gauge (0-100) - used to show a "before vs after" optimization score without pulling in a charting library.</summary>
public sealed class RadialGauge : FrameworkElement
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(double), typeof(RadialGauge),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TrackBrushProperty = DependencyProperty.Register(
        nameof(TrackBrush), typeof(Brush), typeof(RadialGauge),
        new FrameworkPropertyMetadata(Brushes.DarkGray, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ValueBrushProperty = DependencyProperty.Register(
        nameof(ValueBrush), typeof(Brush), typeof(RadialGauge),
        new FrameworkPropertyMetadata(Brushes.CornflowerBlue, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
        nameof(StrokeThickness), typeof(double), typeof(RadialGauge),
        new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Brush TrackBrush
    {
        get => (Brush)GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    public Brush ValueBrush
    {
        get => (Brush)GetValue(ValueBrushProperty);
        set => SetValue(ValueBrushProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        var size = Math.Min(ActualWidth, ActualHeight);
        if (size <= 0) return;

        var thickness = StrokeThickness;
        var radius = (size - thickness) / 2;
        if (radius <= 0) return;
        var center = new Point(ActualWidth / 2, ActualHeight / 2);

        var trackPen = new Pen(TrackBrush, thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        dc.DrawEllipse(null, trackPen, center, radius, radius);

        var percent = Math.Clamp(Value, 0, 100);
        var angleDeg = percent / 100.0 * 359.9; // just under a full circle so the arc geometry stays well-defined
        if (angleDeg < 0.5) return;

        var valuePen = new Pen(ValueBrush, thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var startAngleRad = -Math.PI / 2;
        var endAngleRad = startAngleRad + angleDeg * Math.PI / 180;
        var startPoint = new Point(center.X + radius * Math.Cos(startAngleRad), center.Y + radius * Math.Sin(startAngleRad));
        var endPoint = new Point(center.X + radius * Math.Cos(endAngleRad), center.Y + radius * Math.Sin(endAngleRad));
        var isLargeArc = angleDeg > 180;

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(startPoint, false, false);
            ctx.ArcTo(endPoint, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise, true, false);
        }
        geometry.Freeze();
        dc.DrawGeometry(null, valuePen, geometry);
    }
}
