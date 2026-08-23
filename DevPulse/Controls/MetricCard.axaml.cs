using Avalonia;
using Avalonia.Controls;

namespace DevPulse.Controls
{
    /// <summary>
    /// Displays a resource name, formatted value, and percentage indicator.
    /// </summary>
    public partial class MetricCard : UserControl
    {
        #region Public fields

        /// <summary>Identifies the styled <see cref="Title"/> property.</summary>
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<MetricCard, string>(nameof(Title), defaultValue: "CPU");

        /// <summary>Identifies the styled <see cref="Value"/> property.</summary>
        public static readonly StyledProperty<string> ValueProperty =
            AvaloniaProperty.Register<MetricCard, string>(nameof(Value), defaultValue: "0%");

        /// <summary>Identifies the styled <see cref="Percentage"/> property.</summary>
        public static readonly StyledProperty<double> PercentageProperty =
            AvaloniaProperty.Register<MetricCard, double>(nameof(Percentage), defaultValue: 0);

        #endregion

        #region Public properties

        /// <summary>Gets or sets the resource name displayed by the card.</summary>
        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>Gets or sets the human-readable resource value.</summary>
        public string Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        
        /// <summary>Gets or sets the utilization percentage shown by the progress bar.</summary>
        public double Percentage
        {
            get => GetValue(PercentageProperty);
            set => SetValue(PercentageProperty, value);
        }

        #endregion

        #region Public constructors

        /// <summary>Initializes a new instance of the <see cref="MetricCard"/> control.</summary>
        public MetricCard()
        {
            InitializeComponent();
        }

        #endregion
    }
}
