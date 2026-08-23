using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace DevPulse.Controls;

/// <summary>
/// Renders a percentage history as a line chart that reacts to collection changes.
/// </summary>
public sealed class HistoryChart : Control
{
    #region Public fields

    /// <summary>Identifies the styled <see cref="Values"/> property.</summary>
    public static readonly StyledProperty<IEnumerable<double>?>
        ValuesProperty =
            AvaloniaProperty.Register<
                HistoryChart,
                IEnumerable<double>?>(nameof(Values));

    #endregion

    #region Public properties

    /// <summary>Gets or sets the percentage samples rendered by the chart.</summary>
    public IEnumerable<double>? Values
    {
        get => GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    #endregion

    #region Private constructors

    /// <summary>Registers the chart properties that invalidate its rendering.</summary>
    static HistoryChart()
    {
        AffectsRender<HistoryChart>(ValuesProperty);
    }

    #endregion

    #region Protected methods

    /// <summary>Updates collection subscriptions when the bound values collection changes.</summary>
    /// <param name="change">Information about the changed Avalonia property.</param>
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

    /// <summary>Draws the current percentage history within the control bounds.</summary>
    /// <param name="context">The drawing context used to render the chart.</param>
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

    #endregion

    #region Private methods

    /// <summary>Requests a redraw after samples are added, removed, or replaced.</summary>
    /// <param name="sender">The collection that raised the notification.</param>
    /// <param name="e">Details of the collection change.</param>
    private void OnCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e)
    {
        InvalidateVisual();
    }

    /// <summary>Maps a percentage sample and its index to chart coordinates.</summary>
    /// <param name="index">The zero-based sample index.</param>
    /// <param name="value">The sample percentage.</param>
    /// <param name="count">The total number of rendered samples.</param>
    /// <param name="width">The available chart width.</param>
    /// <param name="height">The available chart height.</param>
    /// <returns>The point at which the sample should be rendered.</returns>
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

    #endregion
}
