using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DevPulse.Controls;

public sealed class HistoryChart : Control
{
    public static readonly StyledProperty<IEnumerable<double>?>
        ValuesProperty =
            AvaloniaProperty.Register<
                HistoryChart,
                IEnumerable<double>?>(nameof(Values));

    public IEnumerable<double>? Values
    {
        get => GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    static HistoryChart()
    {
        AffectsRender<HistoryChart>(ValuesProperty);
    }

    protected override void OnPropertyChanged(
        AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != ValuesProperty)
            return;

        var oldValues =
            change.GetOldValue<IEnumerable<double>?>();

        var newValues =
            change.GetNewValue<IEnumerable<double>?>();

        if (oldValues is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -=
                OnCollectionChanged;
        }

        if (newValues is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged +=
                OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var values = Values?.ToArray();

        if (values is null || values.Length < 2)
            return;

        var width = Bounds.Width;
        var height = Bounds.Height;

        if (width <= 0 || height <= 0)
            return;

        var pen = new Pen(Brushes.DodgerBlue, 2);

        for (var index = 1; index < values.Length; index++)
        {
            var previousPoint = CreatePoint(
                index - 1,
                values[index - 1],
                values.Length,
                width,
                height);

            var currentPoint = CreatePoint(
                index,
                values[index],
                values.Length,
                width,
                height);

            context.DrawLine(
                pen,
                previousPoint,
                currentPoint);
        }
    }

    private static Point CreatePoint(
        int index,
        double value,
        int count,
        double width,
        double height)
    {
        var x =
            index * width / (count - 1);

        var normalizedValue =
            Math.Clamp(value, 0, 100);

        var y =
            height - normalizedValue / 100 * height;

        return new Point(x, y);
    }
}