using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevPulse.Controls
{
    public partial class MetricCard : UserControl
    {
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<MetricCard, string>(nameof(Title), defaultValue: "CPU");
        public static readonly StyledProperty<string> ValueProperty =
            AvaloniaProperty.Register<MetricCard, string>(nameof(Value), defaultValue: "67%");

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public MetricCard()
        {
            InitializeComponent();
        }
    }
}
