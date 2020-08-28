using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace PressureSensorTest
{
    /// <summary>
    /// Interaction logic for LedIndicator.xaml
    /// </summary>
    public partial class LedIndicatorUC : UserControl
    {
        Brush defaultColor = Brushes.LightSteelBlue;

        public LedIndicatorUC()
        {
            InitializeComponent();
            Led.Fill = defaultColor;
        }

        public StateIndicator StateInd
        {
            get { return (StateIndicator)GetValue(StateIndProperty); }
            set { SetValue(StateIndProperty, value); }
        }

        public static readonly DependencyProperty StateIndProperty =
            DependencyProperty.Register("StateInd", typeof(StateIndicator), typeof(LedIndicatorUC),
            new UIPropertyMetadata(StateIndicator.Disabled, UpdStateInd));

        private static void UpdStateInd(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (LedIndicatorUC)d;
            var state = (StateIndicator)e.NewValue;
            switch (state)
            {
                case StateIndicator.Disabled:
                    obj.Led.Fill = obj.defaultColor;
                    break;
                case StateIndicator.Ok:
                    obj.Led.Fill = Brushes.Lime;
                    break;
                case StateIndicator.Warning:
                    obj.Led.Fill = Brushes.Yellow;
                    break;
                case StateIndicator.Error:
                    obj.Led.Fill = Brushes.Tomato;
                    break;
            }
        }
    }

    public enum StateIndicator { Disabled = -1, Ok = 0, Error, Warning }
}
