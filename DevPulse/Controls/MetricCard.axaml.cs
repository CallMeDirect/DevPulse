using Avalonia;
using Avalonia.Controls;

namespace DevPulse.Controls
{
    public partial class MetricCard : UserControl
    {
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<MetricCard, string>(nameof(Title), defaultValue: "CPU");
        public static readonly StyledProperty<string> ValueProperty =
            AvaloniaProperty.Register<MetricCard, string>(nameof(Value), defaultValue: "0%");
        public static readonly StyledProperty<double> PercentageProperty =
            AvaloniaProperty.Register<MetricCard, double>(nameof(Percentage), defaultValue: 0);

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
        
        public double Percentage
        {
            get => GetValue(PercentageProperty);
            set => SetValue(PercentageProperty, value);
        }

        public MetricCard()
        {
            InitializeComponent();
        }
    }
}
